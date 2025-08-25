using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;

/// <summary>
/// Custom NetworkAnimator that makes animation updates **owner-authoritative** rather than server-authoritative.
/// This allows the player who owns the object to control animations directly without waiting for server approval.
/// </summary>
public class OwnerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
