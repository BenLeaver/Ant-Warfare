using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Essentially functions as memory for the state manager. 
/// Therefore not tied to singleplayer or multiplayer implementation.
/// Centralizes data from IAntWorld.
/// </summary>
public class AntContext
{
    public IAntWorld World;

    public AntType Type;

    public Vector3 LocalTarget;
    public Vector3 UltimateTarget;

    public GameObject CurrentEnemyTarget;
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
        //1. Check Pheromone applies
        //2. Add to dict, including pheromone target and weight
        //3. Check the greatest weight sum passes threshold -> If not allow placing and transition to search state.
        //4. Calculate the mean target pos of greatest pheromone type.
        //5. Transition to state given by pheromone type.

        return World.GetPheromonesNearby();
    }

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


    public void FacePoint(Vector3 point)
    {
        Vector3 direction = point - World.Position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        World.SetRotation(direction);
    }

    public void FaceLocalTarget()
    {
        FacePoint(LocalTarget);
    }

    public void AttackEnemy()
    {
        World.Attack(CurrentEnemyTarget);
    }

    public Vector3 GetValidPointWithinRange(Vector3 origin, float minRange, float maxRange)
    {
        return World.GetReachableNavMeshPoint(origin, minRange, maxRange);
    }
}
