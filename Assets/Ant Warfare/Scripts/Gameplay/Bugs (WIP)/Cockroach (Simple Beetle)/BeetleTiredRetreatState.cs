using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A slower retreat state entered after fast retreat.
/// </summary>
public class BeetleTiredRetreatState : BeetleBaseState
{
    private float elapsed;

    public BeetleTiredRetreatState(BeetleStateManager beetle) : base(beetle) { }

    /// <summary>
    /// Begins the tired retreat and selects an initial destination.
    /// </summary>
    public override void EnterState()
    {
        manager.currentStateName = "Tired Retreat";
        manager.ChangeSpeed(manager.fastRetreatSpeed);
        elapsed = 0f;
        SetRetreatDestination();
    }

    /// <summary>
    /// Gradually slows the beetle and retreats until the enemy is far enough.
    /// </summary>
    public override void UpdateState()
    {
        elapsed += Time.deltaTime;

        GameObject nearestEnemy = manager.FindNearestEnemy();
        if (nearestEnemy == null)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }

        float distance = Vector3.Distance(manager.transform.position, nearestEnemy.transform.position);
        if (distance > 20f)
        {
            manager.ChangeState(manager.IdleState);
            return;
        }

        float t = Mathf.Clamp01(elapsed / 20f);
        float speedFactor = Mathf.Lerp(1f, 0.4f, t);
        manager.ChangeSpeed(manager.fastRetreatSpeed * speedFactor);

        if (!manager.Agent.pathPending && manager.Agent.remainingDistance <= 0.5f)
        {
            SetRetreatDestination();
        }
    }

    /// <summary>
    /// Selects a new retreat target as needed.
    /// </summary>
    private void SetRetreatDestination()
    {
        Vector3 destination = manager.GetRetreatPoint();
        manager.Agent.SetDestination(destination);
    }

    public override void OnDamageTaken() { }
}
