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
    private float pathUpdateTimer = 0f;
    private Vector3 lastPathTarget;
    private Vector3 lastPathDirection;

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
        pathUpdateTimer = 0f;

        context.DisableAutoRotation();
        context.CurrentEnemyTarget = context.World.FindClosestEnemy();

        if (context.CurrentEnemyTarget == null)
        {
            manager.PopInterrupt();
            context.EnableAutoRotation();
            return;
        }

        context.LocalTarget = context.CurrentEnemyTarget.transform.position;
        context.World.SetDestination(context.LocalTarget);
        lastPathTarget = context.LocalTarget;
        lastPathDirection = (context.LocalTarget - context.World.Position).normalized;
    }

    public override void UpdateState(float deltaTime)
    {
        base.UpdateState(deltaTime);
        enemyUpdateTimer += deltaTime;
        pathUpdateTimer += deltaTime;

        // Update the enemy target about once a second - or sooner if the previous target no longer exists.
        // When an enemy is killed the gameobject is marked as null
        // But the cached enemy UnitInfo in this script is not necessarily marked as null
        if (enemyUpdateTimer > 1f || context.CurrentEnemyTarget == null || context.CurrentEnemyTarget.go == null)
        {
            enemyUpdateTimer = 0f;
            if (!UpdateEnemyTarget())
            {
                context.EnableAutoRotation();
                return;
            }
        }

        Vector3 enemyPosition = context.CurrentEnemyTarget.transform.position;

        float enemyDist = Vector3.Distance(context.World.MouthPosition, enemyPosition);
        if (enemyDist > context.SightRange)
        {
            // Enemy is outside of sight range -> stop interrupt.
            manager.PopInterrupt();
            context.EnableAutoRotation();
            return;
        }

        if (enemyDist > context.AttackRange)
        {
            // Enemy is outside of attack range -> move towards enemy.
            if (pathUpdateTimer >= 0.25f)
            {
                pathUpdateTimer = 0f;
                // Recalculate only when enemy has moved far enough
                if (ShouldRepath(enemyDist, enemyPosition))
                {
                    context.LocalTarget = enemyPosition;
                    context.World.SetDestination(enemyPosition);
                    lastPathTarget = enemyPosition;
                    lastPathDirection = (enemyPosition - context.World.Position).normalized;
                }
            }
        }
        else
        {
            // Enemy is within attack range -> attack enemy and stand still.

            if (context.World.HasActivePath())
            {
                context.World.StopMovement();
            }
            
            if (context.AttackTimer > context.AttackDelay)
            {
                context.AttackTimer = 0f;
                context.AttackEnemy();
            }
        }

        context.FacePoint(enemyPosition);
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

    private bool ShouldRepath(float enemyDist, Vector3 enemyPos)
    {
        float moveThreshold = Mathf.Lerp(0.2f, 2.0f, Mathf.Clamp01(enemyDist / 10f));

        bool movedEnough = Vector3.Distance(
            enemyPos,
            lastPathTarget)
            > moveThreshold;

        Vector3 currentDirection = (enemyPos - context.World.Position).normalized;
        float angleChange = Vector3.Angle(lastPathDirection, currentDirection);
        bool angleChanged = angleChange > 15f;

        return movedEnough || angleChanged;
    }
}
