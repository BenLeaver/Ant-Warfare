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
        public bool drawWalkable = true;
        public bool drawBlocked = true;

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

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
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

                    Gizmos.color = Color.black;

                    Gizmos.DrawWireCube(
                        cell.WorldPosition,
                        Vector3.one * grid.CellSize);
                }
            }
        }
    }

}
