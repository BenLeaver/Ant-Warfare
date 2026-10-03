using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Manages and keeps track of Ant and Bug units in the game.
/// </summary>
public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    public List<UnitInfo> allUnits = new List<UnitInfo>();

    // Team lookup dict
    public Dictionary<int, List<UnitInfo>> unitsByTeam = new Dictionary<int, List<UnitInfo>>();

    public Dictionary<int, List<UnitInfo>> queenDict = new Dictionary<int, List<UnitInfo>>();

    // Spatial grid
    private SpatialGrid grid;

    [Header("Grid Settings")]
    public int cellSize = 20;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            grid = new SpatialGrid(cellSize);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Update spatial grid cells for all units
        foreach (var info in allUnits)
        {
            grid.UpdateUnit(info);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Units: " + allUnits.Count);
            foreach (var u in allUnits)
            {
                Debug.Log($"Unit: {u.go.name}, Team: {u.team}, Type: {u.unitType}, AntWorld: {u.antWorld}");
            }
        }
    }

    /// <summary>
    /// Adds a unit to the manager, storing the details and components of the unit game object.
    /// </summary>
    /// <param name="unit">The unit to register.</param>
    public UnitInfo RegisterUnit(GameObject unit)
    {
        var info = new UnitInfo
        {
            go = unit,
            transform = unit.transform,
            health = unit.GetComponent<IHealth>(),
            antWorld = unit.GetComponent<IAntWorld>(),
            queenScript = unit.GetComponent<BaseAntQueenAI>(),
            playerScript = unit.GetComponent<Player_Singleplayer>()
        };

        info.team = info.health.Team;

        // Determine unit type
        if (info.playerScript != null)
        {
            info.unitType = UnitType.Player;
        }
        else if (info.queenScript != null)
        {
            info.unitType = UnitType.Queen;

            if (!queenDict.ContainsKey(info.team))
                queenDict[info.team] = new List<UnitInfo>();

            queenDict[info.team].Add(info);
        }
        else if (info.antWorld != null)
        {
            info.unitType = info.antWorld.Type == AntType.Worker
                ? UnitType.Worker
                : UnitType.Soldier;
        }
        else
        {
            info.unitType = UnitType.Bug;
        }

        // Add to master list
        allUnits.Add(info);

        // Add to team list
        if (!unitsByTeam.ContainsKey(info.team))
            unitsByTeam[info.team] = new List<UnitInfo>();

        unitsByTeam[info.team].Add(info);

        grid.AddUnit(info);

        return info;
    }

    /// <summary>
    /// Removes a unit from the manager.
    /// </summary>
    public void UnregisterUnit(UnitInfo info)
    {
        if (info == null) return;

        allUnits.Remove(info);
        unitsByTeam[info.team].Remove(info);

        if (info.unitType == UnitType.Queen)
            queenDict[info.team].Remove(info);

        grid.RemoveUnit(info);
    }
    
    /// <summary>
    /// Removes a unit from the manager 
    /// </summary>
    /// <param name="unit">The unit to unregister.</param>
    public void UnregisterUnit(GameObject unit)
    {
        UnitInfo info = allUnits.Find(u => u.go == unit);
        if (info == null) return;

        allUnits.Remove(info);
        unitsByTeam[info.team].Remove(info);

        if (info.unitType == UnitType.Queen)
            queenDict[info.team].Remove(info);

        grid.RemoveUnit(info);
    }

    /// <summary>
    /// Returns list of all units nearby an ant. 
    /// This includes all units within sight range (20f).
    /// </summary>
    public List<UnitInfo> GetNearbyUnits(Vector3 pos)
    {
        return grid.GetNearbyUnits(pos);
    }

    /// <summary>
    /// Returns all enemy units visible to a particular unit.
    /// </summary>
    public List<UnitInfo> GetVisibleEnemyUnits(Vector3 pos, int friendlyTeam, float sightRange)
    {
        // Get enemy units in the 3x3 neighbourhood
        var nearbyEnemies = grid.GetNearbyEnemies(pos, friendlyTeam);

        // Filter by actual sight range
        float sqrRange = sightRange * sightRange;
        List<UnitInfo> result = new List<UnitInfo>(nearbyEnemies.Count);

        foreach (var enemy in nearbyEnemies)
        {
            Vector3 diff = enemy.transform.position - pos;
            if (diff.sqrMagnitude <= sqrRange)
                result.Add(enemy);
        }

        return result;
    }

    public UnitInfo GetClosestEnemyUnit(Vector3 pos, int friendlyTeam, float sightRange = 20f)
    {
        // Get enemy units in the 3x3 neighbourhood
        var nearbyEnemies = grid.GetNearbyEnemies(pos, friendlyTeam);

        float sqrRange = sightRange * sightRange;
        UnitInfo closest = null;
        float closestDist = float.MaxValue;

        // Find closest enemy within sight range
        foreach (var enemy in nearbyEnemies)
        {
            Vector3 diff = enemy.transform.position - pos;
            float sqrDist = diff.sqrMagnitude;

            if (sqrDist <= sqrRange && sqrDist < closestDist)
            {
                closestDist = sqrDist;
                closest = enemy;
            }
        }

        return closest;
    }

    public UnitInfo GetClosestNonQueenEnemy(Vector3 pos, int friendlyTeam, float sightRange = 20f)
    {
        var nearbyEnemies = grid.GetNearbyEnemies(pos, friendlyTeam);

        float sqrRange = sightRange * sightRange;
        UnitInfo closest = null;
        float closestDist = float.MaxValue;

        // Find closest enemy within sight range
        foreach (var enemy in nearbyEnemies)
        {
            // Ignore queens
            if (enemy.unitType == UnitType.Queen) continue;

            Vector3 diff = enemy.transform.position - pos;
            float sqrDist = diff.sqrMagnitude;

            if (sqrDist <= sqrRange && sqrDist < closestDist)
            {
                closestDist = sqrDist;
                closest = enemy;
            }
        }

        return closest;
    }

    /// <summary>
    /// Returns list of all units within a team.
    /// </summary>
    public List<UnitInfo> GetTeamUnits(int team)
    {
        if (!unitsByTeam.TryGetValue(team, out var list) || list == null)
            return new List<UnitInfo>(); // always return a valid list

        return list;
    }


    /// <summary>
    /// Returns a list of all units in the game.
    /// </summary>
    public List<UnitInfo> GetAllUnits()
    {
        return allUnits;
    }

    public UnitInfo GetInfo(GameObject unit)
    {
        return allUnits.Find(u => u.go == unit);
    }

    /// <summary>
    /// Returns a list of all queen units across all teams.
    /// </summary>
    public List<UnitInfo> GetAllQueens()
    {
        List<UnitInfo> result = new List<UnitInfo>(queenDict.Count);

        foreach (var kvp in queenDict)
        {
            var queens = kvp.Value;
            if (queens != null && queens.Count > 0)
            {
                result.AddRange(queens);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns all queen units that do NOT belong to the given friendly team.
    /// </summary>
    public List<UnitInfo> GetEnemyQueens(int friendlyTeam)
    {
        List<UnitInfo> result = new List<UnitInfo>(queenDict.Count);

        foreach (var kvp in queenDict)
        {
            int team = kvp.Key;
            if (team == friendlyTeam)
                continue;

            var queens = kvp.Value;
            if (queens != null && queens.Count > 0)
                result.AddRange(queens);
        }

        return result;
    }

    /// <summary>
    /// Return list of all ant units.
    /// </summary>
    public List<UnitInfo> GetAllAnts()
    {
        // Return all ants by using the team-based dictionaries.
        // Teams 1-4 will all be ants, team 5 is bugs.
        List<UnitInfo> result = new List<UnitInfo>(200);
        for (int i=1; i<5; i++)
        {
            result.AddRange(GetTeamUnits(i));
        }
        return result;
    }


}