using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Abstract base class for all ant AI states.
/// 
/// Responsibilities:
/// - Stores references to the AntContext and AntStateManager.
/// - Tracks duration spent in the state and enforces a maximum duration.
/// - Provides virtual entry and update methods for derived states to extend.
/// - Handles subtype assignment (used by some states to restrict interrupts).
/// 
/// Derived states are expected to:
/// - Override EnterState() to initialize movement targets or timers.
/// - Override UpdateState() to implement behaviour logic.
/// - Call base.UpdateState() to ensure duration tracking and timeout behaviour.
/// </summary>
public abstract class BaseAntState
{
    protected AntContext context;
    protected AntStateManager manager;

    public AntStateType StateType { get; private set; }
    public int SubType { get; protected set; }

    protected float maxDuration = 30f;
    protected float currentDuration;
    
    public BaseAntState(AntStateManager manager, AntContext context, AntStateType stateType)
    {
        this.context = context;
        this.manager = manager;
        this.StateType = stateType;
    }

    public virtual void EnterState(Vector3 target = default, int subtype = 0, bool isResuming = false)
    {
        if (!isResuming) this.SubType = subtype;
    }

    public virtual void UpdateState(float deltaTime)
    {
        currentDuration += deltaTime;
        if (currentDuration >= maxDuration)
        {
            manager.DecideNextState();
            return;
        }
    }
}
