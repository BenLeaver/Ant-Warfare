using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


/// <summary>
/// Handles the singleplayer UI, including buying ants, upgrades, placing marker commands,
/// and updating display elements like food, colony size, and upgrade tiers.
/// </summary>
public class Singleplayer_UI : MonoBehaviour
{
    /// <summary>
    /// Black - 0
    /// Fire - 1
    /// </summary>
    public int speciesIndex;

    public GameObject[] BuyMenus;

    public GameObject[] UpgradeMenus;

    public Button fireSuperSoldier;

    [System.Serializable]
    public class SpeciesUpgrades
    {
        public GameObject[] upgrades = new GameObject[6];
        public GameObject[] tiers = new GameObject[3];
    }

    public SpeciesUpgrades[] speciesUpgrades;

    public int playerTeam;
    public string playerSpecies;
    public GameObject playerQueen;
    public GameObject player;
    private BaseAntQueenAI queenScript;
    private PheromoneMarkerManager playerMarkerManager;
    private PlayerVision playerVision;

    public TMP_Text foodText;
    public TMP_Text colonySizeText;

    private int lastFood;
    private int lastColonySize;
    private int lastMaxColonySize;
    private int lastUpgradeTier;

    public GameObject helpUI;
    public GameObject upgradeUI;
    public GameObject commandUI;
    public GameObject spawnUI;
    public Canvas mainCanvas;

    public int upgradeTier = 0;
    public GameObject[] upgrades;
    public GameObject[] tiers;
    public GameObject lastLifeVolume;
    private bool upgradeUIActive = false;

    public bool inTutorial = false;

    public Color positiveFoodTextColor;
    public Color negativeFoodTextColor;

    public PlayerFollowUIButton pfButton;

    void Start()
    {
        queenScript = playerQueen.GetComponent<BaseAntQueenAI>();
        playerMarkerManager = player.GetComponent<PheromoneMarkerManager>();
        playerVision = player.GetComponent<PlayerVision>();

        if (GameObject.Find("LastLifeVolume"))
        {
            lastLifeVolume = GameObject.Find("LastLifeVolume");
        }
        else
        {
            Debug.LogError("LastLifeVolume could not be found.");
        }
        InitialiseCorrectSpeciesUI();
    }

