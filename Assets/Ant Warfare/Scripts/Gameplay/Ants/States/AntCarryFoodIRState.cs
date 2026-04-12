using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.AI;
/// <summary>
/// Da plan:
/// If within nest range (10 units?), deposit food to queen, and exit state (normal state decision).
/// Else if pheromone food retreat markers nearby, follow the average target of them.
/// Else generate navmesh path to nest, but and some natural randomness:
/// - Offset destination slightly.
/// - Could mess around with getting corners and then setting that as a 'sub-destination'
/// 
/// But maybe simpler method that avoids needing to perfectly interface with the navmesh corner system:
/// - If close to nest (e.g. within 20 units) just set local destination to nest (with some randomisation)
/// - Else, attempt to set destination to location 10-20 units away that is closer to the nest. Try 20 times, and take the one that gets you the closest.
/// - If no closer destination was found (i.e. current location is about the closest you can possibly get within 10-20 units - unlikely) just set destination to the nest as a fallback.
/// 
/// Just realised what happens when state time runs out -> base state calls DecideNextState -> Need to override UpdateState method and DON'T call the superclass version.
/// Instead could still have a currentDuration timer, but each time it runs out check distance to nest and generate destination again.
/// Only exit state when the food has been deposited.
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

        // TODO: Allow placing of Food Path markers.
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
        }
        else if (queenDist <= 25f)
        {
            // Set destination to the queen position (with some random variation).

            context.UltimateTarget = queenPos;
            context.LocalTarget = context.GetValidPointWithinRange(queenPos, 0f, 5f);
            context.World.SetDestination(context.LocalTarget);
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
        }
    }

    public override void UpdateState(float deltaTime)
    {
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


}
