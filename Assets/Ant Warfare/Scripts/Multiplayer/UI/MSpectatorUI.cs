using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI for spectators, such as hiding or showing spectator-specific text.
/// </summary>
public class MSpectatorUI : MonoBehaviour
{
    public TextMeshProUGUI spectatorText;

    public void DestroySpectatorText()
    {
        spectatorText.enabled = false;
    }
}
