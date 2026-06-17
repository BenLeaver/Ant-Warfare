using UnityEngine;

/// <summary>
/// State where the beetle remains still for a random duration before walking.
/// </summary>
public class BeetleIdleState : BeetleBaseState
{
    private float idleTime;
    private float elapsed;

    public BeetleIdleState(BeetleStateManager beetle) : base(beetle) { }

    /// <summary>
    /// Initializes idle duration and stops movement.
    /// </summary>
    public override void EnterState()
    {
        manager.currentStateName = "Idle";
        idleTime = Random.Range(2f, 5f);
        elapsed = 0f;
        manager.Agent.ResetPath();
        manager.ChangeSpeed(0f);
    }


    /// <summary>
    /// Leaves idle once the timer has elapsed.
    /// </summary>
    public override void UpdateState()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= idleTime)
        {
            manager.ChangeState(manager.WalkState);
        }
    }

    /// <summary>
    /// Immediately switches to fast retreat when damaged.
    /// </summary>
    public override void OnDamageTaken()
    {
        manager.ChangeState(manager.FastRetreatState);
    }
}
