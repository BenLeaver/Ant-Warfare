using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.Navigation;

public class TestNavigationSource : MonoBehaviour
{
    public Transform start;
    public Transform end;

    private List<Vector2> currentPath;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (start == null || end == null)
            return;

        Vector2 startPos = new Vector2(start.position.x, start.position.y);
        Vector2 endPos = new Vector2(end.position.x, end.position.y);
        currentPath = NavigationManager.Instance.RequestPath(startPos, endPos);

        if (currentPath == null)
        {
            Debug.LogWarning("No path found");
        }
    }

    private void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        // Draw path segments
        Gizmos.color = Color.cyan;

        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 a = new Vector3(
                currentPath[i].x,
                currentPath[i].y,
                0f);

            Vector3 b = new Vector3(
                currentPath[i + 1].x,
                currentPath[i + 1].y,
                0f);

            Gizmos.DrawLine(a, b);
        }

        // Draw waypoint nodes
        Gizmos.color = Color.yellow;

        foreach (Vector2 waypoint in currentPath)
        {
            Gizmos.DrawSphere(
                new Vector3(
                    waypoint.x,
                    waypoint.y,
                    0f),
                0.2f);
        }

        // Draw start/end markers
        if (currentPath.Count > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentPath[0], 0.35f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentPath[currentPath.Count - 1], 0.35f);
        }
    }
}
