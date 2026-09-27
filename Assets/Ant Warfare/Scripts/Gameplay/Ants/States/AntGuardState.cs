using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Guard behavior state.
/// 
/// Overview:
/// -   When first entered, the ant idles for 2-5 seconds.
/// -   After idling, it moves towards a random point near the assigned guard target.
/// -   If interrupted or later resumed, the idle period is not repeated.
/// -   Periodically checks for enemy interrupts.
/// -   Decides next state after reaching the local target.
/// </summary>
public class AntGuardState : BaseAntState
{

    private float IRCheckTimer = 0f;
    private float idleTime;

    public AntGuardState(AntStateManager manager, AntContext context, AntStateType stateType)
    : base(manager, context, stateType)
    {
    }

    public override void EnterState(Vector3 target = default, int subtype = 0, bool isResuming = false)
    {
        base.EnterState(target, subtype, isResuming);
        maxDuration = 15f;
        IRCheckTimer = 0f;

        if (!isResuming)
        {
            currentDuration = 0f;
            context.UltimateTarget = target;
            context.LocalTarget = context.GetValidPointWithinRange(target, 0f, 10f);
            idleTime = Random.Range(2f, 5f);
            context.World.StopMovement();
        }
    }

    public override void UpdateState(float deltaTime)
    {
        base.UpdateState(deltaTime);

        IRCheckTimer += deltaTime;
        if (CheckInterrupts()) return;

        // Ant will stay still for the first few seconds, and will then move towards a random position nearby.
        if (currentDuration > idleTime)
        {
            if (!context.World.HasActivePath())
            {
                context.World.SetDestination(context.LocalTarget);
            }

            context.FaceLocalTarget();

            if (Vector3.Distance(context.World.Position, context.LocalTarget) <= 2f)
            {
                manager.DecideNextState();
                return;
            }
        }
    }

    public bool CheckInterrupts()
    {
        // Only check interrupts about every 0.5 seconds.
        if (IRCheckTimer < 0.5f) return false;

        IRCheckTimer = 0f;
        if (CheckEnemyIR()) return true;

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
}
