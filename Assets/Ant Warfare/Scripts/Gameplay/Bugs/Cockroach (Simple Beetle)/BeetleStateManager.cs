using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Acts as context for the cockroach beetle's finite state machine.
/// Holds state instances, runs updates, and provides shared utilities.
/// </summary>
public class BeetleStateManager : BugStateManager
{
    [Header("NavMesh & Movement")]
    public float walkSpeed = 3f;
    public float fastRetreatSpeed = 8f;

    //[Header("States")]
    public BeetleIdleState IdleState { get; private set; }
    public BeetleWalkState WalkState { get; private set; }
    public BeetleFastRetreatState FastRetreatState { get; private set; }
    public BeetleTiredRetreatState TiredRetreatState { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        IdleState = new BeetleIdleState(this);
        WalkState = new BeetleWalkState(this);
        FastRetreatState = new BeetleFastRetreatState(this);
        TiredRetreatState = new BeetleTiredRetreatState(this);

        currentState = IdleState;
        currentState.EnterState();
    }

    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Updates NavMeshAgent speed and animation parameters.
    /// </summary>
    public void ChangeSpeed(float newSpeed)
    {
        Agent.speed = newSpeed;
        anim.SetFloat("speed", newSpeed);
        anim.SetBool("isMoving", newSpeed > 0);
    }
}
