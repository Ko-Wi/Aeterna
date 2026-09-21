using System.Collections;
using UnityEngine;
using PathologicalGames;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnData
    {
        public Transform spawnPoint;       // 몬스터가 생성될 위치
        public PathRoute pathRoute;         // 몬스터가 이동할 경로
        public int spawnPointIndex;         // 이동을 시작할 경로 지점
    }

    /********************************** 싱 글 톤 *******************************************/

    private static SpawnManager _instance;
    public static SpawnManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SpawnManager>();

                if (_instance == null)
                {
                    Debug.LogError("SpawnManager instance is null. Please ensure an instance of UIManager is present in the scene.");
                }
            }
            return _instance;
        }
    }
    /*************************************************************************************/

    private MyObject myChar;
    private UiManager uiManger;
    // 일반 몬스터와 보스를 관리하는 풀
    private SpawnPool monsterPool;

    [Header("스테이지 등급별 몬스터 데이터")]

    // Normal, Heroic, Demigod, Titan StageData를 등록
    [SerializeField] private StageData[] stageDatas;

    [Header("몬스터 소환 설정")]

    // 생성된 몬스터가 들어갈 부모 오브젝트
    [SerializeField] private Transform monsterParent;

    // 몬스터를 생성할 위치와 이동 경로 목록
    [SerializeField] private SpawnData[] spawnDatas;

    // 한 스테이지에서 사용할 일반 몬스터 종류 수
    [SerializeField, Min(1)] private int monsterKindsPerStage = 2;

    private const int RoundsPerWave = 20; // 웨이브당 소환 횟수
    private const int LastWave = 30;      // 마지막 웨이브

    private Coroutine spawnCoroutine;   // 실행 중인 소환 코루틴
    private bool isSpawning;             // 일반 몬스터 소환 진행 여부
    private bool bossSpawnAttempted;     // 보스 중복 소환 방지

    [Header("자동 소환 설정")]

    // 몬스터가 반복 소환되는 시간 간격
    [Min(0.01f)] public float spawnInterval = 1f;

    [Header("몬스터 크기 설정")]
    [SerializeField] private float normalMonsterScale = 0.2f; // 일반 몬스터 크기
    [SerializeField] private float bossMonsterScale = 0.4f;   // 보스 몬스터 크기


    [Header("소환 지점 확장")]
    [SerializeField, Min(1)] private int twoPointStartStage = 10;
    [SerializeField, Min(1)] private int threePointStartStage = 50;


    // 해당 몬스터의 풀을 처음 준비하거나 보충할 때 생성할 개수
    private const int PoolBatchSize = 10;

    private void Awake()
    {
        // 스테이지와 현재 몬스터 수를 관리하는 myChar 가져오기
        myChar = MyObject.MyChar;
        uiManger = UiManager.Instance;
    }

    private void Start()
    {
        // 새로운 스테이지 시작 시 현재 몬스터 수 초기화
        myChar.ResetMonsterCount();

        // 몬스터 자동 소환 시작
        StartSpawning();
    }

    // 최초 시작 또는 스테이지 진행을 다시 시작할 때 호출
    public void StartSpawning()
    {
        // 기존 코루틴이 있으면 중지하여 중복 소환 방지
        StopSpawning();

        if (myChar.CurrentWave < 1 || myChar.CurrentWave > LastWave)
        {
            Debug.LogError($"현재 웨이브가 잘못되었습니다: {myChar.CurrentWave}", this);
            return;
        }

        // 저장된 웨이브는 유지하고 해당 스테이지의 소환 횟수로 초기화
        myChar.CurrentRound = GetRoundsPerWave();

        bossSpawnAttempted = false;
        isSpawning = true;

        spawnCoroutine = StartCoroutine(AutoSpawn());
    }
    public void StopSpawning()
    {
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private void OnDisable()
    {
        // 매니저가 비활성화되면 자동 소환 중지
        StopSpawning();
    }

    // 몬스터 풀이 없으면 생성
    private void CreateMonsterPool()
    {
        if (monsterPool != null)
            return;

        // 이미 생성된 풀이 있으면 재사용
        if (PoolManager.Pools.ContainsKey("MonsterPool"))
        {
            monsterPool = PoolManager.Pools["MonsterPool"];
            return;
        }

        monsterPool = PoolManager.Pools.Create("MonsterPool");

        // 기존 몬스터 부모 아래에 풀 배치
        monsterPool.group.SetParent(monsterParent != null ? monsterParent : transform, false);
    }

    // 현재 스테이지 등급에 맞는 일반 몬스터를 모든 소환 지점에 생성
    public void SpawnAll()
    {
        if (!isSpawning || bossSpawnAttempted || myChar.CurrentRound <= 0)
            return;

        GameObject prefab = GetCurrentMonsterPrefab();

        // 소환 묶음이 성공한 경우에만 라운드 감소
        if (!SpawnMonsters(prefab, normalMonsterScale, false))
        {
            isSpawning = false;

            Debug.LogError("일반 몬스터 소환 실패로 자동 소환을 중지합니다.", this);
            return;
        }


        // 이번 소환으로 최대 몬스터 수를 초과했는지 확인
        if (myChar.CurrentMonsterCount > myChar.MaxEnemyCnt)
        {
            RetreatStage();
            return; // 후퇴 후 기존 소환 처리가 이어지지 않도록 종료
        }

        // 정상적으로 소환했다면 라운드 감소
        myChar.CurrentRound--;

        if (myChar.CurrentRound > 0) 
            return;

        if (myChar.CurrentWave < LastWave)
        {
            // 다음 웨이브 시작
            myChar.CurrentWave++;
            myChar.CurrentRound = GetRoundsPerWave();
        }
        else
        {
            // 30웨이브의 마지막 소환을 완료하면 보스 소환
            SpawnBoss();
        }
    }

    // 현재 스테이지에 해당하는 보스를 기존 소환 지점마다 생성
    public void SpawnBoss()
    {
        // 중복 호출 방지
        if (bossSpawnAttempted)
            return;

        // 마지막 웨이브의 일반 몬스터 소환이 끝난 경우만 허용
        if (myChar.CurrentWave != LastWave ||
            myChar.CurrentRound != 0)
        {
            return;
        }

        bossSpawnAttempted = true;
        isSpawning = false;

        GameObject prefab = GetCurrentBossPrefab();

        if (!SpawnMonsters(prefab, bossMonsterScale, true))
        {
            Debug.LogError("보스 소환에 실패했습니다.", this);
        }

        // 웨이브 30, 라운드 0을 유지한 채 자동 소환 종료
    }

    // 전달받은 프리팹을 기존 spawnDatas의 위치와 경로에 맞춰 생성
    private bool SpawnMonsters(GameObject monsterPrefab, float monsterScale, bool isBoss)
    {
        if (monsterPrefab == null || spawnDatas == null)
            return false;

        if (monsterPrefab.GetComponent<MonsterController>() == null)
        {
            Debug.LogError($"{monsterPrefab.name}에 MonsterController가 없습니다.", this);
            return false;
        }

        // 보스는 첫 번째 소환 지점에서 한 마리만 소환
        // 일반 몬스터는 스테이지에 따라 여러 지점 사용
        int pointCount = isBoss ? Mathf.Min(1, spawnDatas.Length) : GetActiveSpawnPointCount();

        if (pointCount == 0)
            return false;

        // 소환 전에 사용할 지점을 모두 검사
        for (int i = 0; i < pointCount; i++)
        {
            SpawnData data = spawnDatas[i];

            if (data == null || data.spawnPoint == null || data.pathRoute == null || data.pathRoute.PointCount == 0)
            {
                Debug.LogError($"소환 지점 {i + 1}의 위치 또는 경로가 잘못되었습니다.", this);
                return false;
            }
        }

        //CreateMonsterPool();
        // 처음에는 50마리 준비, 이후 부족할 때마다 50마리 추가 , 보스 여부를 전달하여 풀 준비 개수도 구분
        if (!PrepareMonsterPool(monsterPrefab, pointCount, isBoss))
            return false;

        // 사용하는 지점마다 한 마리씩 소환
        for (int i = 0; i < pointCount; i++)
        {
            SpawnData data = spawnDatas[i];

            Transform monster = monsterPool.Spawn(monsterPrefab.transform, data.spawnPoint.position, Quaternion.identity);

            if (monster == null)
                return false;

            monster.localScale = Vector3.one * monsterScale;

            MonsterController controller = monster.GetComponent<MonsterController>();

            controller._enemyCategory = isBoss ? EnemyCategory.Boss : EnemyCategory.Nomal;

            // 기존 임시 체력 설정 유지
            controller.maxHp = 100;

            controller.Init(data.pathRoute, data.spawnPointIndex, monsterPool);

            // 초기화가 완료된 보스만 UI에 등록
            if (isBoss)
            {
                uiManger.RegisterBoss(controller);
            }
        }

        // 모든 지점의 소환 완료
        return true;
    }

    // 현재 스테이지 등급과 일치하는 StageData를 찾아서 반환
    private StageData GetCurrentStageData()
    {
        // StageData가 하나도 등록되지 않았다면 null 반환
        if (stageDatas == null || stageDatas.Length == 0)
            return null;

        // 등록된 StageData를 순서대로 확인
        for (int i = 0; i < stageDatas.Length; i++)
        {
            // 비어 있는 데이터는 건너뜀
            if (stageDatas[i] == null)
                continue;

            // 현재 등급과 일치하는 StageData를 찾으면 반환
            if (stageDatas[i].Tier == myChar.CurrentStageTier)
                return stageDatas[i];
        }

        // 현재 등급에 해당하는 데이터를 찾지 못했다면 오류 표시
        Debug.LogError($"{myChar.CurrentStageTier} 등급의 StageData가 등록되지 않았습니다.");
        return null;
    }


    // 현재 등급, 스테이지, 라운드에 등장할 일반 몬스터 프리팹 반환
    private GameObject GetCurrentMonsterPrefab()
    {
        // 현재 등급에 맞는 StageData 가져오기
        StageData currentStageData = GetCurrentStageData();

        // 현재 등급의 데이터가 없다면 몬스터를 선택할 수 없음
        if (currentStageData == null)
            return null;

        // 현재 등급에 일반 몬스터가 등록되지 않았다면 소환할 수 없음
        if (currentStageData.MonsterList == null || currentStageData.MonsterList.Count == 0)
            return null;

        // 스테이지가 1보다 작아지는 상황 방지
        int currentStage = Mathf.Max(1, myChar.CurrentStage);

        // 남은 라운드가 아닌 현재 웨이브로 몬스터 종류 결정
        int currentWave = Mathf.Clamp(myChar.CurrentWave, 1, LastWave);

        /*
         * 현재 스테이지에서 사용할 첫 번째 몬스터 인덱스 계산
         *
         * 1스테이지 → 0
         * 2스테이지 → 2
         * 3스테이지 → 4
         */
        int firstMonsterIndex = (currentStage - 1) * monsterKindsPerStage;

        // 2종 기준: 1~15웨이브는 첫 번째, 16~30웨이브는 두 번째
        int waveMonsterIndex = (currentWave - 1) * monsterKindsPerStage / LastWave;

        // 스테이지 시작 인덱스와 라운드 몬스터 순번을 합침
        int monsterIndex = firstMonsterIndex + waveMonsterIndex;

        // 목록의 마지막을 넘으면 해당 등급의 첫 번째 몬스터부터 다시 순환
        monsterIndex %= currentStageData.MonsterList.Count;

        // 계산된 일반 몬스터 프리팹 반환
        return currentStageData.MonsterList[monsterIndex];
    }


    // 현재 등급과 스테이지에 등장할 보스 프리팹 반환
    public GameObject GetCurrentBossPrefab()
    {
        // 현재 등급에 맞는 StageData 가져오기
        StageData currentStageData = GetCurrentStageData();

        // 현재 등급의 데이터가 없다면 보스를 선택할 수 없음
        if (currentStageData == null)
            return null;

        // 현재 등급에 보스가 등록되지 않았다면 소환할 수 없음
        if (currentStageData.BossList == null || currentStageData.BossList.Count == 0)
            return null;

        // 스테이지가 1보다 작아지는 상황 방지
        int currentStage = Mathf.Max(1, myChar.CurrentStage);

        /*
         * 스테이지마다 보스를 순서대로 선택
         *
         * 1스테이지  → 0번 보스
         * 2스테이지  → 1번 보스
         * 40스테이지 → 39번 보스
         * 41스테이지 → 다시 0번 보스
         */
        int bossIndex = (currentStage - 1) % currentStageData.BossList.Count;

        // 계산된 보스 프리팹 반환
        return currentStageData.BossList[bossIndex];
    }

    //스테이지에서 몇마리씩 소환하는지 체크하는 부분
    private int GetActiveSpawnPointCount()
    {
        if (spawnDatas == null)
            return 0;

        int count = 1;

        if (myChar.CurrentStage >= threePointStartStage)
            count = 3;
        else if (myChar.CurrentStage >= twoPointStartStage)
            count = 2;

        return Mathf.Min(count, spawnDatas.Length);
    }

    // 이번 소환에 필요한 개체를 풀에 준비
    private bool PrepareMonsterPool(GameObject monsterPrefab, int requiredCount, bool isBoss)
    {
        // 일반 몬스터는 10마리, 보스는 1마리씩 준비
        int batchSize = isBoss ? 1 : PoolBatchSize;

        CreateMonsterPool();

        PrefabPool prefabPool = monsterPool.GetPrefabPool(monsterPrefab);

        if (prefabPool == null)
        {
            // 처음 사용하는 몬스터 종류라면 전용 풀 등록
            prefabPool = new PrefabPool(monsterPrefab.transform);

            // 50마리를 즉시 생성한 뒤 비활성 상태로 보관
            prefabPool.preloadAmount = batchSize;
            prefabPool.preloadTime = false;

            // 생성 개수 제한은 사용하지 않음
            prefabPool.limitInstances = false;

            monsterPool.CreatePrefabPool(prefabPool);
        }

        // 현재 필드에 나온 개체가 아니라,
        // 반환되어 재사용을 기다리는 개체 수를 확인
        int availableCount = prefabPool.despawned.Count;

        while (availableCount < requiredCount)
        {
            // 기존 풀에 제한 설정이 있더라도 사용하지 않도록 설정
            prefabPool.limitInstances = false;

            // 부족하면 50마리 단위로 추가 준비
            for (int i = 0; i < batchSize; i++)
            {
                // 대기 중인 개체를 꺼내지 않고 새 개체를 생성
                Transform instance = prefabPool.SpawnNew();

                if (instance == null)
                {
                    Debug.LogError($"{monsterPrefab.name}의 풀 준비에 실패했습니다.", this);

                    return false;
                }

                // 실제 전투용 Init()은 호출하지 않고 풀에 반환
                monsterPool.Despawn(instance);
                availableCount++;
            }
        }

        return true;
    }
    // 보스 시간 초과에 대한 조건 검사
    public void HandleBossTimeout(MonsterController boss, uint spawnVersion)
    {
        if (boss == null || boss.SpawnVersion != spawnVersion 
            || boss._enemyCategory != EnemyCategory.Boss || !boss.IsTargetable)
        {
            return;
        }

        RetreatStage();
    }
    // 보스 시간 초과, 몬스터 수 초과 등 공통 실패 처리
    public void RetreatStage()
    {
        //※※※※※나중에 UI효과로 변경하고 작동하게 할 예정※※※※※
        StopSpawning();

        // 보스 타이머와 참조부터 정리
        if (uiManger != null)
            uiManger.ClearBoss();

        // 필드에 있는 모든 몬스터를 풀로 회수
        if (monsterPool != null)
            monsterPool.DespawnAll();

        myChar.ResetMonsterCount();

        // 같은 등급에서 3웨이브 후퇴, 최소 1웨이브
        myChar.CurrentWave = Mathf.Max(1, myChar.CurrentWave - 3);

        // 기존 규칙: 웨이브 유지, 라운드는 20으로 초기화
        StartSpawning();
    }
    // 현재 등급과 스테이지에 맞는 웨이브당 소환 횟수
    private int GetRoundsPerWave()
    {
        if (myChar.CurrentStageTier == StageTier.Normal)
        {
            switch (myChar.CurrentStage)
            {
                case 1:
                    return 5;

                case 2:
                    return 10;

                case 3:
                case 4:
                    return 15;
            }
        }

        // 노멀 5스테이지 이상과 다른 등급은 기본 20회
        return RoundsPerWave;
    }

    // 설정된 시간 간격마다 일반 몬스터 반복 소환
    private IEnumerator AutoSpawn()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(
                Mathf.Max(0.01f, spawnInterval));

            if (!isSpawning)
                break;

            // 모든 사용 지점에 소환하는 묶음 한 번 실행
            SpawnAll();
        }

        spawnCoroutine = null;
    }

    
}