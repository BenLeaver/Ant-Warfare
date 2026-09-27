using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

[System.Serializable]
public class PheromoneInfo
{
    public PheromoneSubtype subtype;
    public GameObject prefab;
    public int total = 5;
    public int remaining = 5;
    public TMP_Text amountText;
}

/// <summary>
/// Manages all pheromone command markers that belong to the Queen's colony.
/// Keeps track of marker limits, placement distance, and provides functionality to remove markers.
/// </summary>
public class ColonyPheromonesManager : MonoBehaviour
{
    /// <summary>
    /// ## Player-placed:
    /// 
    /// 1: UnifiedFollowPath,
    /// 2: SoldierAttackPath,
    /// 3: WorkerGatheringPath,
    /// 4: FoodReturnPath,
    /// 5: GuardPoint,
    /// 
    /// ## Ant-AI
    /// SearchPath,
    /// FoodPath
    /// </summary>

    [Header("Pheromones")]
    public GameObject unifiedFollowPrefab;
    public GameObject soldierAttackPrefab;
    public GameObject workerGatheringPrefab;
    public GameObject foodReturnPrefab;
    public GameObject guardPrefab;
    public GameObject searchPrefab;
    public GameObject foodPrefab;

    private Dictionary<PheromoneSubtype, PheromoneInfo> pheromoneDict;

    public List<GameObject> markers;


    void Awake()
    {
        pheromoneDict = new Dictionary<PheromoneSubtype, PheromoneInfo>
        {
            {
                PheromoneSubtype.UnifiedFollowPath,
                new PheromoneInfo
                {
                    subtype = PheromoneSubtype.UnifiedFollowPath,
                    prefab = unifiedFollowPrefab,
                    total = 40,
                    remaining = 40
                }
            },
            {
                PheromoneSubtype.SoldierAttackPath,
                new PheromoneInfo
                {
                    subtype = PheromoneSubtype.SoldierAttackPath,
                    prefab = soldierAttackPrefab,
                    total = 40,
                    remaining = 40
                }
            },
            {
                PheromoneSubtype.WorkerGatheringPath,
                new PheromoneInfo
                {
                    subtype = PheromoneSubtype.WorkerGatheringPath,
                    prefab = workerGatheringPrefab,
                    total = 40,
                    remaining = 40
                }
            },
            {
                PheromoneSubtype.FoodReturnPath,
                new PheromoneInfo
                {
                    subtype = PheromoneSubtype.FoodReturnPath,
                    prefab = foodReturnPrefab,
                    total = 40,
                    remaining = 40
                }
            },
            {
                PheromoneSubtype.GuardPoint,
                new PheromoneInfo
                {
                    subtype = PheromoneSubtype.GuardPoint,
                    prefab = guardPrefab,
                    total = 40,
                    remaining = 40
                }
            },
            {
                PheromoneSubtype.SearchPath,
                new PheromoneInfo
                {
                    subtype = PheromoneSubtype.SearchPath,
                    prefab = searchPrefab,
                    total = 40,
                    remaining = 40
                }
            },
            {
                PheromoneSubtype.FoodPath,
                new PheromoneInfo
                {
                    subtype = PheromoneSubtype.FoodPath,
                    prefab = foodPrefab,
                    total = 40,
                    remaining = 40
                }
            }
        };
    }

