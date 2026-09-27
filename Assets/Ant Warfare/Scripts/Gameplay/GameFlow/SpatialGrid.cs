using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A spatial hash grid for fast proximity queries.
/// Supports negative coords and only stores occupied cells.
/// Stores both units and food.
/// </summary>
public class SpatialGrid
{
    public readonly int cellSize;

    public Dictionary<Vector2Int, List<UnitInfo>> unitCells
        = new Dictionary<Vector2Int, List<UnitInfo>>();

    public Dictionary<Vector2Int, List<FoodInfo>> foodCells
        = new Dictionary<Vector2Int, List<FoodInfo>>();

    public SpatialGrid(int cellSize)
    {
        this.cellSize = cellSize;
    }

    /// <summary>
    /// Convert world position to a grid cell.
    /// </summary>
    public Vector2Int WorldToCell(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / cellSize);
        int y = Mathf.FloorToInt(pos.y / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Store info for a unit in it's spatial grid cell.
    /// The cell vector is also stored in the UnitInfo object.
    /// </summary>
    public void AddUnit(UnitInfo info)
    {
        Vector2Int cell = WorldToCell(info.transform.position);
        info.cell = cell;

        if (!unitCells.TryGetValue(cell, out var list))
        {
            list = new List<UnitInfo>();
            unitCells[cell] = list;
        }

        list.Add(info);
    }

    /// <summary>
    /// Remove a unit from the spatial grid.
    /// </summary>
    public void RemoveUnit(UnitInfo info)
    {
        if (unitCells.TryGetValue(info.cell, out var list))
        {
            list.Remove(info);
            if (list.Count == 0)
                unitCells.Remove(info.cell);
        }
    }

    /// <summary>
    /// Update a unit's position in the spatial grid.
    /// </summary>
    public void UpdateUnit(UnitInfo info)
    {
        Vector2Int newCell = WorldToCell(info.transform.position);

        if (newCell == info.cell)
            return;

        // Remove from old cell
        if (unitCells.TryGetValue(info.cell, out var oldList))
        {
            oldList.Remove(info);
            if (oldList.Count == 0)
                unitCells.Remove(info.cell);
        }

        // Add to new cell
        info.cell = newCell;

        if (!unitCells.TryGetValue(newCell, out var newList))
        {
            newList = new List<UnitInfo>();
            unitCells[newCell] = newList;
        }

        newList.Add(info);
    }

    /// <summary>
    /// Returns a list of all units in neighbouring cells to the position vector (of a unit).
    /// 
    /// If each cell has a size of 20, assuming sight range is 20, this list is guaranteed 
    /// to include all units in the unit's sight range.
    /// </summary>
    public List<UnitInfo> GetNearbyUnits(Vector3 pos)
    {
        Vector2Int cell = WorldToCell(pos);
        List<UnitInfo> result = new List<UnitInfo>(32);
        
        // Search all cells within 1 cell of the center cell. So 9 cells will be searched.
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int c = new Vector2Int(cell.x + dx, cell.y + dy);

                if (unitCells.TryGetValue(c, out var list))
                    result.AddRange(list);
            }
        }
        return result;
    }

    /// <summary>
    /// Returns a list of all enemy units in neighbouring cells.
    /// </summary>
    public List<UnitInfo> GetNearbyEnemies(Vector3 pos, int friendlyTeam)
    {
        Vector2Int cell = WorldToCell(pos);
        List<UnitInfo> result = new List<UnitInfo>(32);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int c = new Vector2Int(cell.x + dx, cell.y + dy);

                if (unitCells.TryGetValue(c, out var list))
                {
                    foreach (var info in list)
                    {
                        // Only add units from other teams.
                        if (info.team != friendlyTeam)
                            result.Add(info);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Store info for a food object in it's spatial grid cell.
    /// The cell vector is also stored in the UnitInfo object.
    /// </summary>
    public void AddFood(FoodInfo info)
    {
        Vector2Int cell = WorldToCell(info.transform.position);
        info.cell = cell;

        if (!foodCells.TryGetValue(cell, out var list))
        {
            list = new List<FoodInfo>();
            foodCells[cell] = list;
        }

        list.Add(info);
    }

    /// <summary>
    /// Remove a food object from the spatial grid.
    /// </summary>
    public void RemoveFood(FoodInfo info)
    {
        if (foodCells.TryGetValue(info.cell, out var list))
        {
            list.Remove(info);
            if (list.Count == 0)
                foodCells.Remove(info.cell);
        }
    }

    /// <summary>
    /// Update a food object's position in the spatial grid.
    /// </summary>
    public void UpdateFood(FoodInfo info)
    {
        Vector2Int newCell = WorldToCell(info.transform.position);

        if (newCell == info.cell)
            return;

        if (foodCells.TryGetValue(info.cell, out var oldList))
        {
            oldList.Remove(info);
            if (oldList.Count == 0)
                foodCells.Remove(info.cell);
        }

        info.cell = newCell;

        if (!foodCells.TryGetValue(newCell, out var newList))
        {
            newList = new List<FoodInfo>();
            foodCells[newCell] = newList;
        }

        newList.Add(info);
    }

    /// <summary>
    /// Returns a list of all food objects in neighbouring cells to the position vector (of a unit).
    /// 
    /// If each cell has a size of 20, assuming sight range is 20, this list is guaranteed 
    /// to include all food pellets in the unit's sight range.
    /// </summary>
    public List<FoodInfo> GetNearbyFood(Vector3 pos)
    {
        Vector2Int cell = WorldToCell(pos);
        List<FoodInfo> result = new List<FoodInfo>(32);

        for (int dx = -1; dx <= 1; dx++)
        { 
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int c = new Vector2Int(cell.x + dx, cell.y + dy);

                if (foodCells.TryGetValue(c, out var list))
                    result.AddRange(list);
            }
        }
        return result;
    }
}
