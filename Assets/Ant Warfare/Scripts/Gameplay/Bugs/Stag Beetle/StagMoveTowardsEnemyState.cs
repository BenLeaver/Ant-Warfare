using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagMoveTowardsEnemyState : StagBaseState
{
    public StagMoveTowardsEnemyState(StagStateManager manager) : base(manager) { }

    public override void EnterState() {
        manager.currentStateName = "MoveTowardsEnemy";
        manager.anim.SetBool("isMoving", true);
    }

    public override void UpdateState() {
        UnitInfo closestEnemy = manager.FindNearestEnemy();

        if (closestEnemy == null)
        {
            Debug.Log("closest enemy null");
            manager.ChangeState(manager.IdleState);
            return;
        }
        if (closestEnemy.transform.position == null)
        {
            Debug.Log("Transform pos null");
            manager.ChangeState(manager.IdleState);
            return;
        }

        manager.Agent.SetDestination(closestEnemy.transform.position);

        float closestEnemyDist = Vector3.Distance(manager.transform.position, closestEnemy.transform.position);
        if (closestEnemyDist < 4f){
            manager.ChangeState(manager.AttackIdleState);
            return;
        }
        else if (closestEnemyDist > 15f)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }
    }

    public override void OnDamageTaken() { }
}
