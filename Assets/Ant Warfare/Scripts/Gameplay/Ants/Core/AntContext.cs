using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Essentially functions as memory for the state manager. 
/// Therefore not tied to singleplayer or multiplayer implementation.
/// Centralizes data from IAntWorld.
/// 
/// Stores the LocalTarget (where the ant is actually moving towards) and the UltimateTarget, 
/// which is the final destination of the ant in its current state.
/// </summary>
public class AntContext
{
    public IAntWorld World;

    public AntType Type;

    public Vector3 LocalTarget;
    public Vector3 UltimateTarget;

    public UnitInfo CurrentEnemyTarget;
    public GameObject CurrentFoodTarget;

    public float AttackTimer;
    public float RandomTimer;

    public float SightRange => World.SightRange;
    public float AttackRange => World.AttackRange;
    public float AttackDelay => World.AttackDelay;
    public float PickupRange => World.PickupRange;

    public AntContext(IAntWorld world)
    {
        this.World = world;
    }

    public Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3 target)>> EvaluatePheromones()
    {
        return World.GetPheromonesNearby();
    }

    /// <summary>
    /// Returns the total weight and mean target of the nearby food return path pheromones.
    /// </summary>
    public (float totalWeight, Vector3 meanTarget) GetFoodReturnPathTarget()
    {
        float total = 0f;
        Vector3 weightedSum = Vector3.zero;


        foreach (var kvp in World.GetFoodReturnPathPheromones())
        {
            float w = kvp.Key;
            var (subtype, target) = kvp.Value;

            total += w;
            weightedSum += target * w;
        }

        Vector3 meanTarget = total > 0f ? weightedSum / total : Vector3.zero;
        return (total, meanTarget);
    }

    /// <summary>
    /// Rotate the ant to face a point.
    /// </summary>
    /// <param name="point"></param>
    public void FacePoint(Vector3 point)
    {
        Vector3 direction = point - World.Position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        World.SetRotation(direction);
    }

    /// <summary>
    /// Rotate the ant to face the local target.
    /// </summary>
    public void FaceLocalTarget()
    {
        FacePoint(LocalTarget);
    }

    /// <summary>
    /// Attack the CurrentEnemyTarget.
    /// </summary>
    public void AttackEnemy()
    {
        World.Attack(CurrentEnemyTarget);
    }

    /// <summary>
    /// Given an origin, returns a reachable point on the navmesh within 
    /// the minimum and maximum range.
    /// 
    /// If no reachable point was found, just returns this ant's current world position.
    /// <returns></returns>
    public Vector3 GetValidPointWithinRange(Vector3 origin, float minRange, float maxRange)
    {
        return World.GetReachableNavMeshPoint(origin, minRange, maxRange);
    }
}
