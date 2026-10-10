using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagSimpleAttackState : StagBaseState
{
    public StagSimpleAttackState(StagStateManager manager) : base(manager) { }
    public float elapsed;
    public const float totalAttackTime = (25f/60f);
    public const float attackDamageTime = (15f / 60f);

    private UnitInfo closestEnemy;
    private GameObject stag;
    private bool attacked;

    public override void EnterState() {
        manager.currentStateName = "SimpleAttack";
        manager.anim.SetBool("isMoving", false);
        manager.anim.SetInteger("attackDecision", 1);
        elapsed = 0f;
        attacked = false;
        stag = manager.gameObject;
    }

    public override void UpdateState() {
        closestEnemy = manager.FindNearestEnemy();
        elapsed += Time.deltaTime;

        if (closestEnemy == null)
        {
            EndAttack();
            return;
        }
        if (closestEnemy.go == null)
        {
            EndAttack();
            return;
        }

        float closestEnemyDist = Vector3.Distance(manager.transform.position, closestEnemy.transform.position);

        if (elapsed > totalAttackTime || closestEnemyDist > 5f)
        {
            EndAttack();
            return;
        }
        else if (elapsed > attackDamageTime && !attacked)
        {
            DamageClosest();
            attacked = true;
            EndAttack();
            return;
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
        if (stag == null || !stag.activeInHierarchy) return;

        if (!closestEnemy.IsAliveAndActive()) return;
        
        closestEnemy.health.UpdateHealth(manager.simpleAttackDamage);
    }

    public override void OnDamageTaken() { }
}
