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
    public UnitInfo myInfo;

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
    public UnitInfo queenToAttack;

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

    public void Initialise(int team, Vector3 spawnPos, GameObject playerRef=null, int difficulty=0)
    {
        this.spawnPos = spawnPos;
        this.player = playerRef;
        this.difficulty = difficulty;
        this.playerOnTeam = (playerRef != null);

        healthScript.UpdateTeam(team);
        myInfo = UnitManager.Instance.RegisterUnit(gameObject);
    }

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

        if (lastStand && healthScript.Health < (healthScript.MaxHealth * 0.75f))
        {
            lastStand = false;
            LastStand();
        }
    }


    void UpdateFortressHeal()
    {
        if (healthScript.Health < healthScript.MaxHealth)
        {
            lastHealTime += Time.deltaTime;
            if (lastHealTime >= 1f)
            {
                lastHealTime -= 1f;
                if (healthScript.Health + 5 > healthScript.MaxHealth)
                {
                    healthScript.Health = healthScript.MaxHealth;
                }
                else
                {
                    healthScript.Health += 5;
                }
            }
        }
    }

    public void LastStand()
    {
        for (int i = 0; i < 10; i++)
        {
            
            GameObject soldier = InstantiateAnt(soldierPrefab);
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            if (playerOnTeam)
            {
                AudioManager.instance.Play("Spawn");
                ApplySoldierUpgrades(soldier);
            }
        }
    }

    public void SpawnSoldier()
    {
        if (food >= soldierCost && colonySize < maxColonySize)
        {
            //Will instantiate soldier and set the correct team
            GameObject soldier = InstantiateAnt(soldierPrefab);
            
            food -= soldierCost;
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            if (playerOnTeam)
            {
                AudioManager.instance.Play("Spawn");
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
                a.GetComponent<IAntWorld>().UpgradeSpeed(1.15f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Stronger Soldiers")
            {
                a.GetComponent<IAntWorld>().UpgradeAttack(1.2f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Less Food Waste")
            {
                a.GetComponent<IAntWorld>().UpgradeFoodMult(1.3f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "First Aid")
            {
                a.GetComponent<IAntWorld>().ActivateFirstAid();
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Rapid Movement")
            {
                a.GetComponent<IAntWorld>().UpgradeSpeed(1.2f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Long Stingers")
            {
                a.GetComponent<IAntWorld>().UpgradeAttack(1.2f);
            }
        }
    }

    public void SpawnWorker()
    {
        if (food >= workerCost && colonySize < maxColonySize)
        {
            //Will instantiate worker and set the correct team
            GameObject worker = InstantiateAnt(workerPrefab);
            food -= workerCost;
            totalWorkers += 1;
            colonySize = totalSoldiers + totalWorkers;
            if (playerOnTeam)
            {
                AudioManager.instance.Play("Spawn");
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
                a.GetComponent<IAntWorld>().UpgradeSpeed(1.15f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Less Food Waste")
            {
                a.GetComponent<IAntWorld>().UpgradeFoodMult(1.3f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Rapid Movement")
            {
                a.GetComponent<IAntWorld>().UpgradeSpeed(1.2f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Long Stingers")
            {
                a.GetComponent<IAntWorld>().UpgradeAttack(1.2f);
            }
            if (u.GetComponent<Upgrade>().upgradeName == "Aggressive Workers")
            {
                a.GetComponent<IAntWorld>().UpgradeAttack(1.1f);
                a.GetComponent<IAntWorld>().UpgradeSpeed(1.1f);
            }
        }
    }

    public void SpawnSuperSoldier()
    {
        if (food >= superSoldierCost && colonySize < maxColonySize)
        {
            GameObject soldier = InstantiateAnt(superSoldierPrefab);
            food -= superSoldierCost;
            totalSoldiers += 1;
            colonySize = totalSoldiers + totalWorkers;
            if (playerOnTeam)
            {
                AudioManager.instance.Play("Spawn");
                ApplySoldierUpgrades(soldier);
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
        // Spawn near spawn position.
        float rX = Random.Range(-2f, 2f);
        float rY = Random.Range(-2f, 2f);
        Vector3 position = new Vector3(spawnPos.x + rX, spawnPos.y + rY, 0);
        var ant = Instantiate(antPrefab, position, Quaternion.identity);

        // Set the ant's team.
        int team = myInfo.team;
        ant.GetComponent<IHealth>().UpdateTeam(team);

        // Initialise the ant with a reference to the queen.
        ant.GetComponent<SingleplayerAntWorld>().InitializeAntFromQueen(myInfo);

        return ant;
    }

    public void SpawnDecision()
    {
        int safety = 0;

        while ((food >= workerCost && colonySize < 30) || (food >= soldierCost && colonySize < maxColonySize))
        {
            safety++;
            if (safety > 100)
            {
                Debug.LogError("SpawnDecision runaway loop detected");
                break;
            }

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
            else if (colonySize < maxColonySize)
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

        List<UnitInfo> queens = UnitManager.Instance.GetAllQueens();

        foreach (UnitInfo q in queens)
        {
            topStrength = Mathf.Max(q.queenScript.getStrength(), topStrength);
        }
        return topStrength - getStrength();
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
        List<UnitInfo> enemyQueens = UnitManager.Instance.GetEnemyQueens(myInfo.team);

        // Pick a random enemy queen to attack
        int randomIndex = Random.Range(0, enemyQueens.Count);
        queenToAttack = enemyQueens[randomIndex];
        attackLocation = queenToAttack.queenScript.spawnPos;

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
        
        // Attack Check disabled for now as currently ants don't listen to the queen's commands.
        // It also makes testing easier.
        //AttackCheck();

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
            //StartCoroutine(AttackCheck());
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
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.UnregisterUnit(myInfo);
        }

        if (playerOnTeam)
        {
            if (UnitManager.Instance != null)
                UnitManager.Instance.ClearAllUnits();
            AudioManager.instance.Play("Lose");
            AudioManager.instance.Stop("GameMusic");
            AudioManager.instance.Play("MenuMusic");
            SceneManager.LoadScene("LoseMenu");
        }
        else
        {
            GameObject.Find("SGameManager").GetComponent<SGameManager>().TeamDied();
            KillColony();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Return whether the queen can see any enemies (within 20 units).
    /// </summary>
    private bool CheckEnemiesInNest()
    {
        List<UnitInfo> visibleEnemies = UnitManager.Instance.GetVisibleEnemyUnits(myInfo.transform.position, myInfo.team, 20f);
        return visibleEnemies.Count > 0;
    }

    /// <summary>
    /// Will get all ants in this colony, and kill them instantly.
    /// </summary>
    private void KillColony()
    {
        var friendlyUnits = UnitManager.Instance.GetTeamUnits(myInfo.team);

        var copy = new List<UnitInfo>(friendlyUnits);
        foreach (UnitInfo u in copy)
        {
            if (u.go != myInfo.go)
            {
                // Ensure all other ants in the colony die (no one is surviving 999999 damage).
                u.health.UpdateHealth(999999);
            }
        }
    }
}
