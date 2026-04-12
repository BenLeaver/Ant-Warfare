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

    public bool isPathMarker = false;

    public int team = -1;
    public bool affectWorkers = true;
    public bool affectSoldiers = true;
    public bool affectFoodCarriers = false;
    public PheromoneType type;
    public PheromoneSubtype subtype;
    public Vector3 target;
    private bool playerPlaced;

    public float strength = 1f;

    public ColonyPheromonesManager mgr;

    void Start()
    {

        strength = 10f;
        playerPlaced = true;
        if (subtype == PheromoneSubtype.SearchPath)
        {
            playerPlaced = false;
            strength = 0.2f;
        }
        else if (subtype == PheromoneSubtype.FoodPath)
        {
            playerPlaced = false;
            strength = 0.5f;
        }
    }

    void SetTarget(float distance)
    {
        Vector3 direction = transform.rotation * Vector3.up;
        target = transform.position + direction.normalized * distance;
    }

    void Update()
    {
        if (!playerPlaced)
        {
            strength -= 0.01f * Time.deltaTime;
            if (strength <= 0)
            {
                mgr.RemoveMarkerObject(this.gameObject);
            }
        }
    }
}
