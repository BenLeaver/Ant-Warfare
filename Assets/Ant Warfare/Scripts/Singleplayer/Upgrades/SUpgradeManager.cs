using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the application of upgrades to ants and the player's colony in singleplayer mode.
/// Handles modifying stats, enabling abilities, and unlocking special units.
/// </summary>
public class SUpgradeManager : MonoBehaviour
{
    public GameObject singleplayerUI;
    private int thisTeam = -1;

    /// <summary>
    /// Applies the effect of an upgrade.
    /// </summary>
    /// <param name="name">The name of the upgrade to apply.</param>
    public void ApplyUpgrade(string name)
    {
        if (thisTeam == -1)
        {
            // Need to get player team
            GameObject player = singleplayerUI.GetComponent<Singleplayer_UI>().player;
            UnitInfo playerInfo = player.GetComponent<Player_Singleplayer>().myInfo;
            
            if (playerInfo.IsAliveAndActive())
            {
                thisTeam = playerInfo.team;
            }
            else
            {
                return;
            }
        }
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
        List<UnitInfo> friendlyUnits = UnitManager.Instance.GetTeamUnits(thisTeam);
        foreach (UnitInfo u in friendlyUnits)
        {
            if (u.playerScript != null)
            {
                u.playerScript.moveSpeed *= speedMult;
            }
            if (u.antWorld != null)
            {
                u.antWorld.UpgradeSpeed(speedMult);
            }
        }
    }

    void StrongerSoldiers(float damageMult)
    {
        List<UnitInfo> friendlyUnits = UnitManager.Instance.GetTeamUnits(thisTeam);
        foreach (UnitInfo u in friendlyUnits)
        {
            if (u.unitType == UnitType.Soldier)
            {
                u.antWorld.UpgradeAttack(damageMult);
            }
        }
    }

    void LessFoodWaste(float foodMult)
    {
        List<UnitInfo> friendlyUnits = UnitManager.Instance.GetTeamUnits(thisTeam);
        foreach (UnitInfo u in friendlyUnits)
        {
            if (u.playerScript != null)
            {
                u.playerScript.foodMult = foodMult;
            }
            if (u.antWorld != null)
            {
                u.antWorld.UpgradeFoodMult(foodMult);
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
        List<UnitInfo> friendlyUnits = UnitManager.Instance.GetTeamUnits(thisTeam);
        foreach (UnitInfo u in friendlyUnits)
        {
            if (u.unitType == UnitType.Soldier)
            {
                u.antWorld.ActivateFirstAid();
            }
        }
    }

    void LongStingers(float damageMult)
    {
        List<UnitInfo> friendlyUnits = UnitManager.Instance.GetTeamUnits(thisTeam);
        foreach (UnitInfo u in friendlyUnits)
        {
            if (u.playerScript != null)
            {
                u.playerScript.UpgradeAttack(damageMult);
            }
            if (u.antWorld != null)
            {
                u.antWorld.UpgradeAttack(damageMult);
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
        List<UnitInfo> friendlyUnits = UnitManager.Instance.GetTeamUnits(thisTeam);
        foreach (UnitInfo u in friendlyUnits)
        {
            if (u.unitType == UnitType.Worker)
            {
                u.antWorld.UpgradeSpeed(speedMult);
                u.antWorld.UpgradeAttack(damageMult);
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
        Button button = singleplayerUI.GetComponent<Singleplayer_UI>().fireSuperSoldier;
        button.gameObject.SetActive(true);
    }
}
