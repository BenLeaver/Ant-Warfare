using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Exploration state used when an ant has no strong pheromone guidance.
/// 
/// Overview:
/// - Picks a random direction biased away from the queen to encourage outward exploration.
/// - Moves toward a randomly chosen vaild point within range.
/// - If the chosen target is far enough (>= 10 units), places a SearchPath pheromone 
///   so other ants can follow the exploratory direction.
/// - Periodically checks for interrupts (enemy or food).
/// - Returns to pheromone-based decision making when reaching the target.
/// </summary>
public class AntSearchState : BaseAntState
{
    private float IRCheckTimer = 0f;

    public AntSearchState(AntStateManager manager, AntContext context, AntStateType stateType)
    : base(manager, context, stateType)
    {
    }

    public override void EnterState(Vector3 target = default, int subtype = 0, bool isResuming = false)
    {
        base.EnterState(target, subtype, isResuming);
        maxDuration = 15f;
        currentDuration = 0f;
        IRCheckTimer = 0f;


        if (!isResuming)
        {
            GameObject q = context.World.FindFriendlyQueen();
            Vector3 queenPos = q.transform.position;

            // Bias towards moving further away from the queen.
            // Generate two possible positions, and pick the one further away.
            Vector3 p1 = context.GetValidPointWithinRange(context.World.Position, 3f, 20f);
            float p1Dist = Vector3.Distance(p1, queenPos);
            Vector3 p2 = context.GetValidPointWithinRange(context.World.Position, 3f, 20f);
            float p2Dist = Vector3.Distance(p2, queenPos);

            if (p1Dist > p2Dist)
            {
                context.LocalTarget = p1;
            }
            else
            {
                context.LocalTarget = p2;
            }

            context.UltimateTarget = context.LocalTarget;
            context.FaceLocalTarget();

            float targetDist = Vector3.Distance(context.LocalTarget, context.World.Position);

            if (targetDist >= 10f)
            {
                // If the target is far away place a pheromone so that other ants follow.
                context.World.PlacePheromone(PheromoneSubtype.SearchPath);
            }
            
        }

        context.World.SetDestination(context.LocalTarget);
    }

    public override void UpdateState(float deltaTime)
    {
        base.UpdateState(deltaTime);

        IRCheckTimer += deltaTime;
        if (CheckInterrupts()) return;

        if (Vector3.Distance(context.World.Position, context.LocalTarget) <= 2f)
        {
            manager.DecideNextState();
            return;
        }

        context.FaceLocalTarget();

        // Just double check that the agent is still moving towards the destination.
        // If it was disrupted somehow, and the path is lost, sets destination again.
        if (!context.World.HasActivePath())
        {
            context.World.SetDestination(context.LocalTarget);
        }
    }

    public bool CheckInterrupts()
    {

        // Only check interrupts about every 0.5 seconds.
        if (IRCheckTimer < 0.5f) return false;

        IRCheckTimer = 0f;

        if (CheckEnemyIR()) return true;

        if (CheckFoodIR()) return true;

        return false;
    }

    public bool CheckEnemyIR()
    {
        UnitInfo closest = context.World.FindClosestEnemy();

        if (closest == null) return false;

        float dist = Vector3.Distance(closest.transform.position, context.World.Position);
        if (dist < context.SightRange)
        {
            // Enemy in sight - interrupt.
            manager.PushInterrupt(manager.enemyIRState);
            return true;
        }
        return false;
    }

    public bool CheckFoodIR()
    {
        GameObject closest = context.World.FindClosestFood();

        if (closest == null) return false;

        float dist = Vector3.Distance(closest.transform.position, context.World.Position);
        if (dist < context.SightRange)
        {
            // Food in sight - interrupt.
            manager.PushInterrupt(manager.foodIRState);
            return true;
        }
        return false;
    }
}
