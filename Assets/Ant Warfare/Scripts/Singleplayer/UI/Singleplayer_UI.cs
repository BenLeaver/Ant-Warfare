using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Handles the singleplayer UI, including buying ants, upgrades, placing marker commands,
/// and updating display elements like food, colony size, and upgrade tiers.
/// </summary>
public class Singleplayer_UI : MonoBehaviour
{
    public Button buySoldier;
    public Button buyWorker;
    public Button buySuperSoldier;

    public int playerTeam;
    public string playerSpecies;
    public GameObject playerQueen;
    public GameObject player;
    public TMP_Text foodText;
    public TMP_Text colonySizeText;
    public GameObject helpUI;
    public GameObject upgradeUI;
    public int upgradeTier = 0;
    public GameObject[] upgrades;
    public GameObject[] tiers;
    public GameObject lastLifePostProcessing;

    public bool inTutorial = false;

    [Header("Commands")]
    public GameObject[] markerAmountTexts;

    void Start()
    {
        buySoldier.onClick.AddListener(BuySoldier);
        buyWorker.onClick.AddListener(BuyWorker);
        if (buySuperSoldier != null)
        {
            buySuperSoldier.onClick.AddListener(BuySuperSoldier);
        }
        if (GameObject.Find("LastLifePostProcessing"))
        {
            lastLifePostProcessing = GameObject.Find("LastLifePostProcessing");
        }
        else
        {
            Debug.LogError("LastLifePostProcessing could not be found.");
        }
        StartCoroutine(UpdatePlayerMarkerTexts());

    }

    /// <summary>
    /// Waits 1 frame to make sure player has been instantiated before updating the 
    /// markerAmountTexts, so that the player has a reference to the marker amount UI texts.
    /// </summary>
    IEnumerator UpdatePlayerMarkerTexts()
    {
        yield return null;
        player = GameObject.Find("SGameManager").GetComponent<SGameManager>().player;
        player.GetComponent<PheremoneMarkerManager>().markerAmountTexts = markerAmountTexts;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerQueen == null)
        {
            playerTeam = GameObject.Find("SGameManager").GetComponent<SGameManager>().playerTeams[0];
            playerSpecies = GameObject.Find("SGameManager").GetComponent<SGameManager>().playerSpecies[0];
            if (GameObject.Find(playerSpecies + "AntQueen" + playerTeam))
            {
                playerQueen = GameObject.Find(playerSpecies + "AntQueen" + playerTeam);
            }
            
        }
        else if (playerQueen != null)
        {
            int food = playerQueen.GetComponent<BaseAntQueenAI>().food;
            if (food < 0)
            {
                foodText.color = new Color(0.566f, 0.0275f, 0.0275f, 1f);
                lastLifePostProcessing.GetComponent<PostProcessVolume>().enabled = true;
            }
            else
            {
                foodText.color = new Color(0.0275f, 0.0275f, 0.0275f, 1f);
                lastLifePostProcessing.GetComponent<PostProcessVolume>().enabled = false;
            }
            foodText.text = food.ToString();
            colonySizeText.text = playerQueen.GetComponent<BaseAntQueenAI>().colonySize.ToString() 
                + "/" + playerQueen.GetComponent<BaseAntQueenAI>().maxColonySize.ToString();
            UpdateUpgradeBoxes();
            UpdateUpgradeTiers();
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            ToggleHelpUI();
        }
        if(Input.GetKeyDown(KeyCode.U))
        {
            ToggleUpgradeUI();
        }
    }

    public void ToggleHelpUI()
    {
        bool isActive = helpUI.activeSelf;
        helpUI.SetActive(!isActive);
    }

    public void ToggleUpgradeUI()
    {
        bool isActive = upgradeUI.activeSelf;
        upgradeUI.SetActive(!isActive);
    }

    public bool isUpgradeUIActive()
    {
        return upgradeUI.activeSelf;
    }

    public void PlacePheremone(int index)
    {
        player.GetComponent<PheremoneMarkerManager>().PlaceMarker(index);
    }

    public void RemovePheremone()
    {
        player.GetComponent<PheremoneMarkerManager>().RemoveMarker();
    }

    /// <summary>
    /// Called to handle when player tries to buy an upgrade.
    /// The given index determines which upgrade it is. Tier 1 upgrades will be 0-1, 
    /// Tier 2: 2-3, Tier 3: 4-5.
    /// </summary>
    /// <param name="upgradeIndex">The index of the upgrade.</param>
    public void BuyUpgrade(int upgradeIndex)
    {
        if (playerQueen != null)
        {
            int food = playerQueen.GetComponent<BaseAntQueenAI>().food;
            GameObject upgrade = upgrades[upgradeIndex];
            int cost = upgrade.GetComponent<Upgrade>().cost;
            if (food >= cost)
            {
                upgradeTier += 1;
                playerQueen.GetComponent<BaseAntQueenAI>().food -= cost;
                playerQueen.GetComponent<BaseAntQueenAI>().AddSelectedUpgrade(upgrade);
                upgrade.GetComponent<Upgrade>().Selected();
                foreach (GameObject u in upgrades)
                {
                    if(u.GetComponent<Upgrade>().tier == upgradeTier && u != upgrade)
                    {
                        u.GetComponent<Upgrade>().MakeUnavailable();
                    }
                }
                UpdateUpgradeBoxes();
            }
        }
    }

    void UpdateUpgradeTiers()
    {
        if (upgradeTier == 0)
        {
            tiers[0].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            tiers[1].GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
            tiers[2].GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
        }
        else if (upgradeTier == 1)
        {
            tiers[0].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            tiers[1].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            tiers[2].GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
        }
        else if (upgradeTier == 2)
        {
            tiers[0].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            tiers[1].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
            tiers[2].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
        }
    }

    void UpdateUpgradeBoxes()
    {
        foreach (GameObject u in upgrades)
        {
            if (u.GetComponent<Upgrade>().tier > upgradeTier)
            {
                
                if (u.GetComponent<Upgrade>().tier > upgradeTier + 1)
                {
                    // Higher tiers locked until lower tiers have been bought.
                    u.GetComponent<Upgrade>().Darken();
                }
                else
                {
                    int food = playerQueen.GetComponent<BaseAntQueenAI>().food;
                    if (u.GetComponent<Upgrade>().cost > food)
                    {
                        u.GetComponent<Upgrade>().Darken();
                    }
                    else
                    {
                        u.GetComponent<Upgrade>().Lighten();
                    }
                }
            }
        }
    }

    void BuySoldier()
    {
        playerQueen.GetComponent<BaseAntQueenAI>().SpawnSoldier();
    }

    void BuyWorker()
    {
        playerQueen.GetComponent<BaseAntQueenAI>().SpawnWorker();
        if (inTutorial)
        {
            if (GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart == 9)
            {
                GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart = 10;
            }
        }
    }

    void BuySuperSoldier()
    {
        playerQueen.GetComponent<BaseAntQueenAI>().SpawnSuperSoldier();
    }
}
