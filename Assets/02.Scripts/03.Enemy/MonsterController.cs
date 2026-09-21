using System;
using UnityEngine;
using UnityEngine.UI;
using PathologicalGames;

public interface IEnemy
{
    void AttackHit(double damage, bool Cri = false);
    void EnemyDestroy();
}
public enum EnemyCategory
{
    None,
    Nomal,
    Boss
}
public class MonsterController : MonoBehaviour, IEnemy
{
    MyObject myChar;
    GameManager gameManager;

    private Collider2D _collider2D;

    // 이 몬스터를 소환한 풀
    private SpawnPool monsterPool;

    // 생존 몬스터 수에 포함되어 있는지 확인
    private bool isCounted;

    // 같은 오브젝트가 재소환되었는지 구별하는 번호
    public uint SpawnVersion { get; private set; }

    public EnemyCategory _enemyCategory;
    [SerializeField] private float moveSpeed = 2f;
    public Animator _anim;
    public Transform hpBarPose;

    public double maxHp;
    public double currentHp;

    public GameObject hpBar;
    [SerializeField] private Slider hpBarSlider;
    private RectTransform hpBarRect;
    private Camera mainCamera;


    private PathRoute pathRoute;
    private int currentPointIndex;
    private bool isInitialized;
    private bool isDead;

    // 활성화되어 있고 초기화가 완료된 살아 있는 몬스터만 타겟 가능
    public bool IsTargetable => gameObject.activeInHierarchy && isInitialized && !isDead && currentHp > 0;

    [Header("이동 기준점")]
    [SerializeField] private Transform pathAnchor;
    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();

        if (_anim != null)
        {
            // 풀 반환으로 비활성화될 때 애니메이션이 변경한 값을 복원
            _anim.writeDefaultValuesOnDisable = true;
        }

