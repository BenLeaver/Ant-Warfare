using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

/// <summary>
/// Controls the behavior of a queen ant in a multiplayer colony. 
/// Handles spawning of ants, upgrades, and passive food generation.
/// </summary>
public class MBaseAntQueenAI : NetworkBehaviour
{
    public GameObject m_player;
    public string species;

    public bool playerOnTeam = true;
    public int food = 100;
    private int passiveFoodIncome = 0;
    private float passiveTimer = 0f;

    public GameObject superSoldierPrefab;
    public int superSoldierCost = 200;

    public GameObject soldierPrefab;
    public int soldierCost = 50;

    public GameObject workerPrefab;
    public int workerCost = 25;

    public Vector3 spawnPos;
    public int difficulty = 3;

    [Header("Brain")]
    public string command = "none";
    public int totalSoldiers = 0;
    public int totalWorkers = 0;
    public int colonySize = 0;
    public int maxColonySize = 50;
    public GameObject[] Ants;
    private int strengthRanking = 1;
    public Vector3 attackLocation;

    [Header("Health")]
    public MHealth healthScript;

    [Header("Fire Ant")]
    private bool initialWaveSpawned = false;

    [Header("Upgrades")]
    private List<string> upgradesSelected = new List<string>();
    public bool fortress = false;
    public float lastHealTime = 0f;
    public bool aphidFarming = false;
    public bool lastStand = false;

    /// <summary>
    /// Adds an upgrade to a list of selected upgrades, used to ensure the upgrades apply to 
    /// newly spawned ants.
    /// </summary>
    /// <param name="upgrade"></param>
    [Rpc(SendTo.Everyone)]
    public void AddSelectedUpgradeRpc(string upgrade)
    {
        upgradesSelected.Add(upgrade);
    }

    // Update is called once per frame
    void Update()
    {
        passiveTimer += Time.deltaTime;
        colonySize = totalSoldiers + totalWorkers;
        if (passiveTimer >= 1f)
        {
            food += passiveFoodIncome;
            passiveTimer -= 1;
        }

        if (!playerOnTeam)
        {
            Ants = GameObject.FindGameObjectsWithTag("Ant");
            Brain();
        }
        else
        {
            if (species == "Black")
            {
                passiveFoodIncome = 1;
            }
            else if (species == "Fire")
            {
                if (aphidFarming)
                {
                    passiveFoodIncome = 2;
                }
                else
                {
                    passiveFoodIncome = 0;
                }
            }
        }

        if(species == "Fire" && initialWaveSpawned == false)
        {
            for (int i = 0; i < 4; i++)
            {
                food += workerCost;
                SpawnWorker();
            }
            initialWaveSpawned = true;
        }

        if (fortress)
        {
            UpdateFortressHeal();
        }

        if (lastStand && healthScript.health < (healthScript.maxHealth * 0.75f))
        {
            lastStand = false;
            LastStand();
        }
    }

    void UpdateFortressHeal()
    {
        if (healthScript.health < healthScript.maxHealth)
        {
            lastHealTime += Time.deltaTime;
            if (lastHealTime >= 1f)
            {
                lastHealTime -= 1f;
                if (healthScript.health + 5 > healthScript.maxHealth)
                {
                    healthScript.SetHealthToMax();
                }
                else
                {
                    healthScript.UpdateHealth(-5);
                }
            }
        }
    }

    public void LastStand()
    {
        for (int i = 0; i < 10; i++)
        {
            if (IsServer)
            {
                int team = gameObject.GetComponent<MHealth>().team;
                var newAnt = Instantiate(soldierPrefab, spawnPos, Quaternion.identity);
                newAnt.GetComponent<NetworkObject>().Spawn();
                UpdateSpawnedAntClientRpc(newAnt.GetComponent<NetworkObject>(), team);
            }
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            m_player.GetComponent<PlayerController>().playSpawnRpc();
        }
    }

    public void SpawnSoldier()
    {
        if (food >= soldierCost && colonySize < maxColonySize)
        {
            //Will instantiate soldier and set the correct team
            if (IsServer)
            {
                int team = gameObject.GetComponent<MHealth>().team;
                var newAnt = InstantiateAnt(soldierPrefab);
                newAnt.GetComponent<NetworkObject>().Spawn();
                UpdateSpawnedAntClientRpc(newAnt.GetComponent<NetworkObject>(), team);
            }
            food -= soldierCost;
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            m_player.GetComponent<PlayerController>().playSpawnRpc();
        }
    }

    public void SpawnWorker()
    {
        if (food >= workerCost && colonySize < maxColonySize)
        {
            //Will instantiate worker and set the correct team
            if (IsServer)
            {
                int team = gameObject.GetComponent<MHealth>().team;
                var newAnt = InstantiateAnt(workerPrefab);
                newAnt.GetComponent<NetworkObject>().Spawn();
                UpdateSpawnedAntClientRpc(newAnt.GetComponent<NetworkObject>(), team);
            }
            food -= workerCost;
            totalWorkers += 1;
            colonySize = totalSoldiers + totalWorkers;
            m_player.GetComponent<PlayerController>().playSpawnRpc();
        }
    }

