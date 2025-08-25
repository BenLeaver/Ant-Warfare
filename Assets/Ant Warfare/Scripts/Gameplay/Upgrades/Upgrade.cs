using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single upgrade in the game UI.
/// Handles display, cost initialization, and interaction state (selectable, unavailable, or selected).
/// </summary>
public class Upgrade : MonoBehaviour
{

    public int tier;
    public string upgradeName;
    public string description;

    public Sprite frontImage;
    public Sprite backImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    [HideInInspector]
    public int cost;

    private bool available = true;

    void Start()
    {
        nameText.text = upgradeName;
        descriptionText.text = description;
        gameObject.GetComponent<Image>().sprite = frontImage;
        InitialiseCost();
    }

    void InitialiseCost()
    {
        if (tier == 1)
        {
            cost = 50;
        }
        else if (tier == 2)
        {
            cost = 100;
        }
        else if (tier == 3)
        {
            cost = 200;
        }
    }

    public void DisplayBack()
    {
        gameObject.GetComponent<Image>().sprite = backImage;
        nameText.enabled = false; 
        descriptionText.enabled = true;
    }

    public void DisplayFront()
    {
        gameObject.GetComponent<Image>().sprite = frontImage;
        nameText.enabled = true;
        descriptionText.enabled = false;
    }

    /// <summary>
    /// Make Upgrade box appear darker and prevent interacting.
    /// </summary>
    public void Darken()
    {
        if (available)
        {
            gameObject.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            gameObject.GetComponent<Button>().interactable = false;
        }
    }

    /// <summary>
    /// Make Upgrade box appear lighter and allow interacting.
    /// </summary>
    public void Lighten()
    {
        if (available)
        {
            gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            gameObject.GetComponent<Button>().interactable = true;
        }
    }

    /// <summary>
    /// Permanently prevent interacting with this upgrade button, and make it appear darker, to 
    /// indicate it is no longer selectable.
    /// </summary>
    public void MakeUnavailable()
    {
        gameObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        gameObject.GetComponent<Button>().interactable = false;
        available = false;
    }

    /// <summary>
    /// Permanently prevent interacting with this upgrade button, and make it appear slightly 
    /// darker, to indicate that it has already been selected.
    /// </summary>
    public void Selected()
    {
        gameObject.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f, 0.8f);
        gameObject.GetComponent<Button>().interactable = false;
        available = false;

        if (GetComponent<SUpgradeManager>())
        {
            GetComponent<SUpgradeManager>().ApplyUpgrade(upgradeName);
        }
        if (GetComponent<MUpgradeManager>())
        {
            GetComponent<MUpgradeManager>().ApplyUpgrade(upgradeName);
        }
    }
}
