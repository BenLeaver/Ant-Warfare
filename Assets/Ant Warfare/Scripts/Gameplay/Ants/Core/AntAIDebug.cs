using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Provides visual debugging for an ant's AI logic in scene view.
/// Draws gizmos representing the ant's local and ultimate targets,
/// the NavMesh path toward the local target, and the current AI state.
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
        Gizmos.DrawLine(pos, context.LocalTarget);
        Gizmos.DrawSphere(context.LocalTarget, 0.1f);

        Gizmos.color = ultimateColor;
        Gizmos.DrawLine(pos, context.UltimateTarget);
        Gizmos.DrawSphere(context.UltimateTarget, 0.1f);


        // Draw NavMesh path to LocalTarget
        if (Application.isPlaying)
        {
            if (path == null)
                path = new NavMeshPath();

            if (NavMesh.CalculatePath(pos, context.LocalTarget, NavMesh.AllAreas, path))
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                }
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
#endif
    }
}
