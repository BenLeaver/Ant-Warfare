using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages pheromone markers that can be placed in the world by the player to command other ants.
/// </summary>
public class PheromoneMarkerManager : MonoBehaviour
{
    public GameObject queen;
    public List<UnitInfo> playerFollowAnts = new List<UnitInfo>(60);
    public Transform placementTransform;

    private ColonyPheromonesManager colonyPheromonesManager;
    private int team;

    void Start()
    {
        queen = GetComponent<Player_Singleplayer>().queen;
        colonyPheromonesManager = queen.GetComponent<ColonyPheromonesManager>();
        team = queen.GetComponent<BaseAntQueenAI>().myInfo.team;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) RemoveMarker();
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlaceMarker(PheromoneSubtype.UnifiedFollowPath);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlaceMarker(PheromoneSubtype.WorkerGatheringPath);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlaceMarker(PheromoneSubtype.SoldierAttackPath);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlaceMarker(PheromoneSubtype.FoodReturnPath);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlaceMarker(PheromoneSubtype.GuardPoint);
        if (Input.GetKeyDown(KeyCode.T)) RemoveAllMarkers();

        // Note: SearchPath and FoodPath are AI-only -> not bound to keys
    }

    public void PlaceMarker(PheromoneSubtype subtype)
    {
        if (subtype == PheromoneSubtype.GuardPoint)
        {
            colonyPheromonesManager.PlacePointMarker(subtype, placementTransform.position, placementTransform.rotation);
        }
        else
        {
            colonyPheromonesManager.PlacePathMarker(subtype, placementTransform.position, placementTransform.rotation);
        }
    }

    public void RemoveMarker()
    {
        colonyPheromonesManager.RemoveMarker(placementTransform.position);
    }

    public void RemoveAllMarkers()
    {
        colonyPheromonesManager.RemoveAllMarkers();
    }


    public void RecruitPlayerFollow()
    {
        float recruitRange = 10f;
        List<UnitInfo> friendlyAnts = UnitManager.Instance.GetTeamUnits(team);

        foreach (UnitInfo a in friendlyAnts)
        {
            // Skip null or destroyed objects
            if (a.go == null)
                continue;

            if (a.team != team)
                continue;

            // Check distance
            float distance = Vector3.Distance(a.transform.position, transform.position);
            if (distance > recruitRange)
                continue;

            if (a.antWorld == null) continue;

            // Avoid duplicates
            if (!playerFollowAnts.Contains(a))
            {
                if (a.antWorld.PlayerFollowStart(2)) playerFollowAnts.Add(a);
            }
        }
    }


    public void DisbandPlayerFollow()
    {
        foreach (UnitInfo a in playerFollowAnts)
        {
            // Skip destroyed ants
            if (a.go == null)
                continue;

            if (a.antWorld != null)
            {
                a.antWorld.PlayerFollowStop();
            }
        }

        playerFollowAnts.Clear();
    }

}
