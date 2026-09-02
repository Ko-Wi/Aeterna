using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnData
    {
        public Transform spawnPoint;       // 몬스터가 생성될 위치
        public PathRoute pathRoute;         // 몬스터가 이동할 경로
        public int spawnPointIndex;         // 이동을 시작할 경로 지점
    }

    private MyObject myChar;

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

    // 한 스테이지의 전체 라운드 수
    [SerializeField, Min(1)] private int totalRoundCount = 30;

    [Header("자동 소환 설정")]

    // 몬스터가 반복 소환되는 시간 간격
    [Min(0.01f)] public float spawnInterval = 1f;

    [Header("몬스터 크기 설정")]
    [SerializeField] private float normalMonsterScale = 0.2f; // 일반 몬스터 크기
    [SerializeField] private float bossMonsterScale = 0.4f;   // 보스 몬스터 크기

    private void Awake()
    {
        // 스테이지와 현재 몬스터 수를 관리하는 myChar 가져오기
        myChar = MyObject.MyChar;
    }

    private void Start()
    {
        // 새로운 스테이지 시작 시 현재 몬스터 수 초기화
        myChar.ResetMonsterCount();

        // 몬스터 자동 소환 시작
        StartCoroutine(AutoSpawn());
    }


    // 현재 스테이지 등급에 맞는 일반 몬스터를 모든 소환 지점에 생성
    public void SpawnAll()
    {
        // 현재 스테이지와 라운드에 해당하는 일반 몬스터 가져오기
        GameObject currentMonsterPrefab = GetCurrentMonsterPrefab();

        // 일반 몬스터 또는 소환 데이터가 없다면 소환하지 않음
        if (currentMonsterPrefab == null || spawnDatas == null)
            return;

        // 일반 몬스터 크기 0.2를 적용해서 소환
        SpawnMonsters(currentMonsterPrefab, normalMonsterScale, false);
    }

    // 현재 스테이지에 해당하는 보스를 기존 소환 지점마다 생성
    public void SpawnBoss()
    {
        // 현재 스테이지에 해당하는 보스 가져오기
        GameObject currentBossPrefab = GetCurrentBossPrefab();

        // 보스 또는 소환 데이터가 없다면 소환하지 않음
        if (currentBossPrefab == null || spawnDatas == null)
            return;

        // 보스 크기 0.4를 적용해서 소환
        SpawnMonsters(currentBossPrefab, bossMonsterScale, true);
    }

    // 전달받은 프리팹을 기존 spawnDatas의 위치와 경로에 맞춰 생성
    private void SpawnMonsters(GameObject monsterPrefab, float monsterScale, bool isBoss)
    {
        for (int i = 0; i < spawnDatas.Length; i++)
        {
            // 소환 위치 또는 이동 경로가 없는 데이터는 건너뜀
            if (spawnDatas[i].spawnPoint == null || spawnDatas[i].pathRoute == null)
                continue;

            // 전달받은 일반 몬스터 또는 보스 프리팹 생성
            GameObject monster = Instantiate(monsterPrefab, spawnDatas[i].spawnPoint.position, Quaternion.identity);

            // 생성된 몬스터를 기존 몬스터 부모 아래로 이동
            monster.transform.SetParent(monsterParent);

            // 일반 몬스터는 0.2, 보스는 0.4 크기 적용
            monster.transform.localScale = Vector3.one * monsterScale;

            // 생성된 몬스터의 MonsterController 가져오기
            MonsterController controller = monster.GetComponent<MonsterController>();

            // MonsterController가 없다면 잘못된 프리팹이므로 제거
            if (controller == null)
            {
                Debug.LogError($"{monster.name}에 MonsterController가 없습니다.");
                Destroy(monster);
                continue;
            }

            // 일반 몬스터와 보스 카테고리 구분
            controller._enemyCategory = isBoss ? EnemyCategory.Boss : EnemyCategory.Nomal;

            // 추후 CSV에서 일반 몬스터 또는 보스 체력 적용
            controller.maxHp = 100;

            // 기존 소환 데이터의 경로와 시작 지점 적용
            controller.Init(spawnDatas[i].pathRoute, spawnDatas[i].spawnPointIndex);

            // 현재 살아 있는 몬스터 수 증가
            myChar.AddMonsterCount();
        }
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

        // 라운드가 1부터 전체 라운드 사이를 벗어나지 않도록 제한
        int currentRound = Mathf.Clamp(myChar.CurrentRound, 1, totalRoundCount);

        /*
         * 현재 스테이지에서 사용할 첫 번째 몬스터 인덱스 계산
         *
         * 1스테이지 → 0
         * 2스테이지 → 2
         * 3스테이지 → 4
         */
        int firstMonsterIndex = (currentStage - 1) * monsterKindsPerStage;

        /*
         * 현재 라운드에서 사용할 몬스터 순번 계산
         *
         * 몬스터 2종, 30라운드:
         * 1~15라운드  → 0
         * 16~30라운드 → 1
         */
        int roundMonsterIndex = (currentRound - 1) * monsterKindsPerStage / totalRoundCount;

        // 스테이지 시작 인덱스와 라운드 몬스터 순번을 합침
        int monsterIndex = firstMonsterIndex + roundMonsterIndex;

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


    // 설정된 시간 간격마다 일반 몬스터 반복 소환
    private IEnumerator AutoSpawn()
    {
        while (true)
        {
            // 설정된 소환 간격만큼 대기
            yield return new WaitForSeconds(spawnInterval);

            // 현재 등급과 라운드에 맞는 일반 몬스터 소환
            SpawnAll();
        }
    }
}