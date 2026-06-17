using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles interactivity for the player follow recruit and disband buttons.
/// </summary>
public class PlayerFollowUIButton : MonoBehaviour
{
    public Button recruitButton;
    public Button disbandButton;

    public Sprite inactive;
    public Sprite active;
    private Image buttonImage;

    // Start is called before the first frame update
    void Start()
    {
        buttonImage = recruitButton.GetComponent<Image>();
        recruitButton.onClick.AddListener(Activate);
        disbandButton.onClick.AddListener(Deactivate);
    }

    void Activate()
    {
        buttonImage.sprite = active;
    }

    void Deactivate()
    {
        buttonImage.sprite = inactive;
    }
}
