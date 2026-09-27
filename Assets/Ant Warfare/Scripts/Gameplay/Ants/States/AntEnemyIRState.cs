using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Interrupt state triggered when an ant detects an enemy.
/// 
/// Overview:
/// -   Locks onto the closest enemy and continuously updates the target.
/// -   Moves toward the enemy until within attack range.
/// -   Attacks when close enough, respecting the ant's attack cooldown.
/// -   Exits the interrupt if the enemy is lost or leaves sight range.
/// </summary>
public class AntEnemyIRState : BaseAntState
{
    private float enemyUpdateTimer = 0f;

    public AntEnemyIRState(AntStateManager manager, AntContext context, AntStateType stateType)
    : base(manager, context, stateType)
    {
    }

    public override void EnterState(Vector3 target = default, int subtype = 0, bool isResuming = false)
    {
        base.EnterState(target, subtype, isResuming);
        maxDuration = 30f;
        currentDuration = 0f;
        enemyUpdateTimer = 0f;

        context.CurrentEnemyTarget = context.World.FindClosestEnemy();
        context.World.SetDestination(context.CurrentEnemyTarget.transform.position);
    }

    public override void UpdateState(float deltaTime)
    {
        base.UpdateState(deltaTime);
        enemyUpdateTimer += deltaTime;

        // Update the enemy target about two times a second - or sooner if the previous target no longer exists.
        // When an enemy is killed the gameobject is marked as null
        // But the cached enemy UnitInfo in this script is not necessarily marked as null
        if (enemyUpdateTimer > 0.5f || context.CurrentEnemyTarget == null || context.CurrentEnemyTarget.go == null)
        {
            enemyUpdateTimer = 0f;
            if (!UpdateEnemyTarget()) return;   
        }
        
        context.LocalTarget = context.CurrentEnemyTarget.transform.position;
        float enemyDist = Vector3.Distance(context.World.MouthPosition, context.LocalTarget);
        if (enemyDist > context.SightRange)
        {
            // Enemy is outside of sight range -> stop interrupt.
            manager.PopInterrupt();
            return;
        }

        context.FaceLocalTarget();

        if (enemyDist > context.AttackRange)
        {
            // Enemy is outside of attack range -> move towards enemy.
            context.World.SetDestination(context.LocalTarget);
        }
        else
        {
            // Enemy is within attack range -> attack enemy and stand still.
            context.World.StopMovement();
            if (context.AttackTimer > context.AttackDelay)
            {
                context.AttackTimer = 0f;
                context.AttackEnemy();
            }
        }
    }

    private bool UpdateEnemyTarget()
    {
        context.CurrentEnemyTarget = context.World.FindClosestEnemy();
        if (context.CurrentEnemyTarget == null)
        {
            // No enemies -> stop interrupt.
            manager.PopInterrupt();
            return false;
        }
        return true;
    }
}
