using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Ant will move to a completely random location nearby. 
/// 
/// This state is used to try and deal with situations where the ant gets 'stuck' between contradicting commands.
/// </summary>
public class AntConfusedState : BaseAntState
{

    private float IRCheckTimer = 0f;

    public AntConfusedState(AntStateManager manager, AntContext context, AntStateType stateType)
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
            context.UltimateTarget = context.World.Position;
            context.LocalTarget = context.GetValidPointWithinRange(context.World.Position, 3f, 10f);
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
        GameObject closest = context.World.FindClosestEnemy();

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
