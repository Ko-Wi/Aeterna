using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float hitDistance = 0.15f;
    [SerializeField] private float lifeTime = 3f;

    [SerializeField] private GameObject hiatEffect;
    private MonsterController target;
    private int damage;
    private float timer;

    public void Init(MonsterController target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPos = target.transform.position;
        Vector3 dir = targetPos - transform.position;

        if (dir.magnitude <= hitDistance)
        {
            Hit();
            return;
        }

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
    }

    private void Hit()
    {
        if (target != null)
        {
            // MonsterController 쪽에 데미지 함수가 있으면 여기에 연결
            //target.Damage(damage);
            GameObject projectileObj = Instantiate(hiatEffect, target.transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
