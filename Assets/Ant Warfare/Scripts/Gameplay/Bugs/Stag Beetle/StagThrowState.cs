using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagThrowState : StagBaseState
{
    public StagThrowState(StagStateManager manager) : base(manager) { }

    public float elapsed;
    public const float totalAttackTime = (45f / 60f);
    public const float throwTime = (30f / 60f);

    private GameObject closestEnemy;
    private bool attacked;

    public override void EnterState() {
        manager.currentStateName = "SimpleAttack";
        manager.anim.SetBool("isMoving", false);
        manager.anim.SetInteger("attackDecision", 2);
        elapsed = 0f;
        attacked = false;
    }

    public override void UpdateState() {
        closestEnemy = manager.FindNearestNonQueenEnemy();
        elapsed += Time.deltaTime;

        if (closestEnemy == null)
        {
            EndAttack();
            return;
        }

        float closestEnemyDist = Vector3.Distance(manager.transform.position, closestEnemy.transform.position);

        if (elapsed > totalAttackTime || closestEnemyDist > 5f)
        {
            EndAttack();
        }
        else if (elapsed > throwTime && !attacked)
        {
            ThrowClosest();
            attacked = true;
        }
    }

    /// <summary>
    /// Will reset the stag beetle to the attack idle state.
    /// </summary>
    private void EndAttack()
    {
        manager.anim.SetInteger("attackDecision", 0);
        manager.ChangeState(manager.AttackIdleState);
    }

    /// <summary>
    /// Throws the closest enemy.
    /// </summary>
    private void ThrowClosest()
    {
        if (closestEnemy == null) return;

        Vector3 targetPoint = manager.GetEnemyThrowTarget(closestEnemy);

        if (!manager.HasValidPath(targetPoint))
        {
            Debug.LogWarning("No valid path to the throw target");
            return;
        }

        ThrownAnt thrown = closestEnemy.AddComponent<ThrownAnt>();

        thrown.Initialise(targetPoint, 0.6f, manager.throwDamage, manager.throwImpactEffect);
    }

    public override void OnDamageTaken() { }
}
