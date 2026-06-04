using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arriveDistance = 0.05f;

    private PathRoute pathRoute;
    private int currentPointIndex;
    private bool isInitialized;
    private bool isDead;

    public void Init(PathRoute route, int spawnPoint)
    {
        if (route == null || route.PointCount == 0)
            return;

        pathRoute = route;
        currentPointIndex = spawnPoint;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || isDead || pathRoute == null)
            return;

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        Transform target = pathRoute.GetPoint(currentPointIndex);
        if (target == null) return;

        Vector3 dir = target.position - transform.position;

        if (dir.sqrMagnitude <= arriveDistance * arriveDistance)
        {
            transform.position = target.position;
            currentPointIndex++;

            if (currentPointIndex >= pathRoute.PointCount)
                currentPointIndex = 0;

            return;
        }

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;

        if (Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            transform.localScale = new Vector3(Mathf.Sign(dir.x) * Mathf.Abs(scale.x), scale.y, scale.z);
        }
    }
}
