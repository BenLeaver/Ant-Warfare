using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all data associated with a single pheromone marker in the world.
/// 
/// Behaviour overview:
/// -   Each marker has a type, subtype, strength, and a target direction.
/// -   Some markers are placed by the player (persistent), others by ants (decay over time).
/// -   Strength decays only for non?player?placed markers, and the marker is removed when strength reaches zero.
/// -   Subtype determines initial strength and whether the marker is considered player?placed.
/// 
/// This component is attached to each pheromone marker prefab and is managed
/// by the ColonyPheromonesManager.
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
