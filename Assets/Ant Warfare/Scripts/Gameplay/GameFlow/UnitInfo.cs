using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cached information about a gameobject to be stored in the UnitManager. 
/// 
/// Eliminates the need for GetComponent calls.
/// </summary>
public class UnitInfo
{
    public GameObject go;
    public Transform transform;
    public IHealth health;
    public IAntWorld antWorld; // only for Workers and Soldiers
    public BaseAntQueenAI queenScript; 
    public Player_Singleplayer playerScript;

    public int team;
    public UnitType unitType;

    public Vector2Int cell;
}

public static class UnitInfoExtensions
{
    public static bool IsAliveAndActive(this UnitInfo info)
    {
        if (info == null)
            return false;

        var g = info.go;

        if (g == null)
            return false;

        var h = info.health;

        if (h == null)
            return false;

        if (!g.activeInHierarchy)
            return false;

        return true;
    }
}