    void InitialiseCorrectSpeciesUI()
    {
        for(int i=0; i<BuyMenus.Length; i++)
        {
            if (i == speciesIndex)
            {
                BuyMenus[i].SetActive(true);
                upgradeUI = UpgradeMenus[i];
                UpgradeMenus[i].SetActive(false);
                upgrades = speciesUpgrades[i].upgrades;
                tiers = speciesUpgrades[i].tiers;
            }
            else
            {
                BuyMenus[i].SetActive(false);
                UpgradeMenus[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerQueen != null)
        {
            if (queenScript.food != lastFood)
            {
                if (queenScript.food < 0)
                {
                    foodText.color = negativeFoodTextColor;
                    lastLifeVolume.GetComponent<Volume>().enabled = true;
                }
                else
                {
                    foodText.color = positiveFoodTextColor;
                    lastLifeVolume.GetComponent<Volume>().enabled = false;
                }
                foodText.text = queenScript.food.ToString();
            }
            if (queenScript.colonySize != lastColonySize || queenScript.maxColonySize != lastMaxColonySize)
            {
                colonySizeText.text = queenScript.colonySize.ToString()
                    + "/" + queenScript.maxColonySize.ToString();
            }
            if (upgradeUIActive)
            {
                // Only update upgrade UI if it is active.
                if (upgradeTier != lastUpgradeTier || queenScript.food != lastFood)
                {
                    UpdateUpgradeBoxes();
                }
                if (upgradeTier != lastUpgradeTier)
                {
                    UpdateUpgradeTiers();
                }
            }

            lastFood = queenScript.food;
            lastColonySize = queenScript.colonySize;
            lastMaxColonySize = queenScript.maxColonySize;
            lastUpgradeTier = upgradeTier;
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            //ToggleHelpUI();
        }
        if(Input.GetKeyDown(KeyCode.U))
        {
            ToggleUpgradeUI();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleMainUI();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleSpawnUI();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCommandUI();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            RecruitPlayerFollow();
            pfButton.Activate();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            DisbandPlayerFollow();
            pfButton.Deactivate();
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

        upgradeUIActive = !isActive;
        upgradeUI.SetActive(!isActive);

        if (upgradeUIActive)
        {
            // If the upgrade menu has just been opened, make sure to update the boxes and tiers.
            UpdateUpgradeBoxes();
            UpdateUpgradeTiers();
        }
    }

    public bool isUpgradeUIActive()
    {
        return upgradeUI.activeSelf;
    }

    public void ToggleMainUI()
    {
        bool isEnabled = mainCanvas.enabled;
        mainCanvas.enabled = !isEnabled;
    }

    public void ToggleCommandUI()
    {
        bool isActive = commandUI.activeSelf;
        commandUI.SetActive(!isActive);
    }

    public void ToggleSpawnUI()
    {
        bool isActive = spawnUI.activeSelf;
        spawnUI.SetActive(!isActive);
    }

    public void PlacePheremone(PheromoneSubtype subtype)
    {
        playerMarkerManager.PlaceMarker(subtype);
    }

    public void Place_FoodReturnPath()
    {
        PlacePheremone(PheromoneSubtype.FoodReturnPath);
    }

    public void Place_UnifiedFollowPath()
    {
        PlacePheremone(PheromoneSubtype.UnifiedFollowPath);
    }

    public void Place_SoldierAttackPath()
    {
        PlacePheremone(PheromoneSubtype.SoldierAttackPath);
    }

    public void Place_WorkerGatheringPath()
    {
        PlacePheremone(PheromoneSubtype.WorkerGatheringPath);
    }

    public void Place_GuardPoint()
    {
        PlacePheremone(PheromoneSubtype.GuardPoint);
    }

    public void RemovePheremone()
    {
        playerMarkerManager.RemoveMarker();
    }

    public void RecruitPlayerFollow()
    {
        playerMarkerManager.RecruitPlayerFollow();
    }

    public void DisbandPlayerFollow()
    {
        playerMarkerManager.DisbandPlayerFollow();

    }

    public void ShowFollowRange()
    {
        playerVision.ShowFollowCircle();
    }

    public void HideFollowRange()
    {
        playerVision.HideFollowCircle();
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
            int food = queenScript.food;
            GameObject upgrade = upgrades[upgradeIndex];
            int cost = upgrade.GetComponent<Upgrade>().cost;
            if (food >= cost)
            {
                upgradeTier += 1;
                queenScript.food -= cost;
                queenScript.AddSelectedUpgrade(upgrade);
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

    /// <summary>
    /// Updates image background for Upgrade Tier title boxes to indicate whether that upgrade tier is unlocked.
    /// </summary>
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

    /// <summary>
    /// Updates the upgrade boxes to darken unavailable upgrades (either due to cost or unlocked tier).
    /// </summary>
    void UpdateUpgradeBoxes()
    {
        foreach (GameObject u in upgrades)
        {
            if (u.GetComponent<Upgrade>().tier > upgradeTier)
            {
                
                if (u.GetComponent<Upgrade>().tier > upgradeTier + 1)
                {
                    // Higher tiers locked until lower tiers have been bought.`
                    u.GetComponent<Upgrade>().Darken();
                }
                else
                {
                    int food = queenScript.food;
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

    public void BuySoldier()
    {
        queenScript.SpawnSoldier();
    }

    public void BuyWorker()
    {
        queenScript.SpawnWorker();
        if (inTutorial)
        {
            if (GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart == 9)
            {
                GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart = 10;
            }
        }
    }

    public void BuySuperSoldier()
    {
        queenScript.SpawnSuperSoldier();
    }
}
