using UnityEngine.Rendering.PostProcessing;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the in-game UI for multiplayer players, including food display, colony upgrades,
/// unit purchases, and pheromone marker placement UI.
/// </summary>
public class Multiplayer_UI : MonoBehaviour
{
    public Button buySoldier;
    public Button buyWorker;
    public Button buySuperSoldier;

    public GameObject playerQueen;
    public TMP_Text foodText;
    public TMP_Text colonySizeText;
    public GameObject helpUI;

    public GameObject upgradeUI;
    public int upgradeTier = 0;
    public GameObject[] upgrades;
    public GameObject[] tiers;

    public int playerTeam;
    private string playerSpecies;
    public GameObject lastLifePostProcessing;

    [Header("Commands")]
    private GameObject player;
    public GameObject[] markerAmountTexts;
    

    // Start is called before the first frame update
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
            Debug.LogWarning("LastLifePostProcessing could not be found.");
        }
    }

    public void UpdatePlayerMarkerTexts(GameObject playerObject)
    {
        player = playerObject;
        player.GetComponent<MPheremoneMarkerManager>().markerAmountTexts = markerAmountTexts;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerQueen != null)
        {
            int food = playerQueen.GetComponent<MBaseAntQueenAI>().food;
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
            colonySizeText.text = playerQueen.GetComponent<MBaseAntQueenAI>().colonySize.ToString() + "/" 
                + playerQueen.GetComponent<MBaseAntQueenAI>().maxColonySize.ToString();
            UpdateUpgradeBoxes();
            UpdateUpgradeTiers();
        }
        else
        {
            if(GameObject.Find("M" + playerSpecies + "AntQueen" + playerTeam.ToString()))
            {
                playerQueen = GameObject.Find("M" + playerSpecies + "AntQueen" + playerTeam.ToString());
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHelpUI();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleUpgradeUI();
        }
    }

    public void SetQueen(int team, string species)
    {
        playerTeam = team;
        playerSpecies = species;
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

    public void PlacePheremone(int index)
    {
        player.GetComponent<MPheremoneMarkerManager>().PlaceMarkerServerRpc(index, playerTeam);
    }

    public void RemovePheremone()
    {
        player.GetComponent<MPheremoneMarkerManager>().RemoveMarkerServerRpc(playerTeam);
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
            int food = playerQueen.GetComponent<MBaseAntQueenAI>().food;
            GameObject upgrade = upgrades[upgradeIndex];
            int cost = upgrade.GetComponent<Upgrade>().cost;
            if (food >= cost)
            {
                upgradeTier += 1;
                playerQueen.GetComponent<MBaseAntQueenAI>().food -= cost;
                playerQueen.GetComponent<MBaseAntQueenAI>().AddSelectedUpgradeRpc(upgrade.GetComponent<Upgrade>().upgradeName);
                upgrade.GetComponent<Upgrade>().Selected();
                foreach (GameObject u in upgrades)
                {
                    if (u.GetComponent<Upgrade>().tier == upgradeTier && u != upgrade)
                    {
                        u.GetComponent<Upgrade>().MakeUnavailable();
                    }
                }
                UpdateUpgradeBoxes();

                if (upgrade.GetComponent<Upgrade>().upgradeName == "Supersoldier")
                {
                    buySuperSoldier.gameObject.SetActive(true);
                }
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

    /// <summary>
    /// Updates the upgrade boxes based on available food and tier restrictions.
    /// </summary>
    void UpdateUpgradeBoxes()
    {
        foreach (GameObject u in upgrades)
        {
            if (u.GetComponent<Upgrade>().tier > upgradeTier)
            {

                if (u.GetComponent<Upgrade>().tier > upgradeTier + 1)
                {
                    u.GetComponent<Upgrade>().Darken();
                }
                else
                {
                    int food = playerQueen.GetComponent<MBaseAntQueenAI>().food;
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
        if(playerSpecies == "Black")
        {
            GameObject.Find("ProjectSceneManager").GetComponent<AntSpawner>().SpawnAntClientRpc(0, 1, playerTeam);
        }
        else if (playerSpecies == "Fire")
        {
            GameObject.Find("ProjectSceneManager").GetComponent<AntSpawner>().SpawnAntClientRpc(1, 1, playerTeam);
        }

    }

    void BuyWorker()
    {
        if (playerSpecies == "Black")
        {
            GameObject.Find("ProjectSceneManager").GetComponent<AntSpawner>().SpawnAntClientRpc(0, 0, playerTeam);
        }
        if (playerSpecies == "Fire")
        {
            GameObject.Find("ProjectSceneManager").GetComponent<AntSpawner>().SpawnAntClientRpc(1, 0, playerTeam);
        }
    }

    void BuySuperSoldier()
    {
        GameObject.Find("ProjectSceneManager").GetComponent<AntSpawner>().SpawnAntClientRpc(1, 2, playerTeam);
    }
}
