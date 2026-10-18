using UnityEngine;
using System.Collections.Generic;

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
            OverworldGrid = new NavigationGrid(
                width,
                height,
                cellSize);

            Debug.Log("Baking navigation grid...");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
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
            // A* will be added later.
            // For now just return start -> end.

            List<Vector2> path = new List<Vector2>
            {
                start,
                end
            };

            return path;
        }
    }
}

