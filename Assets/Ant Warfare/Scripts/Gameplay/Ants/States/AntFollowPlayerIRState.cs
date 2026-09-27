using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Interrupt state used when an ant is following the player.
/// 
/// Overview:
/// -   Continuously moves toward a point near the player, with follow range increasing as the colony grows (prevents crowding).
/// -   Periodically updates the target position to stay near the player.
/// -   Subtype determines which interrupts are allowed:
///     
///     Subtype     Allowed Interrupts
///     0 (strict)  None — always follow player
///     1 (food)    EnemyIR + FoodIR
///     2 (attack)  EnemyIR only
///     
/// </summary>
public class AntFollowPlayerIRState : BaseAntState
{

    /// <summary>
    /// 
    /// Subtype         Interrupts Allowed
    /// 0 (strict)      None - follows player no matter what
    /// 1 (food)        Attack & Food
    /// 2 (attack)      Attack
    /// 
    /// </summary>

    private float IRCheckTimer = 0f;
    private float updateTargetTimer = 0f;
    private GameObject player;

    public AntFollowPlayerIRState(AntStateManager manager, AntContext context, AntStateType stateType)
    : base(manager, context, stateType)
    {
    }

    public override void EnterState(Vector3 target=default, int subtype=0, bool isResuming=false)
    {
        base.EnterState(target, subtype, isResuming);
        IRCheckTimer = 0f;
        updateTargetTimer = 0f;
        player = context.World.FindFriendlyPlayer();
        UpdateTarget();
    }

    public override void UpdateState(float deltaTime)
    {
        IRCheckTimer += deltaTime;
        updateTargetTimer += deltaTime;
        if (CheckInterrupts()) return;

        context.FaceLocalTarget();

        if (updateTargetTimer >= 1f)
        {
            updateTargetTimer = 0f;
            UpdateTarget();
        }
    }

    private void UpdateTarget()
    {
        // Allow follow range to expand as colony gets bigger.
        // Should allow ants to distribute more evenly without crowding the player.
        int size = context.World.GetFriendlyColonySize();
        float followRange = 5f + (size / 5);
        context.UltimateTarget = player.transform.position;
        context.LocalTarget = context.GetValidPointWithinRange(player.transform.position, 0f, followRange);
        context.World.SetDestination(context.LocalTarget);
    }

    public bool CheckInterrupts()
    {
        // Only check interrupts about every 0.5 seconds.
        if (IRCheckTimer < 0.5f) return false;

        IRCheckTimer = 0f;

        if (SubType != 0)
        {
            if (CheckEnemyIR()) return true;
        }

        if (SubType == 1)
        {
            if (CheckFoodIR()) return true;
        }

        return false;
    }

    public bool CheckEnemyIR()
    {
        UnitInfo closest = context.World.FindClosestEnemy();

        if (closest == null) return false;

        float dist = Vector3.Distance(closest.transform.position, context.World.Position);
        if (dist < context.SightRange / 2)
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
        if (dist < context.SightRange / 2)
        {
            // Food in sight - interrupt.
            manager.PushInterrupt(manager.foodIRState);
            return true;
        }
        return false;
    }
}
