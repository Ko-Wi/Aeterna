using UnityEngine;

public class PathRoute : MonoBehaviour
{
    [SerializeField] private Transform[] points;

    public int PointCount => points != null ? points.Length : 0;

    public Transform GetPoint(int index)
    {
        if (points == null || points.Length == 0)
            return null;

        if (index < 0 || index >= points.Length)
            return null;

        return points[index];
    }

    private void OnDrawGizmos()
    {
        if (points == null || points.Length < 2)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;

            Gizmos.DrawSphere(points[i].position, 0.1f);

            int nextIndex = i + 1;
            if (nextIndex >= points.Length)
                nextIndex = 0;

            if (points[nextIndex] != null)
                Gizmos.DrawLine(points[i].position, points[nextIndex].position);
        }
    }
}
