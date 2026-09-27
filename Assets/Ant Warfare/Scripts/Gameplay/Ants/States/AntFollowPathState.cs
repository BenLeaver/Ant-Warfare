using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// State used when an ant is following a pheromone path.
/// 
/// Overview:
/// -   Moves toward the mean pheromone target with slight random variation.
/// -   Subtype determines which interrupts are allowed:
///         0 (default) -> Allow both EnemyIR & FoodIR
///         1 -> Attack-only
///         2 -> Food-only (and Attack interrupt if enemy VERY close)
/// Ant will move towards the mean pheromone target, with some random variation.
/// </summary>
public class AntFollowPathState : BaseAntState
{
    private float IRCheckTimer = 0f;

    public AntFollowPathState(AntStateManager manager, AntContext context, AntStateType stateType)
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
            context.UltimateTarget = target; 
            context.LocalTarget = context.GetValidPointWithinRange(target, 0f, 7.5f);
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

        if (SubType == 0) // Default
        {
            if (CheckEnemyIR()) return true;
            if (CheckFoodIR()) return true;
        }
        else if (SubType == 1) // Attack
        {
            if (CheckEnemyIR()) return true;
        }
        else if (SubType == 2) // Food
        {
            if (CheckEnemyCloseIR()) return true;
            if (CheckFoodIR()) return true;
        }
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

    public bool CheckEnemyCloseIR()
    {
        // Only interrupts if enemy is very close
        UnitInfo closest = context.World.FindClosestEnemy();

        if (closest == null) return false;

        float dist = Vector3.Distance(closest.transform.position, context.World.Position);
        if (dist < context.SightRange / 4)
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
