using UnityEngine;

/// <summary>
/// Base class for all beetle states providing a common interface.
/// </summary>
public abstract class BeetleBaseState : BaseState
{
    protected BeetleStateManager manager;

    public BeetleBaseState(BeetleStateManager beetle) => this.manager = beetle;
}
