using System.Collections.Generic;
using UnityEngine;

namespace Ant.Navigation.Pathfinding
{
    public class AStarPathfinder
    {
        public List<Vector2> FindPath(NavigationGrid grid, Vector2 startPos, Vector2 endPos)
        {
            Vector2Int startCoord = grid.WorldToCell(startPos);
            Vector2Int endCoord = grid.WorldToCell(endPos);

            NavigationCell startCell = grid.GetCell(startCoord);
            NavigationCell endCell = grid.GetCell(endCoord);

            if (startCell == null || endCell == null)
                return null;

            if (!startCell.Walkable || !endCell.Walkable)
                return null;

            List<PathNode> openSet = new List<PathNode>();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

            Dictionary<Vector2Int, PathNode> nodes = new Dictionary<Vector2Int, PathNode>();

            PathNode startNode = new PathNode(startCell);
            startNode.GCost = 0f;
            startNode.HCost = Heuristic(startCell, endCell);

            openSet.Add(startNode);
            nodes[startCell.Cell] = startNode;

            while (openSet.Count > 0)
            {
                PathNode current = GetLowestFCostNode(openSet);

                if (current.Cell.Cell == endCell.Cell)
                {
                    return ReconstructPath(current);
                }

                openSet.Remove(current);
                closedSet.Add(current.Cell.Cell);

                foreach (NavigationCell neighbourCell in grid.GetNeighbours(current.Cell))
                {
                    if (!neighbourCell.Walkable) continue;

                    if (closedSet.Contains(neighbourCell.Cell)) continue;

                    float tentativeGCost = current.GCost + Vector2.Distance(current.Cell.WorldPosition, neighbourCell.WorldPosition);

                    if (!nodes.TryGetValue(neighbourCell.Cell, out PathNode neighbour))
                    {
                        neighbour = new PathNode(neighbourCell);
                        nodes.Add(neighbourCell.Cell, neighbour);
                    }

                    bool inOpenSet = openSet.Contains(neighbour);

                    if (!inOpenSet || tentativeGCost < neighbour.GCost)
                    {
                        neighbour.Parent = current;

                        neighbour.GCost = tentativeGCost;

                        neighbour.HCost = Heuristic(neighbourCell, endCell);

                        if (!inOpenSet)
                            openSet.Add(neighbour);
                    }
                }
            }

            // No path found
            return null;
        }

        private float Heuristic(NavigationCell a, NavigationCell b)
        {
            return Vector2.Distance(a.WorldPosition, b.WorldPosition);
        }

        private PathNode GetLowestFCostNode(
            List<PathNode> nodes)
        {
            PathNode best = nodes[0];

            for (int i = 1; i < nodes.Count; i++)
            {
                PathNode current = nodes[i];

                if (current.FCost < best.FCost)
                {
                    best = current;
                }
                else if (Mathf.Approximately(current.FCost, best.FCost))
                {
                    if (current.HCost < best.HCost)
                        best = current;
                }
            }
            return best;
        }

        private List<Vector2> ReconstructPath(
            PathNode endNode)
        {
            List<Vector2> path = new List<Vector2>();

            PathNode current = endNode;

            while (current != null)
            {
                path.Add(current.Cell.WorldPosition);
                current = current.Parent;
            }

            path.Reverse();

            return path;
        }
    }
}
