using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all stag beetle states providing a common interface.
/// </summary>
public abstract class StagBaseState : BaseState
{
    protected StagStateManager manager;

    public StagBaseState(StagStateManager stag) => this.manager = stag;
}
