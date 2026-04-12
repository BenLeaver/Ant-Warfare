using System.Collections;
using System.Collections.Generic;
using UnityEngine;





/// <summary>
/// Manages pheromone markers that can be placed in the world by the player to command other ants.
/// </summary>
public class PheromoneMarkerManager : MonoBehaviour
{


    public GameObject queen;


    //public GameObject[] markerPrefabs;


    //private int[] markersTotal = { 5, 5, 5, 5, 5, 20, 20, 20, 20 };
    //private int[] markersRemaining = { 5, 5, 5, 5, 5, 20, 20, 20, 20};

    //public GameObject[] markerAmountTexts; // Update Singleplayer_UI before deleting
    public Transform placementTransform;

    void Start()
    {
        queen = GetComponent<Player_Singleplayer>().queen;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) RemoveMarker();
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlaceMarker(PheromoneSubtype.UnifiedFollowPath);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlaceMarker(PheromoneSubtype.SoldierAttackPath);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlaceMarker(PheromoneSubtype.WorkerGatheringPath);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlaceMarker(PheromoneSubtype.FoodReturnPath);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlaceMarker(PheromoneSubtype.GuardPoint);
        if (Input.GetKeyDown(KeyCode.T)) RemoveAllMarkers();

        // Note: SearchPath and FoodPath are AI-only -> not bound to keys
    }

    public void PlaceMarker(PheromoneSubtype subtype)
    {
        if (subtype == PheromoneSubtype.GuardPoint)
        {
            queen.GetComponent<ColonyPheromonesManager>().PlacePointMarker(subtype, placementTransform.position, placementTransform.rotation);
        }
        else
        {
            queen.GetComponent<ColonyPheromonesManager>().PlacePathMarker(subtype, placementTransform.position, placementTransform.rotation);
        }
        
        
    }

    public void RemoveMarker()
    {
        queen.GetComponent<ColonyPheromonesManager>().RemoveMarker(placementTransform.position);
    }

    public void RemoveAllMarkers()
    {
        queen.GetComponent<ColonyPheromonesManager>().RemoveAllMarkers();
    }
}
