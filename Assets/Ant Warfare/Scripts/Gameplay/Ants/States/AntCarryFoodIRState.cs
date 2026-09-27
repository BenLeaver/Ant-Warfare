 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// State used when the ant is carrying food back towards the nest.
/// 
/// Overview:
/// -   Continuously moves ant closer to the queen/nest.
/// -   Follows existing food-return pheromone paths when strong enough.
/// -   Otherwise selects a destination that reduces distance to the queen.
/// -   Can place food path pheromones pointing back towards the food source.
/// -   Self-loops until the ant reaches the queen or is interrupted.
/// </summary>
public class AntCarryFoodIRState : BaseAntState
{
    private float foodReturnMajorityThreshold = 5f;

    public AntCarryFoodIRState(AntStateManager manager, AntContext context, AntStateType stateType)
    : base(manager, context, stateType)
    {
    }

    public override void EnterState(Vector3 target = default, int subtype = 0, bool isResuming = false)
    {
        base.EnterState(target, subtype, isResuming);
        maxDuration = 10f;
        currentDuration = 0f;

        InitializeDestination();
    }

    private void InitializeDestination()
    {
        GameObject q = context.World.FindFriendlyQueen();
        Vector3 queenPos = q.transform.position;
        float queenDist = context.World.GetFriendlyQueenDist();
        var stats = context.GetFoodReturnPathTarget();

        if (queenDist <= 5f)
        {
            // Deposit food and exit state.
            context.World.CheckInNest();
            manager.DecideNextState();
            return;
        }
        else if (stats.totalWeight > foodReturnMajorityThreshold)
        {
            // Follow nearby food return path pheromones.

            context.UltimateTarget = stats.meanTarget;
            context.LocalTarget = context.GetValidPointWithinRange(stats.meanTarget, 0f, 5f);
            context.World.SetDestination(context.LocalTarget);
            TryPlaceFoodPathMarker();
        }
        else if (queenDist <= 25f)
        {
            // Set destination to the queen position (with some random variation).

            context.UltimateTarget = queenPos;
            context.LocalTarget = context.GetValidPointWithinRange(queenPos, 0f, 5f);
            context.World.SetDestination(context.LocalTarget);
            TryPlaceFoodPathMarker();
        }
        else
        {
            // Attempt to set destination to location 10-25 units away, that is closer to the queen.
            // Try 20 times, and take the one that gets you the closest.
            // If no destination closer than the current position was found, just set the destination to the queen position (with some random variation).
            context.UltimateTarget = queenPos;

            Vector3 bestPos = context.World.Position;
            float bestDist = queenDist;
            for (int i = 0; i < 20; i++)
            {
                Vector3 candidate = context.GetValidPointWithinRange(context.World.Position, 10f, 25f);
                float dist = Vector3.Distance(queenPos, candidate);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPos = candidate;
                }
            }

            if (bestDist >= queenDist)
            {
                bestPos = context.GetValidPointWithinRange(queenPos, 0f, 5f);
            }

            context.LocalTarget = bestPos;
            context.World.SetDestination(context.LocalTarget);
            TryPlaceFoodPathMarker();
        }
    }

    public override void UpdateState(float deltaTime)
    {
        // Check whether ant is able to deposit food
        float queenDist = context.World.GetFriendlyQueenDist();

        if (queenDist <= 5f)
        {
            // Deposit food and exit state.
            context.World.CheckInNest();
            manager.DecideNextState();
            return;
        }

        context.FaceLocalTarget();

        currentDuration += deltaTime;
        if (currentDuration >= maxDuration || Vector3.Distance(context.World.Position, context.LocalTarget) <= 2f)
        {
            // Self-loop -> Enter this state again.
            manager.ChangeState(manager.carryFoodIRState);
            return;
        }

        // Just double check that the agent is still moving towards the destination.
        // If it was disrupted somehow, and the path is lost, sets destination again.
        if (!context.World.HasActivePath())
        {
            context.World.SetDestination(context.LocalTarget);
        }
    }

    private void TryPlaceFoodPathMarker()
    {
        // Check pheromone density
        if (!manager.CanPlacePheromones()) return;

        // Compute direction opposite to the ant's movement direction
        // Ant is moving from antPos -> LocalTarget
        // So pheromone should point from LocalTarget -> antPos

        Vector3 antPos = context.World.Position;
        Vector3 localTarget = context.LocalTarget;

        Vector2 movementDir = (localTarget - antPos).normalized;
        Vector3 oppositeDir = -movementDir;

        // Convert direction to rotation
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, oppositeDir);

        // Place pheromone at ants current position
        context.World.PlacePheromone(PheromoneSubtype.FoodPath, rot);
    }


}
