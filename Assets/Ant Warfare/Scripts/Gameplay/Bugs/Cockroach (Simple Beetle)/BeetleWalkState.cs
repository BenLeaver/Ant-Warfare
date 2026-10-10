using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State where the beetle roams to random NavMesh points.
/// </summary>
public class BeetleWalkState : BeetleBaseState
{
    public BeetleWalkState(BeetleStateManager beetle) : base(beetle) { }

    /// <summary>
    /// Attempts to choose a random navigable destination.
    /// </summary>
    public override void EnterState()
    {
        manager.currentStateName = "Walk";
        manager.ChangeSpeed(manager.walkSpeed);
        
        Vector3 randomPoint = manager.GetReachableNavMeshPoint(manager.transform.position, 3f, 8f);
        if (randomPoint == manager.transform.position)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }
        else
        {
            manager.Agent.SetDestination(randomPoint);
        }
    }

    /// <summary>
    /// Returns to idle when the beetle finishes walking.
    /// </summary>
    public override void UpdateState()
    {
        if (!manager.Agent.pathPending && manager.Agent.remainingDistance <= 1f)
        {
            manager.ChangeState(manager.IdleState);
        }
    }


    /// <summary>
    /// Takes damage => switch to fast retreat.
    /// </summary>
    public override void OnDamageTaken()
    {
        manager.ChangeState(manager.FastRetreatState);
    }
}
