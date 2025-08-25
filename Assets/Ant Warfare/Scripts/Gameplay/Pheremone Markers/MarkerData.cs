using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores data for a pheromone marker.
/// Markers are used to direct ant behavior for each team and can have different effects.
/// </summary>
public class MarkerData : MonoBehaviour
{
    public int commandNumber = 0;
    public string pheremoneName = "Automatic";
    public bool affectWorkers = true;
    public bool affectSoldiers = true;
    public bool affectFoodCarriers = false;
    public bool isPathMarker = false;
    public int team = -1;
}
