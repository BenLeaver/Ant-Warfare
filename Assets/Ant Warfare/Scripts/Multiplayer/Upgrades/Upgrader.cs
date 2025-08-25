using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

/// <summary>
/// Handles applying upgrades to ants for a specific team in multiplayer.
/// </summary>
public class Upgrader : NetworkBehaviour
{
    private GameObject[] Ants;
    private int thisTeam;

    /// <summary>
    /// RPC which applies the effect of an upgrade.
    /// </summary>
    /// <param name="team">The team to apply the upgrade to.</param>
    /// <param name="name">The name of the upgrade to apply.</param>
    [Rpc(SendTo.Everyone)]
    public void SyncUpgradeRpc(int team, string name)
    {
        Debug.Log($"Sync Upgrade {team} {name}");
        Ants = GameObject.FindGameObjectsWithTag("Ant");
        thisTeam = team;

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
        // Supersoldier activated in Multiplayer_UI.
        if (name == "Last Stand")
        {
            LastStand();
        }
    }

    void MovementSpeed(float speedMult)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MHealth>().team == thisTeam)
            {
                if (a.GetComponent<PlayerController>())
                {
                    a.GetComponent<PlayerController>().moveSpeed *= speedMult;
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
            if (a.GetComponent<MHealth>().team == thisTeam)
            {
                if (a.GetComponent<MAntBaseAI>())
                {
                    if (a.GetComponent<MAntBaseAI>().type == "Soldier")
                    {
                        int current = a.GetComponent<MAntBaseAI>().attack;
                        a.GetComponent<MAntBaseAI>().attack = Mathf.RoundToInt(current * damageMult);
                    }
                }
            }
        }
    }

    void LessFoodWaste(float foodMult)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MHealth>().team == thisTeam)
            {
                if (a.GetComponent<PlayerController>())
                {
                    a.GetComponent<PlayerController>().foodMult = foodMult;
                }
                if (a.GetComponent<MAntBaseAI>())
                {
                    a.GetComponent<MAntBaseAI>().foodMult = foodMult;
                }
            }
        }
    }

    void ColonyCapacity(int capacityIncrease)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MBaseAntQueenAI>() && a.GetComponent<MHealth>().team == thisTeam)
            {
                a.GetComponent<MBaseAntQueenAI>().maxColonySize += capacityIncrease;
            }
        }
    }

    void Fortress()
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MBaseAntQueenAI>() && a.GetComponent<MHealth>().team == thisTeam)
            {
                a.GetComponent<MBaseAntQueenAI>().fortress = true;
            }
        }
    }

    void FirstAid()
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MHealth>().team == thisTeam)
            {
                if (a.GetComponent<MAntBaseAI>())
                {
                    if (a.GetComponent<MAntBaseAI>().type == "Soldier")
                    {
                        a.GetComponent<MAntBaseAI>().firstAid = true;
                    }
                }
            }
        }
    }

    void LongStingers(float damageMult)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MHealth>().team == thisTeam)
            {
                if (a.GetComponent<MAntBaseAI>())
                {
                    int current = a.GetComponent<MAntBaseAI>().attack;
                    a.GetComponent<MAntBaseAI>().attack = Mathf.RoundToInt(current * damageMult);
                }
                if (a.GetComponent<PlayerController>())
                {
                    int current = a.GetComponent<PlayerController>().attack;
                    a.GetComponent<PlayerController>().attack = Mathf.RoundToInt(current * damageMult);
                }
            }
        }
    }

    void AphidFarming()
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MBaseAntQueenAI>() && a.GetComponent<MHealth>().team == thisTeam)
            {
                a.GetComponent<MBaseAntQueenAI>().aphidFarming = true;
            }
        }
    }

    void AggressiveWorkers(float speedMult, float damageMult)
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MHealth>().team == thisTeam)
            {
                if (a.GetComponent<MAntBaseAI>())
                {
                    if (a.GetComponent<MAntBaseAI>().type == "Worker")
                    {
                        int current = a.GetComponent<MAntBaseAI>().attack;
                        a.GetComponent<MAntBaseAI>().attack = Mathf.RoundToInt(current * damageMult);
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
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MBaseAntQueenAI>() && a.GetComponent<MHealth>().team == thisTeam)
            {
                a.GetComponent<MBaseAntQueenAI>().lastStand = true;
            }
        }
    }
}
