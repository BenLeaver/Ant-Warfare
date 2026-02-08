using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all states providing a common interface.
/// </summary>
public abstract class BaseState
{
    /// <summary>Called once when the state becomes active.</summary>
    public abstract void EnterState();

    /// <summary>Called every frame while this state is active.</summary>
    public abstract void UpdateState();

    /// <summary>Called when the beetle takes damage.</summary>
    public abstract void OnDamageTaken();
}
