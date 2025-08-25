using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles showing or hiding the description of a command when UI command 
/// buttons are interacted with.
/// Attach this script to a command button GameObject.
/// </summary>
public class CommandButton : MonoBehaviour
{
    public GameObject commandDescription;

    public void DisplayDescription()
    {
        commandDescription.SetActive(true);
    }

    public void HideDescription()
    {
        commandDescription.SetActive(false);
    }
}
