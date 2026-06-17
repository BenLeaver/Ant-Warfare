using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles activating or deactivating the player follow range circle.
/// </summary>
public class PlayerVision : MonoBehaviour
{
    public GameObject followCircle;

    public void ShowFollowCircle()
    {
        followCircle.SetActive(true);
    }

    public void HideFollowCircle()
    {
        followCircle.SetActive(false);
    }
}
