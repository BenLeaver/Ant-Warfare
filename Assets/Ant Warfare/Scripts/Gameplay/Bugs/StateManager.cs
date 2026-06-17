using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class acting as context for a finite state machine.
/// Holds the current state, updates it every frame and allows for changing state.
/// 
/// Extend from this class for more specific state managers.
/// </summary>
public class StateManager : MonoBehaviour
{
    public BaseState currentState;
    public string currentStateName;

    public virtual void Update()
    {
        currentState.UpdateState();
    }

    public void ChangeState(BaseState newState)
    {
        currentState = newState;
        currentState.EnterState();
    }
}
