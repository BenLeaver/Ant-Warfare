using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Provides visual debugging for an ant's AI logic in scene view.
/// Draws gizmos representing the ant's local and ultimate targets,
/// the NavMesh path toward the local target, and the current AI state.
/// 
/// Yellow = Local Target
/// Cyan = Ultimate Target
/// Green = A* path segments
/// Magenta = Individual waypoints
/// Red = current waypoint
/// </summary>
public class AntAIDebug : MonoBehaviour
{
    public AntContext context;
    public AntStateManager manager;
    public Color localColor = Color.yellow;
    public Color ultimateColor = Color.cyan;

    private NavMeshPath path;

    void OnDrawGizmos()
    {
        if (context == null || context.World == null)
            return;

        Vector3 pos = context.World.Position;


        Gizmos.color = localColor;
        //Gizmos.DrawLine(pos, context.LocalTarget);
        //Gizmos.DrawSphere(context.LocalTarget, 0.1f);

        Gizmos.color = ultimateColor;
        //Gizmos.DrawLine(pos, context.UltimateTarget);
        //Gizmos.DrawSphere(context.UltimateTarget, 0.1f);

        SingleplayerAntWorld world = context.World as SingleplayerAntWorld;

        // Draw current A* path
        if (Application.isPlaying && world != null)
        {
            var currentPath = world.CurrentPath;

            if (currentPath != null && currentPath.Count > 0)
            {
                Gizmos.color = Color.green;

                // Draw path lines
                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(
                        currentPath[i],
                        currentPath[i + 1]);
                }

                // Draw waypoint markers
                Gizmos.color = Color.magenta;

                foreach (Vector2 waypoint in currentPath)
                {
                    Gizmos.DrawSphere(
                        waypoint,
                        0.15f);
                }
            }

            if (world.CurrentWaypointIndex < currentPath.Count)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(
                    currentPath[world.CurrentWaypointIndex],
                    0.25f);
            }
        }



#if UNITY_EDITOR
        // Draw state label
        if (manager != null)
        {
            UnityEditor.Handles.Label(
                pos + Vector3.up * 0.5f,
                $"State: {manager.CurrentStateType}"
            );
        }
        else
        {
            Debug.Log("Manger is null");
        }
#endif
    }
}
