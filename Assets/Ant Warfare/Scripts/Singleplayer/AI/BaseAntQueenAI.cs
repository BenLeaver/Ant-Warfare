using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// AI logic for an ant queen. Handles spawning ants, managing upgrades, colony food, and 
/// attack/retreat decisions.
/// </summary>
public class BaseAntQueenAI : MonoBehaviour
{
    public GameObject player;
    public string species;

    public bool playerOnTeam = false;
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
    public Vector3 attackLocation;
    public GameObject queenToAttack;

    [Header("Health")]
    public SHealth healthScript;

    [Header("Fire Ant")]
    private bool initialWaveSpawned = false;

    [Header("Upgrades")]
    private List<GameObject> upgradesSelected = new List<GameObject>();
    public bool fortress = false;
    public float lastHealTime = 0f;
    public bool aphidFarming = false;
    public bool lastStand = false;

    /// <summary>
    /// Adds an upgrade to a list of selected upgrades, used to ensure the upgrades apply to 
    /// newly spawned ants.
    /// </summary>
    public void AddSelectedUpgrade(GameObject upgrade)
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
                    healthScript.health = healthScript.maxHealth;
                }
                else
                {
                    healthScript.health += 5;
                }
            }
        }
    }

    public void LastStand()
    {
        for (int i = 0; i < 10; i++)
        {
            int team = gameObject.GetComponent<SHealth>().team;
            GameObject soldier = Instantiate(soldierPrefab, spawnPos, Quaternion.identity);
            soldier.GetComponent<SHealth>().UpdateTeam(team);
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            if (playerOnTeam)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Spawn");
                ApplySoldierUpgrades(soldier);
            }
        }
    }

    public void SpawnSoldier()
    {
        if (food >= soldierCost && colonySize < maxColonySize)
        {
            //Will instantiate soldier and set the correct team
            int team = gameObject.GetComponent<SHealth>().team;
            GameObject soldier = InstantiateAnt(soldierPrefab);
            soldier.GetComponent<SHealth>().UpdateTeam(team);
            food -= soldierCost;
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            if (playerOnTeam)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Spawn");
                ApplySoldierUpgrades(soldier);
            }
        }
    }

    public void ApplySoldierUpgrades(GameObject a)
    {
        foreach (GameObject u in upgradesSelected)
        {
            if (u.GetComponent<Upgrade>().upgradeName == "Movement Speed")
            {
                a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.15f;
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Stronger Soldiers")
            {
                int current = a.GetComponent<AntBaseAI>().attack;
                a.GetComponent<AntBaseAI>().attack = Mathf.RoundToInt(current * 1.2f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Less Food Waste")
            {
                a.GetComponent<AntBaseAI>().foodMult = 1.3f;
            }
            if (u.GetComponent<Upgrade>().upgradeName == "First Aid")
            {
                a.GetComponent<AntBaseAI>().firstAid = true;
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Rapid Movement")
            {
                a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.2f;
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Long Stingers")
            {
                int current = a.GetComponent<AntBaseAI>().attack;
                a.GetComponent<AntBaseAI>().attack = Mathf.RoundToInt(current * 1.2f);
            }
        }
    }

    public void SpawnWorker()
    {
        if (food >= workerCost && colonySize < maxColonySize)
        {
            //Will instantiate worker and set the correct team
            int team = gameObject.GetComponent<SHealth>().team;
            GameObject worker = InstantiateAnt(workerPrefab);
            worker.GetComponent<SHealth>().UpdateTeam(team);
            food -= workerCost;
            totalWorkers += 1;
            colonySize = totalSoldiers + totalWorkers;
            if (playerOnTeam)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Spawn");
                ApplyWorkerUpgrades(worker);
            }
        }
    }

    public void ApplyWorkerUpgrades(GameObject a)
    {
        foreach (GameObject u in upgradesSelected)
        {
            if (u.GetComponent<Upgrade>().upgradeName == "Movement Speed")
            {
                a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.15f;
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Less Food Waste")
            {
                a.GetComponent<AntBaseAI>().foodMult = 1.3f;
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Rapid Movement")
            {
                a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.2f;
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Long Stingers")
            {
                int current = a.GetComponent<AntBaseAI>().attack;
                a.GetComponent<AntBaseAI>().attack = Mathf.RoundToInt(current * 1.2f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Aggressive Workers")
            {
                int current = a.GetComponent<AntBaseAI>().attack;
                a.GetComponent<AntBaseAI>().attack = Mathf.RoundToInt(current * 1.1f);
                a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.1f;
            }
        }
    }

    public void SpawnSuperSoldier()
    {
        if (food >= superSoldierCost && colonySize < maxColonySize)
        {
            int team = gameObject.GetComponent<SHealth>().team;
            GameObject soldier = InstantiateAnt(superSoldierPrefab);
            soldier.GetComponent<SHealth>().UpdateTeam(team);
            food -= superSoldierCost;
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            if (playerOnTeam)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Spawn");
                ApplySuperSoldierUpgrades(soldier);
            }
        }
    }

    public void ApplySuperSoldierUpgrades(GameObject a)
    {
        foreach (GameObject u in upgradesSelected)
        {
            if (u.GetComponent<Upgrade>().upgradeName == "Rapid Movement")
            {
                a.GetComponent<UnityEngine.AI.NavMeshAgent>().speed *= 1.2f;
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Long Stingers")
            {
                int current = a.GetComponent<AntBaseAI>().attack;
                a.GetComponent<AntBaseAI>().attack = Mathf.RoundToInt(current * 1.2f);
            }
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

    public void SpawnDecision()
    {
        while (food >= workerCost)
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

    /// <summary>
    /// Returns an integer representing the strength of this colony.
    /// </summary>
    public int getStrength()
    {
        return (totalSoldiers * soldierCost * 2) + (totalWorkers * workerCost);
    }

    /// <summary>
    /// Calculates and returns the difference between the strength of the strongest colony and the 
    /// strength of this colony.
    /// </summary>
    public int calcTopStrengthDifference()
    {
        int topStrength = -1;

        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<BaseAntQueenAI>())
            {
                topStrength = Mathf.Max(a.GetComponent<BaseAntQueenAI>().getStrength(), topStrength);
            }
        }
        return topStrength - this.getStrength();
    }

    IEnumerator AttackCheck()
    {
        command = "preparing";
        float randomSeconds = Random.Range(10f, 30f);
        yield return new WaitForSeconds(randomSeconds);
        if (command == "preparing")
        {
            float attackChance = (200f - (calcTopStrengthDifference() / 2f)) / 200f;
            if (Random.value * 4 < attackChance)
            {
                StartCoroutine(CommandAttack());
            }
            else
            {
                command = "none";
            }
        }
    }

    IEnumerator CommandAttack()
    {
        command = "attack";
        queenToAttack = null;
        List<GameObject> enemyQueens = new List<GameObject>();
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<BaseAntQueenAI>() && a != this.gameObject)
            {
                enemyQueens.Add(a);
            }
        }

        int randomIndex = Random.Range(0, enemyQueens.Count);
        queenToAttack = enemyQueens[randomIndex];
        attackLocation = queenToAttack.GetComponent<BaseAntQueenAI>().spawnPos;

        float randomSeconds = Random.Range(15f, 50f);
        yield return new WaitForSeconds(randomSeconds);
        if (command == "attack")
        {
            command = "none";
        }
    }

    public void Brain()
    {
        SpawnDecision();
        AttackCheck();

        if (CheckEnemiesInNest())
        {
            command = "retreat";
        }
        else if (command == "retreat")
        {
            command = "none";
        }

        if (command == "none")
        {
            StartCoroutine(AttackCheck());
        }
        else if (command == "attack" && queenToAttack == null)
        {
            command = "none";
        }
        if (difficulty == 0) //Easy
        {
            if (species != "Fire")
            {
                passiveFoodIncome = 1;
            }
        }
        else if (difficulty == 1) //Medium
        {
            if(species != "Fire")
            {
                passiveFoodIncome = 2;
            }
            else
            {
                passiveFoodIncome = 1;
            }
        }
        else if (difficulty == 2) //Hard
        {
            if(species != "Fire")
            {
                passiveFoodIncome = 3;
            }
            else
            {
                passiveFoodIncome = 2;
            }
        }
    }

    public void Death()
    {
        if (playerOnTeam)
        {
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Lose");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("GameMusic");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("MenuMusic");
            SceneManager.LoadScene("LoseMenu");
        }
        else
        {
            GameObject.Find("SGameManager").GetComponent<SGameManager>().TeamDied();
            Destroy(gameObject);
        }
    }

    private bool CheckEnemiesInNest()
    {
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team != healthScript.team)
            {
                float distance = Vector3.Distance(a.transform.position, gameObject.transform.position);
                if (distance <= 10f)
                {
                    //Enemy near nest
                    return true;
                }
            }
        }
        return false;
    }
}
