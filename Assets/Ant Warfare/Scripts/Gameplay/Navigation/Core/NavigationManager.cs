using UnityEngine;
using System.Collections.Generic;
using Ant.Navigation.Pathfinding;

namespace Ant.Navigation
{
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance;

        [Header("Grid Settings")]
        public int width = 1000;
        public int height = 1000;
        public float cellSize = 1f;

        [Header("Obstacle Detection")]
        public LayerMask obstacleMask;

        public NavigationGrid OverworldGrid { get; private set; }

        private AStarPathfinder pathfinder = new AStarPathfinder();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            //BakeGrid();
        }

        public void BakeGrid()
        {
            var mapInfo = FindObjectOfType<NavigationMapInfo>();

            OverworldGrid = new NavigationGrid(
                mapInfo.width,
                mapInfo.height,
                mapInfo.cellSize,
                mapInfo.origin);

            Debug.Log("Baking navigation grid...");

            for (int x = 0; x < mapInfo.width; x++)
            {
                for (int y = 0; y < mapInfo.height; y++)
                {
                    Vector2Int cellCoord = new Vector2Int(x, y);

                    Vector2 worldPos =
                        OverworldGrid.CellToWorld(cellCoord);

                    bool blocked =
                        Physics2D.OverlapBox(
                            worldPos,
                            Vector2.one * cellSize * 0.9f,
                            0f,
                            obstacleMask);

                    OverworldGrid.SetWalkable(
                        cellCoord,
                        !blocked);
                }
            }

            Debug.Log("Navigation grid bake complete.");
        }

        public List<Vector2> RequestPath(Vector2 start, Vector2 end)
        {
            List<Vector2> path = pathfinder.FindPath(OverworldGrid, start, end);

            if (path == null) return null;

            return SmoothPath(path);
        }

        private List<Vector2> SmoothPath(List<Vector2> path)
        {
            if (path.Count <= 2)
                return path;

            List<Vector2> result = new List<Vector2>();
            int current = 0;

            result.Add(path[current]);

            while (current < path.Count - 1)
            {
                int furthestVisible = current + 1;
                for (int i = current + 2; i < path.Count; i++)
                {
                    if (HasLineOfSight(path[current], path[i]))
                    {
                        furthestVisible = i;
                    }
                    else
                    {
                        break;
                    }
                }
                result.Add(path[furthestVisible]);
                current = furthestVisible;
            }

            return result;
        }

        private bool HasLineOfSight(Vector2 start, Vector2 end)
        {
            float distance = Vector2.Distance(start, end);
            float stepSize = OverworldGrid.CellSize * 0.5f;

            int steps = Mathf.CeilToInt(distance / stepSize);
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;

                Vector2 point = Vector2.Lerp(start, end, t);

                Vector2Int coord = OverworldGrid.WorldToCell(point);
                NavigationCell cell = OverworldGrid.GetCell(coord);

                if (cell == null || !cell.Walkable)
                    return false;
            }
            return true;
        }

    }
}