        hpBarPose = transform.Find("hpBarPos");
        mainCamera = Camera.main;
    }

    public void Init(PathRoute route, int spawnPoint, SpawnPool pool)
    {
        myChar = MyObject.MyChar;
        gameManager = GameManager.Instance;

        if (route == null || route.PointCount == 0 || pool == null) return;

        monsterPool = pool;
        pathRoute = route;
        currentPointIndex = Mathf.Clamp(spawnPoint, 0, route.PointCount - 1);
       
        // 초기화 도중에는 타겟으로 선택되지 않도록 처리
        isInitialized = false;

        // 이전 생애를 추적하던 투사체와 구별
        SpawnVersion++;
        currentHp = maxHp;
        isDead = false;

        if (_collider2D != null)
            _collider2D.enabled = true;

        // 정상 외형의 활성 상태를 먼저 복원
        // Body는 켜고, 원래 꺼져 있던 Dead·Hit 이미지는 끔
        //RestoreVisualStates();

        if (_anim != null)
        {
            _anim.SetBool("Death", false);
            _anim.SetBool("Move", true);
        }
        // 이전 생애의 물리 이동값 초기화 — Unity 6 기준
        if (TryGetComponent(out Rigidbody2D body))
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        // 새 HP바를 풀에서 가져와 현재 체력 반영
        // 일반 몬스터만 개별 HP바 생성
        if (_enemyCategory != EnemyCategory.Boss)
        {
            CreateHpBar();
            UpdateHpBar();
        }

        isInitialized = true;

        // 생존 수에 한 번만 포함
        if (!isCounted)
        {
            isCounted = true;
            myChar.AddMonsterCount();
        }
    }

    private void Update()
    {
        if (!isInitialized || isDead || pathRoute == null)
            return;

        MoveAlongPath();
    }
    private void LateUpdate()
    {
        if (!isInitialized || isDead)
            return;

        UpdateHpBarPosition();
    }

    private void CreateHpBar()
    {
        if (hpBar != null)
            return;

        hpBar = gameManager.GetHPBar();

        hpBarRect = hpBar.GetComponent<RectTransform>();
        hpBarSlider = hpBar.GetComponent<Slider>();

        UpdateHpBarPosition();
    }

    private void UpdateHpBarPosition()
    {
        if (hpBarRect == null || hpBarPose == null || mainCamera == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(hpBarPose.position);

        hpBarRect.position = screenPosition;
    }


    //몬스터 레인따라 움직이는 Path
    private void MoveAlongPath()
    {
        if (pathRoute == null || pathRoute.PointCount == 0)
            return;

        // 이번 프레임에서 이동할 수 있는 총 거리
        float remainingDistance = Mathf.Max(0f, moveSpeed) * Time.deltaTime;

        // 겹친 포인트가 있어도 무한 반복하지 않도록 제한
        int maxSteps = pathRoute.PointCount + 1;

        for (int i = 0; i < maxSteps && remainingDistance > 0f; i++)
        {
            Transform target = pathRoute.GetPoint(currentPointIndex);

            if (target == null)
                return;

            Vector3 currentPosition =
                pathAnchor != null ? pathAnchor.position : transform.position;

            Vector3 direction = target.position - currentPosition;
            float distance = direction.magnitude;

            // 이미 도착했거나 포인트가 겹쳐 있으면 다음 포인트 선택
            if (distance <= 0.0001f)
            {
                currentPointIndex =
                    (currentPointIndex + 1) % pathRoute.PointCount;

                continue;
            }

            Vector3 moveDirection = direction / distance;

            // 진행 방향에 맞춰 좌우 전환
            UpdateFacing(moveDirection);

            // 포인트까지 남은 거리보다 많이 이동하지 않음
            float moveDistance = Mathf.Min(remainingDistance, distance);

            transform.position += moveDirection * moveDistance;
            remainingDistance -= moveDistance;

            if (moveDistance >= distance)
            {
                // 포인트 도착: 남은 이동량으로 다음 구간까지 진행
                currentPointIndex =
                    (currentPointIndex + 1) % pathRoute.PointCount;
            }
            else
            {
                // 이번 프레임의 이동량을 모두 사용
                break;
            }
        }
    }
    private void UpdateFacing(Vector3 moveDirection)
    {
        float horizontalDirection;

        if (Mathf.Abs(moveDirection.x) >= 0.001f)
        {
            // 가로 이동은 실제 이동 방향을 바라봄
            horizontalDirection = moveDirection.x;
        }
        else if (Mathf.Abs(moveDirection.y) >= 0.001f)
        {
            // 아래로 이동(y 음수) → 오른쪽(양수)
            // 위로 이동(y 양수) → 왼쪽(음수)
            horizontalDirection = -moveDirection.y;
        }
        else
        {
            // 이동 방향이 없으면 기존 방향 유지
            return;
        }

        Vector3 scale = transform.localScale;

        // 기본 이미지가 왼쪽을 보므로:
        // 오른쪽 이동은 X 음수, 왼쪽 이동은 X 양수
        float newScaleX = Mathf.Abs(scale.x) *
            (horizontalDirection > 0f ? -1f : 1f);

        if (Mathf.Approximately(scale.x, newScaleX))
            return;

        // 뒤집기 전 이동 기준점의 월드 위치 저장
        Vector3 anchorPosition =
            pathAnchor != null ? pathAnchor.position : transform.position;

        // 일반 몬스터·보스 크기는 유지하고 방향만 변경
        scale.x = newScaleX;
        transform.localScale = scale;

        // PathAnchor가 루트 중앙에서 벗어나 있어도
        // 좌우 전환 때문에 경로상의 위치가 튀지 않도록 유지
        if (pathAnchor != null)
            transform.position += anchorPosition - pathAnchor.position;
    }
    public void AttackHit(double damage, bool Cri = false)
    {
        // 사망했거나 풀에 반환된 몬스터는 데미지를 받지 않음
        if (!IsTargetable)
            return;

        currentHp -= damage;

        // 체력이 모두 소진되면 사망 처리 시작
        if (currentHp <= 0)
        {
            EnemyDestroy();
        }

        UpdateHpBar();
    }


    public void EnemyDestroy()
    {
        if (isDead)
            return;

        isDead = true;
        isInitialized = false;

        // 사망 즉시 생존 몬스터 수 감소
        RemoveMonsterCount();

        // 공격 충돌 차단
        if (_collider2D != null)
            _collider2D.enabled = false;

        // HP바를 풀에 반환
        ReturnHpBar();

        if (_anim != null && _anim.isActiveAndEnabled && _anim.runtimeAnimatorController != null)
        {
            // 몬스터는 활성 상태로 두고 Death 애니메이션을 보여줌
            _anim.SetBool("Move", false);
            _anim.SetBool("Death", true);
        }
        else
        {
            // 재생할 Animator가 없으면 즉시 회수
            OnDeathAnimationEnd();
        }
    }

    // Death 애니메이션 마지막 이벤트에서 호출
    public void OnDeathAnimationEnd()
    {
        if (!isDead)
            return;

        // 이미 반환된 개체는 중복 반환하지 않음
        if (monsterPool != null && monsterPool.IsSpawned(transform))
        {
            monsterPool.Despawn(transform);
        }
    }

    // 생존 수 중복 감소 방지
    private void RemoveMonsterCount()
    {
        if (!isCounted)
            return;

        isCounted = false;
        myChar.RemoveMonsterCount();
    }

    // HP바 반환 후 참조를 비워 다음 소환 때 다시 가져오도록 처리
    private void ReturnHpBar()
    {
        if (hpBar != null)
            gameManager.ReturnHPBar(hpBar);

        hpBar = null;
        hpBarSlider = null;
        hpBarRect = null;
    }

    // 사망으로 회수하거나 강제로 회수할 때 모두 실행되는 정리
    // PathologicalGames가 Despawn 시 자동 호출
    private void OnDespawned(SpawnPool pool)
    {
        isInitialized = false;
        isDead = true;

        // 이미 사망 처리에서 정리했다면 내부 검사에 의해 중복 처리되지 않음
        // 스테이지 정리 등으로 살아 있는 몬스터를 강제 회수할 때도 정리
        RemoveMonsterCount();
        ReturnHpBar();

        if (_collider2D != null)
            _collider2D.enabled = false;

        pathRoute = null;
    }
    private void UpdateHpBar()
    {
        if (hpBarSlider == null)
            return;

        if (maxHp <= 0)
        {
            hpBarSlider.value = 0f;
            return;
        }

        hpBarSlider.value = (float)(currentHp / maxHp);
    }
}
