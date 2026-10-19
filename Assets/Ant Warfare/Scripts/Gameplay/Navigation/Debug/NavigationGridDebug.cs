using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ant.Navigation
{
    public class NavigationGridDebug : MonoBehaviour
    {
        public NavigationManager nav;

        [Header("Visual Settings")]
        public bool drawGrid = true;
        public bool drawWalkable = false;
        public bool drawBlocked = true;
        public int debugRadius = 50;

        [Range(0f, 1f)]
        public float alpha = 0.25f;

        // Draw every cell OnDrawGizmos()
        private void OnDrawGizmos()
        {
            if (!drawGrid)
                return;

            if (nav == null)
                nav = NavigationManager.Instance;

            if (nav == null)
                return;

            if (nav.OverworldGrid == null)
                return;

            NavigationGrid grid = nav.OverworldGrid;

#if UNITY_EDITOR
            if (UnityEditor.SceneView.lastActiveSceneView == null)
                return;

            Vector3 cameraPos = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;

            Vector2Int cameraCell = grid.WorldToCell(cameraPos);

            int cellRadius = Mathf.CeilToInt(debugRadius / grid.CellSize);

            int minX = Mathf.Max(0, cameraCell.x - cellRadius);
            int maxX = Mathf.Min(grid.Width - 1, cameraCell.x + cellRadius);

            int minY = Mathf.Max(0, cameraCell.y - cellRadius);
            int maxY = Mathf.Min(grid.Height - 1, cameraCell.y + cellRadius);


            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    NavigationCell cell =
                        grid.GetCell(new Vector2Int(x, y));

                    if (cell == null)
                        continue;

                    if (cell.Walkable && !drawWalkable)
                        continue;

                    if (!cell.Walkable && !drawBlocked)
                        continue;

                    Gizmos.color = cell.Walkable
                        ? new Color(0f, 1f, 0f, alpha)
                        : new Color(1f, 0f, 0f, alpha);

                    Gizmos.DrawCube(
                        cell.WorldPosition,
                        Vector3.one * grid.CellSize * 0.9f);
                }
            }
#endif
        }
    }

}
