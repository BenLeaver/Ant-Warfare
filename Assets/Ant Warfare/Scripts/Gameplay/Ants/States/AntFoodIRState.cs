using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Interrupt state triggered when an ant detects food.
/// 
/// Overview:
/// -   Locks onto the closest valid food object.
/// -   Moves toward it and periodically refreshes the target.
/// -   Exits the interrupt if the food becomes invalid or leaves sight range.
/// -   Picks up the food when within pickup range, then transitions into the
///     food-carrying interrupt state.
/// </summary>
public class AntFoodIRState : BaseAntState
{
    private float foodUpdateTimer = 0f;
    private float IRCheckTimer = 0f;

    public AntFoodIRState(AntStateManager manager, AntContext context, AntStateType stateType)
    : base(manager, context, stateType)
    {
    }

    public override void EnterState(Vector3 target = default, int subtype = 0, bool isResuming = false)
    {
        base.EnterState(target, subtype, isResuming);
        maxDuration = 30f;
        currentDuration = 0f;
        foodUpdateTimer = 0f;
        IRCheckTimer = 0f;

        context.CurrentFoodTarget = context.World.FindClosestFood();
        // Check the food is still valid before attempting to move towards it.
        if (context.World.CheckValid(context.CurrentFoodTarget))
        {
            context.LocalTarget = context.CurrentFoodTarget.transform.position;
            context.World.SetDestination(context.CurrentFoodTarget.transform.position);
        }
    }

    public override void UpdateState(float deltaTime)
    {
        base.UpdateState(deltaTime);

        IRCheckTimer += deltaTime;
        if (CheckInterrupts()) return;


        foodUpdateTimer += deltaTime;

        GameObject food = context.CurrentFoodTarget;
        if (foodUpdateTimer > 1f || !context.World.CheckValid(food))
        {
            // Update food about once a second - or sooner if food is invalid (can't be picked up).
            foodUpdateTimer = 0f;
            if (!UpdateFoodTarget()) return;
            context.LocalTarget = context.CurrentFoodTarget.transform.position;
            context.World.SetDestination(context.LocalTarget);
        }


        float foodDist = Vector3.Distance(context.World.MouthPosition, food.transform.position);
        if (foodDist > context.SightRange)
        {
            // Food is outside of sight range -> stop interrupt.
            manager.PopInterrupt();
            return;
        }

        context.FaceLocalTarget();

        if (foodDist <= context.PickupRange)
        {
            // Pickup Food and enter food carry state
            context.World.FoodPickup(food);
            manager.HardInterrupt(manager.carryFoodIRState);
        }
    }

    private bool UpdateFoodTarget()
    {
        context.CurrentFoodTarget = context.World.FindClosestFood();
        if (context.CurrentFoodTarget == null)
        {
            // No food -> stop interrupt.
            manager.PopInterrupt();
            return false;
        }
        return true;
    }

    public bool CheckInterrupts()
    {
        if (IRCheckTimer < 0.5f) return false;
        IRCheckTimer = 0f;

        if (CheckEnemyCloseIR()) return true;
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
}