    public void SpawnSuperSoldier()
    {
        if (food >= superSoldierCost && colonySize < maxColonySize)
        {
            if (IsServer)
            {
                int team = gameObject.GetComponent<MHealth>().team;
                var newAnt = InstantiateAnt(superSoldierPrefab);
                newAnt.GetComponent<NetworkObject>().Spawn();
                UpdateSpawnedAntClientRpc(newAnt.GetComponent<NetworkObject>(), team);
            }
            food -= superSoldierCost;
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            m_player.GetComponent<PlayerController>().playSpawnRpc();
        }
    }

    /// <summary>
    /// Instantiates ant prefab at the spawn position with some slight variation, to fix the 
    /// problem of multiple ants spawning on top of each other and looking like one ant.
    /// </summary>
    /// <param name="antPrefab">Prefab of the ant to spawn.</param>
    /// <returns>The Instantiated Ant GameObject.</returns>
    public GameObject InstantiateAnt(GameObject antPrefab)
    {
        float rX = Random.Range(-2f, 2f);
        float rY = Random.Range(-2f, 2f);
        Vector3 position = new Vector3(spawnPos.x + rX, spawnPos.y + rY, 0);
        return Instantiate(antPrefab, position, Quaternion.identity);
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateSpawnedAntClientRpc(NetworkObjectReference antRef, int team)
    {
        var a = ((GameObject)antRef);
        a.GetComponent<MHealth>().UpdateTeam(team);
        a.GetComponent<MAntBaseAI>().SetFriendlyPlayer(m_player);
        // Apply Upgrades
        foreach (string u in upgradesSelected)
        {
            Debug.Log(u);
            if (u == "Movement Speed")
            {
                a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.15f;
            }
            if (u == "Less Food Waste")
            {
                a.GetComponent<MAntBaseAI>().foodMult = 1.3f;
            }
            if (u == "Rapid Movement")
            {
                a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.2f;
            }
            if (u == "Long Stingers")
            {
                int current = a.GetComponent<MAntBaseAI>().attack;
                a.GetComponent<MAntBaseAI>().attack = Mathf.RoundToInt(current * 1.2f);
            }
        }

        if (a.GetComponent<MAntBaseAI>().type == "Soldier")
        {
            foreach (string u in upgradesSelected)
            {
                if (u == "Stronger Soldiers")
                {
                    int current = a.GetComponent<MAntBaseAI>().attack;
                    a.GetComponent<MAntBaseAI>().attack = Mathf.RoundToInt(current * 1.2f);
                }
                if (u == "First Aid")
                {
                    a.GetComponent<MAntBaseAI>().firstAid = true;
                }
            }
        }

        if (a.GetComponent<MAntBaseAI>().type == "Worker")
        {
            foreach (string u in upgradesSelected)
            {
                if (u == "Aggressive Workers")
                {
                    int current = a.GetComponent<MAntBaseAI>().attack;
                    a.GetComponent<MAntBaseAI>().attack = Mathf.RoundToInt(current * 1.1f);
                    a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.1f;
                }
            }
        }
    }

    public void SpawnDecision()
    {
        while (food < workerCost)
        {
            if (colonySize < 5)
            {
                SpawnWorker();
            }
            else if (colonySize < 30)
            {
                if (totalSoldiers * 2 < totalWorkers)
                {
                    if (food >= soldierCost)
                    {
                        SpawnSoldier();
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    SpawnWorker();
                }
            }
            else
            {
                if (food >= soldierCost)
                {
                    SpawnSoldier();
                }
                else
                {
                    break;
                }
            }
        }
    }

    public void Brain()
    {
        SpawnDecision();
        command = "none";
        if (difficulty == 0) //Easy
        {
            if (CheckEnemiesInNest())
            {
                command = "retreat";
            }
            else //Will make decision on whether to attack
            {
                int weakestEnemySoldiers = 9999;
                int strongestEnemySoldiers = 0;
                //will be used to determine if an attack should be carried out
                // - will only happen when this colony is one of the stronger ones
                //Will store the ranking of the colony relative to enemy colonies
                // - so better judgement can be made about whether to attack
                strengthRanking = 1; 
                foreach (GameObject a in Ants)
                {
                    if (a.GetComponent<MBaseAntQueenAI>())
                    {
                        //ant is a queen
                        if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers > totalSoldiers)
                        {
                            strengthRanking += 1; //Lose a place in the strength rankings
                        }
                        if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers < weakestEnemySoldiers) //Weaker enemy found
                        {
                            weakestEnemySoldiers = a.GetComponent<MBaseAntQueenAI>().totalSoldiers;
                            attackLocation = a.GetComponent<MBaseAntQueenAI>().spawnPos; //Weakest enemy will be attacked
                        }
                        else if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers > strongestEnemySoldiers) //Stronger enemy found
                        {
                            strongestEnemySoldiers = a.GetComponent<MBaseAntQueenAI>().totalSoldiers;
                        }
                    }
                }
                //Will attack if this colony is strong enough
                if (((weakestEnemySoldiers * 4) < totalSoldiers) && ((totalSoldiers * 2) > strongestEnemySoldiers)) 
                {
                    command = "attack";
                }

            }
            if (species != "Fire")
            {
                passiveFoodIncome = 1;
            }
        }
        else if (difficulty == 1) //Medium
        {
            if (CheckEnemiesInNest())
            {
                command = "retreat";
            }
            else //Will make decision on whether to attack
            {
                int weakestEnemySoldiers = 9999;
                int strongestEnemySoldiers = 0;
                //int attackChance = 0; //will be used to determine if an attack should be carried out - will only happen when this colony is one of the stronger ones
                strengthRanking = 1; //Will store the ranking of the colony relative to enemy colonies - so better judgement can be made about whether to attack
                foreach (GameObject a in Ants)
                {
                    if (a.GetComponent<MBaseAntQueenAI>())
                    {
                        //ant is a queen
                        if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers > totalSoldiers)
                        {
                            strengthRanking += 1; //Lose a place in the strength rankings
                        }
                        if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers < weakestEnemySoldiers) //Weaker enemy found
                        {
                            weakestEnemySoldiers = a.GetComponent<MBaseAntQueenAI>().totalSoldiers;
                            attackLocation = a.GetComponent<MBaseAntQueenAI>().spawnPos; //Weakest enemy will be attacked
                        }
                        else if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers > strongestEnemySoldiers) //Stronger enemy found
                        {
                            strongestEnemySoldiers = a.GetComponent<MBaseAntQueenAI>().totalSoldiers;
                        }
                    }
                }
                if (((weakestEnemySoldiers * 4) < totalSoldiers) && ((totalSoldiers * 2) > strongestEnemySoldiers)) //Will attack if this colony is strong enough
                {
                    command = "attack";
                }

            }
            if(species != "Fire")
            {
                passiveFoodIncome = 2;
            }
        }
        else if (difficulty == 2) //Hard
        {
            if (CheckEnemiesInNest())
            {
                command = "retreat";
            }
            else //Will make decision on whether to attack
            {
                int weakestEnemySoldiers = 9999;
                int strongestEnemySoldiers = 0;
                //int attackChance = 0; //will be used to determine if an attack should be carried out - will only happen when this colony is one of the stronger ones
                strengthRanking = 1; //Will store the ranking of the colony relative to enemy colonies - so better judgement can be made about whether to attack
                foreach (GameObject a in Ants)
                {
                    if (a.GetComponent<MBaseAntQueenAI>())
                    {
                        //ant is a queen
                        if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers > totalSoldiers)
                        {
                            strengthRanking += 1; //Lose a place in the strength rankings
                        }
                        if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers < weakestEnemySoldiers) //Weaker enemy found
                        {
                            weakestEnemySoldiers = a.GetComponent<MBaseAntQueenAI>().totalSoldiers;
                            attackLocation = a.GetComponent<MBaseAntQueenAI>().spawnPos; //Weakest enemy will be attacked
                        }
                        else if (a.GetComponent<MBaseAntQueenAI>().totalSoldiers > strongestEnemySoldiers) //Stronger enemy found
                        {
                            strongestEnemySoldiers = a.GetComponent<MBaseAntQueenAI>().totalSoldiers;
                        }
                    }
                }
                if (((weakestEnemySoldiers * 4) < totalSoldiers) && ((totalSoldiers * 2) > strongestEnemySoldiers)) //Will attack if this colony is strong enough
                {
                    command = "attack";
                }

            }
            if(species != "Fire")
            {
                passiveFoodIncome = 3;
            }
            else
            {
                passiveFoodIncome = 1;
            }
        }
    }

    public void Death()
    {
        if (playerOnTeam)
        {
            m_player.GetComponent<PlayerController>().ColonyDied();
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Lose");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("GameMusic");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("MenuMusic");
        }
    }

    private bool CheckEnemiesInNest()
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MHealth>().team != healthScript.team)
            {
                float distance = Vector3.Distance(a.transform.position, gameObject.transform.position);
                if (distance <= 20f)
                {
                    //Enemy near nest
                    return true;
                }
            }
        }
        return false;
    }

    public void SetPlayerOnTeam(bool onTeam)
    {
        playerOnTeam = onTeam;
    }
}
