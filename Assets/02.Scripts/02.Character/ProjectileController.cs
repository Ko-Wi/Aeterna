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

    private void Awake()
    {
        __rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Init(MonsterController target, int damage)
    {
        this.target = target;
        this.damage = damage;

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

        // 타겟이 살아 있을 때만 추적 방향을 갱신한다.
        if (target != null && target.IsTargetable)
            UpdateDirection();

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
        if (hasHit || target == null || !target.IsTargetable)
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
