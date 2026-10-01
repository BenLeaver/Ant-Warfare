using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.AI;

/// <summary>
/// Manages the day-night cycle in the game.
/// Controls global lighting, tracks days, 
/// and spawns bugs at specific times.
/// </summary>
public class DayNightManager : MonoBehaviour
{
    [Header("Cycle Settings")]
    public int day = 0;

    [SerializeField] private Light2D globalLight;
    [SerializeField] private Gradient lightColors;
    [SerializeField, Tooltip("Duration of a full day-night cycle (in seconds).")] 
    private float cycleDuration = 120f;

    [Header("Bug Spawning")]
    [SerializeField] private GameObject[] bugPrefabs;
    [SerializeField] private List<Transform> spawnPositions = new List<Transform>();

    private float time = 0;
    private bool bugsSpawned = false;


    /// <summary>
    /// Updates the day-night cycle, adjusts lighting,
    /// and triggers bug spawning at the right time.
    /// </summary>
    void Update()
    {
        time += Time.deltaTime / cycleDuration;
        if (time > 1f) {
            bugsSpawned = false;
            time -= 1f;
        }

        if (!bugsSpawned && time >= 0.5f)
        {
            bugsSpawned = true;
            SpawnBugs();
            day++;
        }
        globalLight.color = lightColors.Evaluate(time);
        globalLight.intensity = Mathf.Lerp(0.5f, 1f, Mathf.Sin((time * Mathf.PI) + Mathf.PI) + 1);
    }

    /// <summary>
    /// Decides which bugs to spawn based on the current day.
    /// </summary>
    void SpawnBugs()
    {
        // Cockroach
        if (day <= 1)
        {
            Spawn(0, 1);
        }
        else if (day == 2)
        {
            Spawn(0, 2);
        }
        else
        {
            Spawn(0, 3);
        }

        // Stag Beetle
        if (day % 2 == 0 && day != 0)
        {
            Spawn(1, 1);
        }
    }

    /// <summary>
    /// Spawns a number of bugs of a given type at random positions.
    /// Each bug will be slightly offset within a circle and have a random rotation.
    /// </summary>
    /// <param name="index">Index of the bug prefab in bugPrefabs array.</param>
    /// <param name="num">Number of bugs to spawn.</param>
    void Spawn(int index, int num)
    {
        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning("No spawn positions assigned!");
            return;
        }

        float spawnRadius = 10f;
        float safeRadius = 2f;           // No ants allowed within this radius
        int maxAttempts = 10;            // Prevent infinite loops if space is crowded

        for (int i = 0; i < num; i++)
        {
            int randomIndex = Random.Range(0, spawnPositions.Count);
            Transform spawnPoint = spawnPositions[randomIndex];
            Vector3 spawnPosition = FindClearSpawnPosition(spawnPoint.position, spawnRadius, safeRadius, maxAttempts);
            
            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            Instantiate(bugPrefabs[index], spawnPosition, spawnRotation);
        }
    }

    /// <summary>
    /// Finds a position near a given point that has no ants within a safe radius.
    /// </summary>
    /// <param name="center">The base spawn position.</param>
    /// <param name="searchRadius">The radius around the center to search within.</param>
    /// <param name="safeRadius">The minimum distance from ants required.</param>
    /// <param name="maxAttempts">How many random tries to make before giving up.</param>
    /// <returns>A clear position for spawning, or the original position if none found.</returns>
    private Vector3 FindClearSpawnPosition(Vector3 center, float searchRadius, float safeRadius, int maxAttempts)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Pick a random position in a circle around the center
            Vector2 offset = Random.insideUnitCircle * searchRadius;
            Vector3 testPosition = center + new Vector3(offset.x, offset.y, 0);

            // 1. Must be on the NavMesh
            Vector3 navPos;
            if (!TryGetNavMeshPosition(testPosition, out navPos))
                continue;

            // 2. Check if there are any ants within the safeRadius
            bool occupied = false;

            var allAnts = UnitManager.Instance.GetAllAnts();
            float safeRadiusSqr = safeRadius * safeRadius;
            foreach (UnitInfo ant in allAnts)
            {
                if (ant.go == null) continue;
                Vector3 diff = ant.transform.position - navPos;
                if (diff.sqrMagnitude < safeRadiusSqr)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                return navPos; // Found a clear spot!
            }
        }

        // Couldn’t find a clear spot — fallback to the original position
        return center;
    }

    private bool TryGetNavMeshPosition(Vector3 position, out Vector3 navPos, float maxDistance=2f)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas))
        {
            navPos = hit.position;
            return true;
        }

        navPos = position;
        return false;
    }

}



