using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

public class AntFoodIRState : BaseAntState
{
    private float foodUpdateTimer = 0f;

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

        context.CurrentFoodTarget = context.World.FindClosestFood();
        // Check the food is still valid before attempting to move towards it.
        if (context.World.CheckValid(context.CurrentFoodTarget))
        {
            context.World.SetDestination(context.CurrentFoodTarget.transform.position);
        }
    }

    public override void UpdateState(float deltaTime)
    {
        base.UpdateState(deltaTime);
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


        float foodDist = Vector3.Distance(context.World.Position, context.LocalTarget);
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
}
