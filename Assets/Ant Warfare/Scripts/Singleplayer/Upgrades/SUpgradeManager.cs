using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;

/// <summary>
/// Manages the application of upgrades to ants and the player's colony in singleplayer mode.
/// Handles modifying stats, enabling abilities, and unlocking special units.
/// </summary>
public class SUpgradeManager : MonoBehaviour
{
    public GameObject singleplayerUI;
    private GameObject[] Ants;
    private int thisTeam;

    /// <summary>
    /// Applies the effect of an upgrade.
    /// </summary>
    /// <param name="name">The name of the upgrade to apply.</param>
    public void ApplyUpgrade(string name)
    {
        Ants = GameObject.FindGameObjectsWithTag("Ant");
        thisTeam = singleplayerUI.GetComponent<Singleplayer_UI>().playerTeam;
        // Black Upgrades
        if (name == "Movement Speed")
        {
            MovementSpeed(1.15f);
        }
        else if (name == "Stronger Soldiers")
        {
            StrongerSoldiers(1.20f);
        }
        else if (name == "Less Food Waste")
        {
            LessFoodWaste(1.30f);
        }
        else if (name == "Colony Capacity")
        {
            ColonyCapacity(10);
        }
        else if (name == "Fortress")
        {
            Fortress();
        }
        else if (name == "First Aid")
        {
            FirstAid();
        }

        // Fire Upgrades
        if (name == "Rapid Movement")
        {
            MovementSpeed(1.2f);
        }
        if (name == "Long Stingers")
        {
            LongStingers(1.2f);
        }
        if (name == "Aphid Farming")
        {
            AphidFarming();
        }
        if (name == "Aggressive Workers")
        {
            AggressiveWorkers(1.1f, 1.1f);
        }
        if (name == "Supersoldier")
        {
            Supersoldier();
        }
        if (name == "Last Stand")
        {
            LastStand();
        }
    }

    void MovementSpeed(float speedMult)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team == thisTeam)
            {
                if (a.GetComponent<Player_Singleplayer>())
                {
                    a.GetComponent<Player_Singleplayer>().moveSpeed *= speedMult;
                }
                if (a.GetComponent<NavMeshAgent>())
                {
                    a.GetComponent<NavMeshAgent>().speed *= speedMult;
                }
            }
        }
    }

    void StrongerSoldiers(float damageMult)
    {

        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team == thisTeam)
            {
                if (a.GetComponent<AntBaseAI>())
                {
                    if (a.GetComponent<AntBaseAI>().type == "Soldier")
                    {
                        int current = a.GetComponent<AntBaseAI>().attack;
                        a.GetComponent<AntBaseAI>().attack = Mathf.RoundToInt(current * damageMult);
                    }
                }
            }
        }
    }

    void LessFoodWaste(float foodMult)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team == thisTeam)
            {
                if (a.GetComponent<Player_Singleplayer>())
                {
                    a.GetComponent<Player_Singleplayer>().foodMult = foodMult;
                }
                if (a.GetComponent<AntBaseAI>())
                {
                    a.GetComponent<AntBaseAI>().foodMult = foodMult;
                }
            }
        }
    }

    void ColonyCapacity(int capacityIncrease)
    {
        GameObject queen = singleplayerUI.GetComponent<Singleplayer_UI>().playerQueen;
        if (queen != null)
        {
            queen.GetComponent<BaseAntQueenAI>().maxColonySize += capacityIncrease;
        }
    }

    void Fortress()
    {
        GameObject queen = singleplayerUI.GetComponent<Singleplayer_UI>().playerQueen;
        if (queen != null)
        {
            queen.GetComponent<BaseAntQueenAI>().fortress = true;
        }
    }

    void FirstAid()
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team == thisTeam)
            {
                if (a.GetComponent<AntBaseAI>())
                {
                    if (a.GetComponent<AntBaseAI>().type == "Soldier")
                    {
                        a.GetComponent<AntBaseAI>().firstAid = true;
                    }
                }
            }
        }
    }

    void LongStingers(float damageMult)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team == thisTeam)
            {
                if (a.GetComponent<AntBaseAI>())
                {
                    int current = a.GetComponent<AntBaseAI>().attack;
                    a.GetComponent<AntBaseAI>().attack = Mathf.RoundToInt(current * damageMult);
                }
                if (a.GetComponent<Player_Singleplayer>())
                {
                    int current = a.GetComponent<Player_Singleplayer>().attack;
                    a.GetComponent<Player_Singleplayer>().attack = Mathf.RoundToInt(current * damageMult);
                }
            }
        }
    }

    void AphidFarming()
    {
        GameObject queen = singleplayerUI.GetComponent<Singleplayer_UI>().playerQueen;
        if (queen != null)
        {
            queen.GetComponent<BaseAntQueenAI>().aphidFarming = true;
        }
    }

    void AggressiveWorkers(float speedMult, float damageMult)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team == thisTeam)
            {
                if (a.GetComponent<AntBaseAI>())
                {
                    if (a.GetComponent<AntBaseAI>().type == "Worker")
                    {
                        int current = a.GetComponent<AntBaseAI>().attack;
                        a.GetComponent<AntBaseAI>().attack = Mathf.RoundToInt(current * damageMult);
                        if (a.GetComponent<NavMeshAgent>())
                        {
                            a.GetComponent<NavMeshAgent>().speed *= speedMult;
                        }
                    }
                }
            }
        }
    }

    void LastStand()
    {
        GameObject queen = singleplayerUI.GetComponent<Singleplayer_UI>().playerQueen;
        if (queen != null)
        {
            queen.GetComponent<BaseAntQueenAI>().lastStand = true;
        }
    }

    void Supersoldier()
    {
        Button button = singleplayerUI.GetComponent<Singleplayer_UI>().buySuperSoldier;
        button.gameObject.SetActive(true);
    }
}
