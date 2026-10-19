using UnityEngine;
using System.Collections.Generic;

namespace Ant.Navigation
{
    public class NavigationGrid
    {
        public int Width;
        public int Height;

        public Vector2 Origin;

        public float CellSize;

        private NavigationCell[,] cells;

        public NavigationGrid(int width, int height, float cellSize, Vector2 origin)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            Origin = origin;

            cells = new NavigationCell[width, height];

            // Create cells
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new NavigationCell
                    {
                        Walkable = true,
                        Cell = new Vector2Int(x, y),
                        WorldPosition = CellToWorld(new Vector2Int(x, y))
                    };
                }
            }
        }

        public Vector2Int WorldToCell(Vector2 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - Origin.x) / CellSize);
            int y = Mathf.FloorToInt((worldPos.y - Origin.y) / CellSize);

            return new Vector2Int(x, y);
        }

        public Vector2 CellToWorld(Vector2Int cell)
        {
            return new Vector2(
                Origin.x + (cell.x + 0.5f) * CellSize,
                Origin.y + (cell.y + 0.5f) * CellSize);
        }

        public bool InBounds(Vector2Int coord)
        {
            return coord.x >= 0 &&
                   coord.y >= 0 &&
                   coord.x < Width &&
                   coord.y < Height;
        }

        public NavigationCell GetCell(Vector2Int coord)
        {
            if (!InBounds(coord)) return null;

            return cells[coord.x, coord.y];
        }

        public void SetWalkable(Vector2Int coord, bool walkable)
        {
            if (!InBounds(coord)) return;

            cells[coord.x, coord.y].Walkable = walkable;
        }

        public List<NavigationCell> GetNeighbours(NavigationCell cell)
        {
            List<NavigationCell> neighbours = new List<NavigationCell>(8);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Skip self
                    if (dx == 0 && dy == 0)
                        continue;

                    Vector2Int neighbourCoord =
                        cell.Cell + new Vector2Int(dx, dy);

                    if (!InBounds(neighbourCoord))
                        continue;

                    neighbours.Add(cells[neighbourCoord.x, neighbourCoord.y]);
                }
            }

            return neighbours;
        }
    }
}