    void LateUpdate()
    {
        // Clean up destroyed markers so the Inspector never sees null entries
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            if (markers[i] == null)
                markers.RemoveAt(i);
        }
    }

    /// <summary>
    /// Called on Start from the CommandButton on the player UI of the pheromone subtype.
    /// Initialises the amountText in pheromoneDict.
    /// </summary>
    public void InitializePlayerAmountText(PheromoneSubtype subtype, TMP_Text amountText)
    {
        var info = pheromoneDict[subtype];
        pheromoneDict[subtype].amountText = amountText;
        pheromoneDict[subtype].amountText.text = $"{info.remaining}/{info.total}";
    }

    /// <summary>
    /// Places a pheremone marker of a specific type, at a given position and rotation, with a target.
    /// 
    /// Only limited numbers of markers from each type can be placed, and markers must be placed at 
    /// least 1 unit away from each other.
    /// </summary>
    public void PlaceMarker(PheromoneSubtype subtype, Vector3 markerPos, Quaternion markerRotation, Vector3 target)
    {
        var info = pheromoneDict[subtype];

        float closestDistance = float.MaxValue;
        foreach (GameObject m in markers)
        {
            float currentDist = Vector3.Distance(markerPos, m.transform.position);
            if (currentDist < closestDistance)
            {
                closestDistance = currentDist;
            }
        }

        if (info.remaining > 0 && (closestDistance >= 1f || closestDistance == float.MaxValue))
        {
            info.remaining--;
            if (info.amountText != null)
                info.amountText.text = $"{info.remaining}/{info.total}";

            GameObject newMarker = Instantiate(info.prefab, markerPos, markerRotation);
            newMarker.GetComponent<MarkerData>().target = target;
            newMarker.GetComponent<MarkerData>().mgr = this;
            markers.Add(newMarker);
        }
    }

    /// <summary>
    /// Places a pheremone marker of a specific type, at a given position and rotation.
    /// 
    /// Ensures there is a valid path to the target point. If not, the distance to the target is reduced until it is.
    /// 
    /// Only limited numbers of markers from each type can be placed, and markers must be placed at 
    /// least 1 unit away from each other.
    /// </summary>
    public void PlacePathMarker(PheromoneSubtype subtype, Vector3 markerPos, Quaternion markerRotation)
    {
        var info = pheromoneDict[subtype];

        float closestDistance = float.MaxValue;
        foreach (GameObject m in markers)
        {
            float currentDist = Vector3.Distance(markerPos, m.transform.position);
            if (currentDist < closestDistance)
            {
                closestDistance = currentDist;
            }
        }

        if (info.remaining > 0 && (closestDistance >= 1f || closestDistance == float.MaxValue))
        {
            info.remaining--;
            if (info.amountText != null)
                info.amountText.text = $"{info.remaining}/{info.total}";

            GameObject newMarker = Instantiate(info.prefab, markerPos, markerRotation);
            Vector3 direction = markerRotation * Vector3.up;

            float targetDist = 20f;
            Vector3 target = markerPos + direction.normalized * targetDist;

            // Reduce target distance by 2 until target is reachable.
            while (targetDist > 0f && !HasValidPath(markerPos, target))
            {
                targetDist -= 2f;
                target = markerPos + direction.normalized * targetDist;
            }

            newMarker.GetComponent<MarkerData>().target = target;

            newMarker.GetComponent<MarkerData>().mgr = this;
            markers.Add(newMarker);
        }
    }

    /// <summary>
    /// Checks whether there exists a valid navmesh path from the start to the end point.
    /// </summary>
    private bool HasValidPath(Vector3 start, Vector3 end)
    {
        // Flatten
        start.z = 0f;
        end.z = 0f;

        // Snap start and end to navmesh
        if (!NavMesh.SamplePosition(start, out NavMeshHit hitStart, 1f, NavMesh.AllAreas))
            return false;

        if (!NavMesh.SamplePosition(end, out NavMeshHit hitEnd, 1f, NavMesh.AllAreas))
            return false;

        NavMeshPath path = new NavMeshPath();
        bool found = NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);
        return found && path.status == NavMeshPathStatus.PathComplete;
    }

    /// <summary>
    /// Places a pheremone marker of a specific type, at a given position and rotation.
    /// 
    /// Assumes the target will be equal to the markers position.
    /// 
    /// Only limited numbers of markers from each type can be placed, and markers must be placed at 
    /// least 1 unit away from each other.
    /// </summary>
    public void PlacePointMarker(PheromoneSubtype subtype, Vector3 markerPos, Quaternion markerRotation)
    {
        var info = pheromoneDict[subtype];

        float closestDistance = float.MaxValue;
        foreach (GameObject m in markers)
        {
            float currentDist = Vector3.Distance(markerPos, m.transform.position);
            if (currentDist < closestDistance)
            {
                closestDistance = currentDist;
            }
        }

        if (info.remaining > 0 && (closestDistance >= 1f || closestDistance == float.MaxValue))
        {
            info.remaining--;
            if (info.amountText != null)
                info.amountText.text = $"{info.remaining}/{info.total}";

            GameObject newMarker = Instantiate(info.prefab, markerPos, markerRotation);
            newMarker.GetComponent<MarkerData>().target = markerPos;
            newMarker.GetComponent<MarkerData>().mgr = this;
            markers.Add(newMarker);
        }
    }

    /// <summary>
    /// Removes the nearest pheremone marker to a point, if it exists.
    /// </summary>
    public void RemoveMarker(Vector3 pos)
    {
        GameObject closestMarker = null;
        float closestDistance = float.MaxValue;
        foreach (GameObject m in markers)
        {
            float currentDist = Vector3.Distance(pos, m.transform.position);
            if (currentDist < closestDistance)
            {
                closestDistance = currentDist;
                closestMarker = m;
            }
        }
        if (closestMarker != null)
        {
            var subtype = closestMarker.GetComponent<MarkerData>().subtype;
            var info = pheromoneDict[subtype];

            info.remaining++;
            if (info.amountText != null)
                info.amountText.text = $"{info.remaining}/{info.total}";

            markers.Remove(closestMarker);
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
            var subtype = m.GetComponent<MarkerData>().subtype;
            var info = pheromoneDict[subtype];

            info.remaining++;
            if (info.amountText != null)
                info.amountText.text = $"{info.remaining}/{info.total}";

            Destroy(m);
        }
        markers.Clear();
    }

    /// <summary>
    /// Removes a specific pheromone marker object.
    /// E.g. used when an AI placed marker loses all strength.
    /// </summary>
    public void RemoveMarkerObject(GameObject m)
    {
        if (m == null) return;

        var subtype = m.GetComponent<MarkerData>().subtype;
        var info = pheromoneDict[subtype];

        info.remaining++;
        if (info.amountText != null)
            info.amountText.text = $"{info.remaining}/{info.total}";
        markers.Remove(m);
        Destroy(m);
    }
}
