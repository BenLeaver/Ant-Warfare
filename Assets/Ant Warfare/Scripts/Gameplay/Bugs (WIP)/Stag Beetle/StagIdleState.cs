using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State where the beetle remains still for a random duration before moving.
/// </summary>
public class StagIdleState : StagBaseState
{
    private float idleTime;
    private float elapsed;

    public StagIdleState(StagStateManager manager) : base(manager) { }

    /// <summary>
    /// Initializes idle duration and stops movement.
    /// </summary>
    public override void EnterState() {
        manager.currentStateName = "Idle";
        idleTime = Random.Range(5f, 10f);
        elapsed = 0f;
        manager.Agent.ResetPath();
        manager.anim.SetBool("isMoving", false);
    }

    /// <summary>
    /// Leaves idle once the timer has elapsed.
    /// </summary>
    public override void UpdateState() {
        elapsed += Time.deltaTime;
        if (manager.FindNearestEnemyDist() <= 15f){
            manager.ChangeState(manager.MoveTowardsEnemyState);
        } else if (elapsed >= idleTime){
            manager.ChangeState(manager.MoveState);
        }
    }

    /// <summary>
    /// Immediately switches to MoveTowardsEnemy when damaged.
    /// </summary>
    public override void OnDamageTaken() {
        manager.ChangeState(manager.MoveTowardsEnemyState);
    }
}
