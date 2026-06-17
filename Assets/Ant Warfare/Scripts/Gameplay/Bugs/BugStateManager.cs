using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A StateManager used to represent a unit in the game e.g. Beetles.
/// 
/// Handles registering and unregistering in UnitManager, initialising a navmesh agent, rotating towards the navmesh 
/// target, forwards taking damage to states, and handles spawning food and deleting the game object on death.
/// 
/// Also contains useful utility methods to use when pathfinding with the navmesh.
/// 
/// </summary>
public class BugStateManager : StateManager
{
    public enum RotationMode
    {
        MovementDirection,
        ClosestEnemy,
        None
    }

    //[Header("NavMesh & Movement")]
    public UnityEngine.AI.NavMeshAgent Agent;
    private NavMeshPath _path;
    public Animator anim;
    public float rotationSpeed = 5f;

    //[Header("Death")]
    public GameObject foodPrefab;
    public int foodAmount = 5;
    public float dropRadius = 2f;

    protected RotationMode currentRotationMode = RotationMode.MovementDirection;

    protected virtual void Awake()
    {
        Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        _path = new NavMeshPath();
    }

    private void OnEnable()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.RegisterUnit(gameObject);
        }
    }

    private void OnDisable()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.UnregisterUnit(gameObject);
        }
    }

    public override void Update()
    {
        base.Update();
    }

    protected void LateUpdate()
    {
        switch (currentRotationMode)
        {
            case RotationMode.MovementDirection:
                RotateTowardsMovementTarget();
                break;
            case RotationMode.ClosestEnemy:
                RotateTowardsClosestEnemy();
                break;
            case RotationMode.None:
                break;
        }
    }

    public void SetRotationMode(RotationMode mode)
    {
        currentRotationMode = mode;
    }

    /// <summary>
    /// Smoothly rotates the to face its NavMesh steering target.
    /// </summary>
    protected void RotateTowardsMovementTarget()
    {
        Vector3 direction = Agent.steeringTarget - transform.position;
        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    protected void RotateTowardsClosestEnemy()
    {
        GameObject enemy = FindNearestEnemy();

        if (enemy == null) return;

        Vector3 direction = enemy.transform.position - transform.position;

        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    /// <summary>
    /// Finds the nearest enemy unit based on team in SHealth.
    /// </summary>
    /// <returns>The closest enemy, or null if none exist.</returns>
    public GameObject FindNearestEnemy()
    {
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject a in UnitManager.Instance.AllUnits)
        {
            if (a.GetComponent<SHealth>())
            {
                if (a.GetComponent<SHealth>().team != GetComponent<SHealth>().team)
                {
                    float dist = Vector3.Distance(transform.position, a.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = a;
                    }
                }
            }
        }
        return nearest;
    }

    /// <summary>
    /// Finds the nearest enemy that is not a queen.
    /// </summary>
    /// <returns></returns>
    public GameObject FindNearestNonQueenEnemy()
    {
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject a in UnitManager.Instance.AllUnits)
        {
            if (a.GetComponent<BaseAntQueenAI>())
            {
                continue;
            }

            if (a.GetComponent<SHealth>())
            {
                if (a.GetComponent<SHealth>().team != GetComponent<SHealth>().team)
                {
                    float dist = Vector3.Distance(transform.position, a.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = a;
                    }
                }
            }
        }
        return nearest;
    }

    public float FindNearestEnemyDist()
    {
        float minDist = Mathf.Infinity;
        foreach (GameObject a in UnitManager.Instance.AllUnits)
        {
            SHealth health = a.GetComponent<SHealth>();

            if (!health || health.team != GetComponent<SHealth>().team)
            {
                float dist = Vector3.Distance(transform.position, a.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                }
            }
        }
        return minDist;
    }

    /// <summary>
    /// Finds the furthest enemy within a given range, excluding a specific enemy.
    /// </summary>
    public GameObject FindFurthestEnemyInRange(float maxRange, float minRange, GameObject exclude)
    {
        GameObject furthest = null;
        float maxDist = 0f;

        foreach (GameObject a in UnitManager.Instance.AllUnits)
        {
            if (a == exclude) continue;

            SHealth health = a.GetComponent<SHealth>();

            if (!health || health.team == GetComponent<SHealth>().team)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, a.transform.position);

            if (dist <= maxRange && dist > maxDist && dist >= minRange)
            {
                maxDist = dist;
                furthest = a;
            }
        }
        return furthest;
    }

    /// <summary>
    /// Forwards damage events to the active state.
    /// </summary>
    public void DamageTaken()
    {
        currentState.OnDamageTaken();
    }

    /// <summary>
    /// Spawns food items around the beetle and removes it from the game.
    /// </summary>
    public void Death()
    {
        for (int i = 0; i < foodAmount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
            Vector3 randomPos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);
            Instantiate(foodPrefab, randomPos, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Returns whether the navmesh agent can find a path to the specified target point.
    /// </summary>
    /// <param name="target">Vector3 representing the target coordinates.</param>
    /// <returns></returns>
    public bool HasValidPath(Vector3 target)
    {
        if (!Agent.isOnNavMesh) return false;

        NavMesh.CalculatePath(Agent.transform.position, target, NavMesh.AllAreas, _path);
        return _path.status == NavMeshPathStatus.PathComplete;
    }

    public Vector3 GetReachableNavMeshPoint(Vector3 origin, float minRange, float maxRange,
    int attempts = 50, float snapDistance = 2f)
    {
        if (!Agent.isOnNavMesh)
        {
            Debug.LogWarning("Bug is not on NavMesh");
            return transform.position;
        }
        for (int i = 0; i < attempts; i++)
        {
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            float dist = Random.Range(minRange, maxRange);

            Vector3 candidate =
                origin + new Vector3(dir2D.x, dir2D.y, 0f) * dist;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, snapDistance, Agent.areaMask))
                continue;

            Vector3 flattened = new Vector3(hit.position.x, hit.position.y, 0f);

            if (HasValidPath(flattened))
                return flattened;
        }
        Debug.LogWarning("No valid navmesh point found");
        return transform.position;
    }

    /// <summary>
    /// Attempts to snap a point to the nearest NavMesh location.
    /// </summary>
    public bool TryGetNavmeshPointNear(Vector3 point, float snapDistance, out Vector3 result)
    {
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, snapDistance, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = point;
        return false;
    }

    /// <summary>
    /// Attempts to generate a retreat point that moves the beetle away from the nearest enemy.
    /// 
    /// The method biases the retreat direction away from the threat and searches for a reachable
    /// NavMesh position within a retreat distance band. If no valid retreat point can be found,
    /// the beetle's current position is returned and a warning is logged.
    /// </summary>
    /// <param name="maxAttempts">
    /// Number of sampling attempts used when searching for a valid retreat point.
    /// </param>
    /// <returns>
    /// A reachable retreat position, or the beetle's current position if none could be found.
    /// </returns>
    public Vector3 GetRetreatPoint(int maxAttempts = 20)
    {
        GameObject enemy = FindNearestEnemy();

        // No enemy: fallback to a generic movement point
        if (enemy == null)
        {
            Vector3 fallback =
                GetReachableNavMeshPoint(transform.position, 3f, 8f, maxAttempts);

            if (fallback == transform.position)
            {
                Debug.LogWarning(
                    $"[{name}] Retreat fallback failed (no enemy): no reachable NavMesh point found.");
            }

            return fallback;
        }

        // Direction away from the enemy
        Vector3 away = transform.position - enemy.transform.position;

        if (away.sqrMagnitude < 0.01f)
            away = Random.insideUnitCircle.normalized;
        else
            away.Normalize();

        // Bias the origin slightly away from the enemy to reduce bad samples
        Vector3 biasedOrigin = transform.position + away * 0.5f;

        Vector3 retreatPoint =
            GetReachableNavMeshPoint(
                biasedOrigin,
                minRange: 6f,
                maxRange: 12f,
                attempts: maxAttempts
            );

        if (retreatPoint == transform.position)
        {
            Debug.LogWarning(
                $"[{name}] Retreat failed: no valid reachable retreat point found. " +
                $"Enemy: {enemy.name}");
        }

        return retreatPoint;
    }
}
