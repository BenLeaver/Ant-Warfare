using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages pheromone markers for the player. Handles placement, removal, and visibility of markers
/// across the network for multiplayer gameplay.
/// </summary>
public class MPheremoneMarkerManager : NetworkBehaviour
{
    public GameObject[] markerPrefabs;
    public List<GameObject> markers;
    public Transform placementTransform;
    private int[] markersTotal = { 5, 5, 5, 5, 5, 20, 20, 20, 20 };
    private int[] markersRemaining = { 5, 5, 5, 5, 5, 20, 20, 20, 20 };

    public GameObject[] markerAmountTexts;
    private MHealth healthScript;

    // Start is called before the first frame update
    void Start()
    {
        healthScript = GetComponent<MHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.R)) RemoveMarkerServerRpc(healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha0)) PlaceMarkerServerRpc(0, healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlaceMarkerServerRpc(1, healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlaceMarkerServerRpc(2, healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlaceMarkerServerRpc(3, healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlaceMarkerServerRpc(4, healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlaceMarkerServerRpc(5, healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha6)) PlaceMarkerServerRpc(6, healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha7)) PlaceMarkerServerRpc(7, healthScript.team);
        if (Input.GetKeyDown(KeyCode.Alpha8)) PlaceMarkerServerRpc(8, healthScript.team);
        if (Input.GetKeyDown(KeyCode.T)) RemoveAllMarkersServerRpc(healthScript.team);
    }

    /// <summary>
    /// Instantiates and spawns a marker, updating visibility for all clients.
    /// 
    /// Checks that the marker is at least 1 unit away from other markers, and that there are 
    /// markers remaining of that type.
    /// </summary>
    /// <param name="index">The marker's index.</param>
    /// <param name="team">The marker's team.</param>
    [Rpc(SendTo.Server)]
    public void PlaceMarkerServerRpc(int index, int team)
    {
        float closestDistance = 9999f;
        foreach (GameObject m in markers)
        {
            if (m.GetComponent<MarkerData>().team == team)
            {
                float currentDist = Vector3.Distance(placementTransform.position, m.transform.position);
                if (currentDist < closestDistance)
                {
                    closestDistance = currentDist;
                }
            }
        }
        if (markersRemaining[index] > 0 && (closestDistance >= 1f || closestDistance == 9999f))
        {
            UpdateMarkerAmountTextsRpc(index, -1);
            var marker = Instantiate(markerPrefabs[index], placementTransform.position, transform.rotation);
            marker.GetComponent<NetworkObject>().Spawn();
            
            markers.Add(marker);
            UpdateMarkerVisibilityClientRpc(marker.GetComponent<NetworkObject>(), team);
        }
    }

    /// <summary>
    /// Updates the visibility of a marker, by disabling the SpriteRenderer and any particle 
    /// effects if the marker is on a different team to this player.
    /// </summary>
    /// <param name="markerRef">Reference to marker.</param>
    /// <param name="team">Team of the marker.</param>
    [Rpc(SendTo.Everyone)]
    public void UpdateMarkerVisibilityClientRpc(NetworkObjectReference markerRef, int team)
    {
        var marker = ((GameObject)markerRef);
        marker.GetComponent<MarkerData>().team = team;
        if (!IsOwner)
        {
            marker.GetComponent<SpriteRenderer>().enabled = false;
            foreach (Transform child in marker.transform)
            {
                if (child.gameObject.GetComponent<ParticleSystem>())
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Removes the nearest pheremone marker, if it exists.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void RemoveMarkerServerRpc(int team)
    {
        GameObject closestMarker = null;
        float closestDistance = 9999f;
        foreach (GameObject m in markers)
        {
            if (m.GetComponent<MarkerData>().team == team)
            {
                float currentDist = Vector3.Distance(transform.position, m.transform.position);
                if (currentDist < closestDistance)
                {
                    closestDistance = currentDist;
                    closestMarker = m;
                }
            }
        }
        if (closestMarker != null)
        {
            markers.Remove(closestMarker);
            int type = closestMarker.GetComponent<MarkerData>().commandNumber;
            UpdateMarkerAmountTextsRpc(type, 1);
            Destroy(closestMarker);
        }
    }

    /// <summary>
    /// Removes all pheremone markers.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void RemoveAllMarkersServerRpc(int team)
    {
        foreach (GameObject m in markers)
        {
            if (m.GetComponent<MarkerData>().team == team)
            {
                int type = m.GetComponent<MarkerData>().commandNumber;
                UpdateMarkerAmountTextsRpc(type, 1);
                Destroy(m);
            }
        }
        markers = new List<GameObject>();
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateMarkerAmountTextsRpc(int type, int increase)
    {
        markersRemaining[type] += increase;
        if (!IsOwner) return;
        markerAmountTexts[type].GetComponent<TMP_Text>().text = $"{markersRemaining[type]}/{markersTotal[type]}";
    }
}
