using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State where the beetle rapidly retreats from threats.
/// </summary>
public class BeetleFastRetreatState : BeetleBaseState
{
    private float retreatDuration;
    private float elapsed;

    public BeetleFastRetreatState(BeetleStateManager beetle) : base(beetle) { }

    /// <summary>
    /// Sets a high movement speed and picks a retreat destination.
    /// </summary>
    public override void EnterState()
    {
        manager.currentStateName = "Fast Retreat";
        manager.ChangeSpeed(manager.fastRetreatSpeed);
        retreatDuration = 5f;
        elapsed = 0f;
        SetRetreatDestination();
    }

    /// <summary>
    /// Continues retreating, switching to tired retreat once the duration ends.
    /// Switches to idle if the beetle is over 30 units from the nearest enemy.
    /// </summary>
    public override void UpdateState()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= retreatDuration)
        {
            manager.ChangeState(manager.TiredRetreatState);
            return;
        }

        GameObject nearestEnemy = manager.FindNearestEnemy();
        if (nearestEnemy == null)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }

        float distance = Vector3.Distance(manager.transform.position, nearestEnemy.transform.position);
        if (distance > 30f)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }

        if (!manager.Agent.pathPending && manager.Agent.remainingDistance <= 0.5f)
        {
            SetRetreatDestination();
        }
    }

    /// <summary>
    /// Selects a new point away from the nearest enemy.
    /// </summary>
    private void SetRetreatDestination()
    {
        Vector3 destination = manager.GetRetreatPoint();
        manager.Agent.SetDestination(destination);
    }

    public override void OnDamageTaken() { }
}
