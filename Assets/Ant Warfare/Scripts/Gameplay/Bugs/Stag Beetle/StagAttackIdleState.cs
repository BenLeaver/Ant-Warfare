using UnityEngine;

/// <summary>
/// 
/// Will transition to either:
/// - SimpleAttack
/// - PickUp (?)
/// - Rampage (?)
/// - MoveTowardsEnemy
/// - Idle
/// </summary>
public class StagAttackIdleState : StagBaseState
{
    private float decisionTime;
    private float elapsed;
    public StagAttackIdleState(StagStateManager manager) : base(manager) { }
    private UnitInfo closestEnemy;

    public override void EnterState() {
        manager.currentStateName = "AttackIdle";
        manager.anim.SetBool("isMoving", false);
        manager.anim.SetInteger("attackDecision", 0);
        decisionTime = 1f + Random.Range(-0.1f, 0.1f);
        elapsed = 0f;
        manager.Agent.ResetPath();
        manager.SetRotationMode(BugStateManager.RotationMode.ClosestEnemy);
    }

    public override void UpdateState() {
        if (elapsed < decisionTime)
        {
            elapsed += Time.deltaTime;
        }
        else
        {
            elapsed = 0f;
            closestEnemy = manager.FindNearestEnemy();

            if (!closestEnemy.IsAliveAndActive())
            {
                manager.ChangeState(manager.IdleState);
                return;
            }
                
                

            float closestEnemyDist = Vector3.Distance(manager.transform.position, closestEnemy.transform.position);

            if (closestEnemyDist > 15f)
            {
                manager.SetRotationMode(BugStateManager.RotationMode.MovementDirection);
                manager.ChangeState(manager.IdleState);
            }
            else if (closestEnemyDist > 4f)
            {
                manager.SetRotationMode(BugStateManager.RotationMode.MovementDirection);
                manager.ChangeState(manager.MoveTowardsEnemyState);
            }
            else
            {
                AttackDecision();
            }
        }
    }

    private void AttackDecision()
    {
        int decisionIndex = Random.Range(1, 7);
        if (decisionIndex <= 4 || closestEnemy.queenScript != null)
        {
            // If closest enemy is a queen, don't throw it.
            manager.ChangeState(manager.SimpleAttackState);
        }
        else
        {
            manager.ChangeState(manager.ThrowState);
        }
    }

    public override void OnDamageTaken() { }
}
