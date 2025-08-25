using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages pheromone markers that can be placed in the world by the player to command other ants.
/// Keeps track of marker limits, placement distance, and provides functionality to remove markers.
/// </summary>
public class PheremoneMarkerManager : MonoBehaviour
{

    public GameObject[] markerPrefabs;
    public List<GameObject> markers;
    public Transform placementTransform;
    private int[] markersTotal = { 5, 5, 5, 5, 5, 20, 20, 20, 20 };
    private int[] markersRemaining = { 5, 5, 5, 5, 5, 20, 20, 20, 20};

    public GameObject[] markerAmountTexts;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) RemoveMarker();
        if (Input.GetKeyDown(KeyCode.Alpha0)) PlaceMarker(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlaceMarker(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlaceMarker(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlaceMarker(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlaceMarker(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlaceMarker(5);
        if (Input.GetKeyDown(KeyCode.Alpha6)) PlaceMarker(6);
        if (Input.GetKeyDown(KeyCode.Alpha7)) PlaceMarker(7);
        if (Input.GetKeyDown(KeyCode.Alpha8)) PlaceMarker(8);
        if (Input.GetKeyDown(KeyCode.T)) RemoveAllMarkers();
    }

    /// <summary>
    /// Places a pheremone marker of a specific type, given by a number from 0 to 5.
    /// 
    /// Only limited numbers of markers from each type can be placed, and markers must be placed at 
    /// least 1 unit away from each other.
    /// </summary>
    /// <param name="i">Represents the pheremone index to use.</param>
    public void PlaceMarker(int i)
    {
        float closestDistance = 9999f;
        foreach (GameObject m in markers)
        {
            float currentDist = Vector3.Distance(placementTransform.position, m.transform.position);
            if (currentDist < closestDistance)
            {
                closestDistance = currentDist;
            }
        }
        if (markersRemaining[i] > 0 && (closestDistance >= 1f || closestDistance == 9999f))
        {
            markersRemaining[i] -= 1;
            markerAmountTexts[i].GetComponent<TMP_Text>().text = $"{markersRemaining[i]}/{markersTotal[i]}";
            GameObject newMarker = Instantiate(markerPrefabs[i], placementTransform.position,
                transform.rotation);
            markers.Add(newMarker);
        }
    }

    /// <summary>
    /// Removes the nearest pheremone marker, if it exists.
    /// </summary>
    public void RemoveMarker()
    {
        GameObject closestMarker = null;
        float closestDistance = 9999f;
        foreach (GameObject m in markers)
        {
            float currentDist = Vector3.Distance(transform.position, m.transform.position);
            if (currentDist < closestDistance)
            {
                closestDistance = currentDist;
                closestMarker = m;
            }
        }
        if (closestMarker != null)
        {
            markers.Remove(closestMarker);
            int type = closestMarker.GetComponent<MarkerData>().commandNumber;
            markersRemaining[type] += 1;
            markerAmountTexts[type].GetComponent<TMP_Text>().text = $"{markersRemaining[type]}/{markersTotal[type]}";
            Destroy(closestMarker);
        }
    }

    /// <summary>
    /// Removes all pheremone markers.
    /// </summary>
    public void RemoveAllMarkers()
    {
        foreach (GameObject m in markers)
        {
            int type = m.GetComponent<MarkerData>().commandNumber;
            markersRemaining[type] += 1;
            markerAmountTexts[type].GetComponent<TMP_Text>().text = $"{markersRemaining[type]}/{markersTotal[type]}";
            Destroy(m);
        }
        markers = new List<GameObject>();
    }
}
