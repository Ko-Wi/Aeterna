using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float lifeTime = 3f;

    [SerializeField] private GameObject hiatEffect;

    private Rigidbody2D __rigidbody2D;
    private MonsterController target;
    private Vector2 lastDirection = Vector2.right;
    private int damage;
    private float timer;
    private bool hasHit;

    // 투사체 생성 당시 타겟의 소환 번호
    private uint targetSpawnVersion;

    // 처음 지정했던 생애의 몬스터가 아직 살아 있는지 확인
    private bool HasValidTarget => target != null && target.IsTargetable && target.SpawnVersion == targetSpawnVersion;

    private void Awake()
    {
        __rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Init(MonsterController target, int damage)
    {
        this.target = target;
        this.damage = damage;

        // 현재 타겟의 소환 번호 저장
        targetSpawnVersion = target != null ? target.SpawnVersion : 0;

        UpdateDirection();
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // 원래 타겟이 살아 있을 때만 방향 갱신
        if (HasValidTarget)
        {
            UpdateDirection();
        }
        else // 타겟을 잃으면 참조를 해제하고 기존 방향으로 계속 이동
        {
            target = null;
        }

        // 타겟이 사망하거나 삭제되면 마지막 방향으로 계속 이동한다.
        Vector2 nextPosition = __rigidbody2D.position
            + lastDirection * moveSpeed * Time.fixedDeltaTime;

        __rigidbody2D.MovePosition(nextPosition);
    }

    private void UpdateDirection()
    {
        if (target == null)
            return;

        Vector2 direction = (Vector2)target.transform.position - __rigidbody2D.position;

        if (direction.sqrMagnitude > 0.0001f)
            lastDirection = direction.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 타겟이 사망하거나 재소환된 경우 공격하지 않음
        if (hasHit || !HasValidTarget)
            return;

        MonsterController monster = other.GetComponent<MonsterController>();

        if (monster == null || monster != target)
            return;

        hasHit = true;
        Hit(monster);
    }

    private void Hit(MonsterController monster)
    {
        if (hiatEffect != null)
            Instantiate(hiatEffect, transform.position, Quaternion.identity);

        monster.AttackHit(damage, false);

        Destroy(gameObject);
    }
}
