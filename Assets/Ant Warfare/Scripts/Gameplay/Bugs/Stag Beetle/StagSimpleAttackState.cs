using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagSimpleAttackState : StagBaseState
{
    public StagSimpleAttackState(StagStateManager manager) : base(manager) { }
    public float elapsed;
    public const float totalAttackTime = (25f/60f);
    public const float attackDamageTime = (15f / 60f);

    private GameObject closestEnemy;
    private bool attacked;

    public override void EnterState() {
        manager.currentStateName = "SimpleAttack";
        manager.anim.SetBool("isMoving", false);
        manager.anim.SetInteger("attackDecision", 1);
        elapsed = 0f;
        attacked = false;
    }

    public override void UpdateState() {
        closestEnemy = manager.FindNearestEnemy();
        elapsed += Time.deltaTime;

        if (closestEnemy == null) return;

        float closestEnemyDist = Vector3.Distance(manager.transform.position, closestEnemy.transform.position);

        if (elapsed > totalAttackTime || closestEnemyDist > 5f)
        {
            EndAttack();
        }
        else if (elapsed > attackDamageTime && !attacked)
        {
            DamageClosest();
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
    /// Deals attack damage to the closest enemy.
    /// </summary>
    private void DamageClosest()
    {
        closestEnemy.GetComponent<SHealth>().UpdateHealth(manager.simpleAttackDamage);
    }

    public override void OnDamageTaken() { }
}
