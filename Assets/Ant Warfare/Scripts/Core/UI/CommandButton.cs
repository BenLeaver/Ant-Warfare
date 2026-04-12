using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles showing or hiding the description of a command when UI command 
/// buttons are interacted with.
/// Attach this script to a command button GameObject.
/// </summary>
public class CommandButton : MonoBehaviour
{
    public GameObject commandDescription;
    public TMP_Text amountText;
    public Singleplayer_UI uiScript;
    public PheromoneSubtype subtype;

    public bool isRemoveButton = false;

    public void DisplayDescription()
    {
        commandDescription.SetActive(true);
    }

    public void HideDescription()
    {
        commandDescription.SetActive(false);
    }

    void Start()
    {
        if (!isRemoveButton)
        {
            var queen = uiScript.playerQueen;
            queen.GetComponent<ColonyPheromonesManager>().InitializePlayerAmountText(subtype, amountText);
        }
    }
}
