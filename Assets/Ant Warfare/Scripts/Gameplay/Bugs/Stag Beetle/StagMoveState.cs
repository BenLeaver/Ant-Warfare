using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State where the stag beetle roams to random NavMesh points.
/// </summary>
public class StagMoveState : StagBaseState
{
    public StagMoveState(StagStateManager manager) : base(manager) { }

    /// <summary>
    /// Attempts to chose a random navigable destination.
    /// </summary>
    public override void EnterState() {
        manager.currentStateName = "Move";
        manager.anim.SetBool("isMoving", true);
        Vector3 randomPoint = manager.GetReachableNavMeshPoint(manager.transform.position, 2f, 5f);
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

    public override void UpdateState() {
        if (manager.FindNearestEnemyDist() <= 15f){
            manager.ChangeState(manager.MoveTowardsEnemyState);
            return;
        }
        else if (!manager.Agent.pathPending && manager.Agent.remainingDistance <= 1f){
            manager.ChangeState(manager.IdleState);
            return;
        }
    }

    public override void OnDamageTaken() {
        manager.ChangeState(manager.MoveTowardsEnemyState);
    }
}
