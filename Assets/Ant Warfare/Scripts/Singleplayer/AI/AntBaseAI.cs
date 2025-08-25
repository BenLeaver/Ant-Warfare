using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Base AI behavior for all ants, handling movement (following pheremone markers), 
/// attacking, food gathering, and upgrade buffs.
/// </summary>
public class AntBaseAI : MonoBehaviour
{
    [Header("Type and Species")]
    public string type; //e.g., Worker, Soldier
    public string species; //e.g., Black, Fire

    [Header("Movement")]
    public Vector3 target = new Vector3(0, 0, 0);
    public Animator anim;
    UnityEngine.AI.NavMeshAgent agent;
    private Quaternion closestMarkerRotation;
    private bool pathSpeedBuffApplied = false;

    [Header("Health Script")]
    public SHealth healthScript;

    [Header("Attacking")]
    public int attack = 30;
    private GameObject closestEnemy;
    public const float ATTACK_RANGE = 2.5f;
    public const float ATTACK_DELAY = 1f;
    public float attackTimer = 5f;
    private GameObject enemyToAttack;
    
    
    [Header("Food")]
    private GameObject[] Food;
    public const float PICKUP_RANGE = 2f;
    public GameObject foodCarried;
    public float queenRange = 5f;
    public float foodMult = 1f;
    private GameObject closestFood;

    [Header("Decision Making")]
    public int playerCommandIndex = -1;
    public GameObject friendlyPlayer;
    private float lastMovement;
    public const float MOVEMENT_DELAY = 0.1f;
    public const float RANDOM_POS_DELAY = 4f;
    private float randomDelayIncrease = 0f;
    private float delayIncreaseTimer = 0.1f;
    private float lastRandomPlayerFollowPos = 4f;
    private Vector3 randomPlayerFollowPos;
    private float lastRandomNestPos = 4f;
    private Vector3 randomNestPos;
    private float lastRandomDirectionPos = 4f;
    private Vector3 randomDirectionPos;
    public GameObject closestMarker;

    [Header("Other")]
    public Transform mouth;
    private GameObject[] Ants;
    private GameObject queen;
    private BaseAntQueenAI queenScript;

    [Header("Upgrades")]
    private bool fortressBuffApplied = false;
    public bool firstAid = false;
    private float lastHealTime;


    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        Ants = GameObject.FindGameObjectsWithTag("Ant");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        
        //Queen might be dead, so need to check if it exists
        if (GameObject.Find(species + "AntQueen" + gameObject.GetComponent<SHealth>().team.ToString())) 
        {
            queen = GameObject.Find(species + "AntQueen" + gameObject.GetComponent<SHealth>().team.ToString());
            queenScript = queen.GetComponent<BaseAntQueenAI>();
            if (queenScript.fortress)
            {
                UpdateFortressBuff();
            }
            if (queenScript.playerOnTeam)
            {
                friendlyPlayer = queenScript.player;
            }

            Ants = GameObject.FindGameObjectsWithTag("Ant");
            attackTimer += Time.deltaTime;

            if (firstAid)
            {
                FirstAid();
            }

            UpdateDelayIncrease();
            FindClosestEnemy();
            FindClosestFood();
            MovementDecision();
            MoveToTarget();
        }
        else
        {
            //Will kill themselves if no queen
            Death();
        }
    }

    /// <summary>
    /// Allows for soldier to heal if it has first aid upgrade.
    /// </summary>
    public void FirstAid()
    {
        if (type == "Soldier")
        {
            if (healthScript.health < healthScript.maxHealth)
            {
                lastHealTime += Time.deltaTime;
                if (lastHealTime > 1f)
                {
                    lastHealTime -= 1f;
                    if (healthScript.health + 2 > healthScript.maxHealth)
                    {
                        healthScript.health = healthScript.maxHealth;
                    }
                    else
                    {
                        healthScript.health += 2;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks and updates the fortress upgrade attack buff if necessary.
    /// </summary>
    public void UpdateFortressBuff()
    {
        if (Vector3.Distance(transform.position, queen.transform.position) < 30f && !fortressBuffApplied)
        {
            fortressBuffApplied = true;
            attack += 15;
        }
        else if (Vector3.Distance(transform.position, queen.transform.position) >= 30f && fortressBuffApplied)
        {
            fortressBuffApplied = false;
            attack -= 15;
        }
    }

    /// <summary>
    /// Changes randomDelayIncrease every 0.1 seconds.
    /// </summary>
    void UpdateDelayIncrease()
    {
        delayIncreaseTimer += Time.deltaTime;
        if (delayIncreaseTimer >= 0.1)
        {
            delayIncreaseTimer -= 0.1f;
            randomDelayIncrease = Random.Range(0f, 2f);
        }
    }

    /// <summary>
    /// Will update the movement target.
    /// </summary>
    void MovementDecision()
    {
        string queenAICommand = queenScript.command;
        float queenAttackDist = Vector3.Distance(queenScript.attackLocation, transform.position);
        float queenDistance = Vector3.Distance(queen.transform.position, mouth.transform.position);

        float closestEnemyDist = 9999;
        if (closestEnemy != null)
        {
            closestEnemyDist = Vector3.Distance(closestEnemy.transform.position, transform.position);
        }
        float closestFoodDist = 9999;
        if (closestFood != null)
        {
            closestFoodDist = Vector3.Distance(closestFood.transform.position, transform.position);
        }

        if (friendlyPlayer != null)
        {
            playerCommandIndex = GetCommandIndex();
        }

        float closestMarkerDistance = 0;
        if (closestMarker != null)
        {
            closestMarkerDistance = Vector3.Distance(closestMarker.transform.position, transform.position);
        }
        
        if (foodCarried != null && playerCommandIndex == 8 && queenDistance >= 15 && closestMarkerDistance <= 10)
        {
            // Food Return path
            MoveTowardsDirection();
        }
        else if (foodCarried != null && playerCommandIndex == 8 && queenDistance >= 15 && IsAntOnEntrySide())
        {
            // Food Return path
            MoveTowardsMarker();
        }
        else if (foodCarried != null)
        {
            MoveTowardsFeedQueen();
        }
        else if (playerCommandIndex == 1 && closestEnemyDist >= 3)
        {
            // Follow player
            MoveTowardsPlayerFollow();
        }
        else if (playerCommandIndex == 2 && closestEnemyDist >= 8)
        {
            // Retreat to nest
            MoveTowardsNest();
        }
        else if (playerCommandIndex == 3 && closestEnemy != null)
        {
            // Attack
            MoveTowardsEnemy();
        }
        else if (playerCommandIndex == 4 && closestFood != null)
        {
            // Gather Food
            MoveTowardsFood();
        }
        else if (playerCommandIndex == 5 && closestEnemyDist >= 10 && closestFoodDist >= 10 && closestMarkerDistance <= 10)
        {
            // Foraging path
            MoveTowardsDirection();
        }
        else if (playerCommandIndex == 5 && closestEnemyDist >= 10 && closestFoodDist >= 10 && IsAntOnEntrySide())
        {
            // Foraging path
            MoveTowardsMarker();
        }
        else if (playerCommandIndex == 6 && closestEnemyDist >= 10 && (type == "Soldier" || type == "Super Soldier") && closestMarkerDistance <= 10)
        {
            // Guard path
            MoveTowardsDirection();
        }
        else if (playerCommandIndex == 6 && closestEnemyDist >= 10 && (type == "Soldier" || type == "Super Soldier") && IsAntOnEntrySide())
        {
            // Guard path
            MoveTowardsMarker();
        }
        else if (playerCommandIndex == 7 && closestFoodDist >= 10 && closestEnemyDist >= 3 && type == "Worker" && closestMarkerDistance <= 10)
        {
            // Food Gathering path
            MoveTowardsDirection();
        }
        else if (playerCommandIndex == 7 && closestFoodDist >= 10 && closestEnemyDist >= 3 && type == "Worker" && IsAntOnEntrySide())
        {
            // Food Gathering path
            MoveTowardsMarker();
        }
        else if (playerCommandIndex == -1 && queenAICommand == "attack" && closestEnemyDist >= 8)
        {
            MoveTowardsQueenAttack();
        }
        else if (playerCommandIndex == -1 && queenAICommand == "retreat" && closestEnemyDist >= 8 
            && closestFoodDist >= 8)
        {
            MoveTowardsNest();
        }
        else if (type == "Worker" && closestFoodDist < closestEnemyDist)
        {
            MoveTowardsFood();
        }
        else if ((type == "Soldier" || type == "Super Soldier") && closestFoodDist < closestEnemyDist - 15)
        {
            MoveTowardsFood();
        }
        else if (closestEnemy != null)
        {
            MoveTowardsEnemy();
        }
        else
        {
            MoveTowardsNest();
        }
    }

    /// <summary>
    /// Finds the nearest pheremone marker that applies to this ant and gets the command type index.
    /// Also updates the closestMarkerDirection variable.
    /// </summary>
    /// <returns>The command/pheremone type index.</returns>
    public int GetCommandIndex()
    {
        List<GameObject> markers = friendlyPlayer.GetComponent<PheremoneMarkerManager>().markers;
        closestMarker = null;
        float closestDistance = 9999f;
        foreach (GameObject m in markers)
        {
            float currentDist = Vector3.Distance(transform.position, m.transform.position);
            if (currentDist < closestDistance)
            {
                int n = m.GetComponent<MarkerData>().commandNumber;
                if (n <= 5 && foodCarried == null)
                {
                    closestDistance = currentDist;
                    closestMarker = m;
                }
                else if (n == 6 && foodCarried == null && (type == "Soldier" || type == "Supersoldier"))
                {
                    closestDistance = currentDist;
                    closestMarker = m;
                }
                else if (n == 7 && foodCarried == null && type == "Worker")
                {
                    closestDistance = currentDist;
                    closestMarker = m;
                }
                else if (n == 8 && foodCarried != null)
                {
                    closestDistance = currentDist;
                    closestMarker = m;
                }
            }
        }
        if (closestMarker != null)
        {
            if (closestMarker.GetComponent<MarkerData>().isPathMarker && !pathSpeedBuffApplied)
            {
                agent.speed *= 1.20f;
                pathSpeedBuffApplied = true;
            }
            else if (!closestMarker.GetComponent<MarkerData>().isPathMarker && pathSpeedBuffApplied)
            {
                float currentSpeed = agent.speed;
                agent.speed = currentSpeed * 100 / 120;
                pathSpeedBuffApplied = false;
            }
            closestMarkerRotation = closestMarker.transform.rotation;
            return closestMarker.GetComponent<MarkerData>().commandNumber;
        }
        else if (pathSpeedBuffApplied)
        {
            float currentSpeed = agent.speed;
            agent.speed = currentSpeed * 100 / 120;
            pathSpeedBuffApplied = false;
        }
        return -1;
    }

    /// <summary>
    /// Movement target will be set to the friendly queen of the ant.
    /// </summary>
    void MoveTowardsFeedQueen()
    {
        target = queen.transform.position;
        CheckInNest();
    }

    /// <summary>
    /// Movement target will be set to a position near the friendly player.
    /// </summary>
    void MoveTowardsPlayerFollow()
    {
        float playerDist = Vector3.Distance(friendlyPlayer.transform.position, transform.position);
        lastRandomPlayerFollowPos += Time.deltaTime;

        // Check if ant has waited long enough to generate a new random position.
        // Some randomness added to the time waited to make movement more natural.
        if (lastRandomPlayerFollowPos >= RANDOM_POS_DELAY + randomDelayIncrease)
        {
            // Generate new random position near player.
            float rX = Random.Range(-8f, 8f) + friendlyPlayer.transform.position.x;
            float rY = Random.Range(-8f, 8f) + friendlyPlayer.transform.position.y;
            randomPlayerFollowPos = new Vector3(rX, rY, 0);
            lastRandomPlayerFollowPos -= RANDOM_POS_DELAY + randomDelayIncrease;
            target = randomPlayerFollowPos;
        }
        else if (lastRandomPlayerFollowPos >= RANDOM_POS_DELAY / 2 && playerDist <= 10)
        {
            target = transform.position;
        }
        else
        {
            target = randomPlayerFollowPos;
        }
    }

    /// <summary>
    /// Moves in the direction given by the nearest marker's rotation.
    /// </summary>
    void MoveTowardsDirection()
    {
        lastRandomDirectionPos += Time.deltaTime;
        float distance = 5f;        
        Vector3 direction = closestMarkerRotation * Vector3.up;
        Vector3 newPosition = transform.position + direction.normalized * distance;
        if (lastRandomDirectionPos >= RANDOM_POS_DELAY / 8 + randomDelayIncrease / 4)
        {
            float rX = Random.Range(-2f, 2f);
            float rY = Random.Range(-2f, 2f);
            newPosition.x += rX;
            newPosition.y += rY;
            randomDirectionPos = newPosition;
            lastRandomDirectionPos -= RANDOM_POS_DELAY / 8 + randomDelayIncrease / 4;
        }
        target = randomDirectionPos;
    }

    /// <summary>
    /// Moves towards the nearest pheremone marker.
    /// </summary>
    void MoveTowardsMarker()
    {
        target = closestMarker.transform.position;
    }

    /// <summary>
    /// Returns bool representing whether ant is on entry or exit side of path marker arrow.
    /// </summary>
    bool IsAntOnEntrySide()
    {
        Vector3 forward = closestMarkerRotation * Vector3.up;
        Vector3 toAnt = transform.position - closestMarker.transform.position;
        // Dot product used to tell if ant is in front or behind.
        float dot = Vector3.Dot(forward.normalized, toAnt.normalized);
        return dot < 0f;
    }

    /// <summary>
    /// Updates the closestEnemy.
    /// </summary>
    void FindClosestEnemy()
    {
        float closestDistance = -1f;
        closestEnemy = null;
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team != healthScript.team)
            {
                float distance = Vector3.Distance(a.transform.position, mouth.transform.position);
                if (closestDistance == -1 || distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = a;
                }
            }
        }
    }

    /// <summary>
    /// Updates the closestFood.
    /// </summary>
    void FindClosestFood()
    {
        Food = GameObject.FindGameObjectsWithTag("Food");
        closestFood = null;
        float closestDist = -1f;
        foreach (GameObject f in Food)
        {
            if (f.GetComponent<Food>().carried == false)
            {
                float currentDist = Vector3.Distance(f.transform.position, mouth.transform.position);
                if (closestDist == -1 || currentDist < closestDist)
                {
                    closestDist = currentDist;
                    closestFood = f;
                }
            }
        }
    }

    /// <summary>
    /// Movement target will be set to the nearest enemy ant. If the enemy is within attackRange,
    /// and this ant has waited long enough since the last attack, the enemy will be attacked.
    /// </summary>
    void MoveTowardsEnemy()
    {
        target = closestEnemy.transform.position;
        float closestDistance = Vector3.Distance(closestEnemy.transform.position, transform.position);
        if (closestDistance <= ATTACK_RANGE)
        {
            target = transform.position;
            Vector3 vectorToTarget = closestEnemy.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: vectorToTarget);
            if (attackTimer >= ATTACK_DELAY)
            {
                attackTimer = 0f;
                Attack();
            }
        }
    }

    /// <summary>
    /// Movement target will be set to the nearest food.
    /// </summary>
    void MoveTowardsFood()
    {
        target = closestFood.transform.position;
        float foodDist = Vector3.Distance(closestFood.transform.position, transform.position);
        if (foodDist <= PICKUP_RANGE)
        {
            FoodPickup();
        }
    }

    /// <summary>
    /// Movement target will be set to the attack location.
    /// </summary>
    void MoveTowardsQueenAttack()
    {
        target = queenScript.attackLocation;
    }

    /// <summary>
    /// Movement target will be set to the friendly nest.
    /// </summary>
    void MoveTowardsNest()
    {
        float nestDist = Vector3.Distance(queenScript.spawnPos, transform.position);
        lastRandomNestPos += Time.deltaTime;

        // Check if ant has waited long enough to generate a new random position.
        // Some randomness added to the time waited to make movement more natural.
        if (lastRandomNestPos >= RANDOM_POS_DELAY + randomDelayIncrease)
        {
            // Generate new random position in nest.
            float rX = Random.Range(-8f, 8f) + queenScript.spawnPos.x;
            float rY = Random.Range(-8f, 8f) + queenScript.spawnPos.y;
            randomNestPos = new Vector3(rX, rY, 0);
            lastRandomNestPos -= RANDOM_POS_DELAY + randomDelayIncrease;
            target = randomNestPos;
        }
        else if (lastRandomNestPos >= RANDOM_POS_DELAY / 2 && nestDist <= 20)
        {
            target = transform.position;
        }
        else
        {
            target = randomNestPos;
        }
    }

    /// <summary>
    /// Handles actual movement of the ant towards the target using navmesh.
    /// MOVEMENT_DELAY is used to help prevent the target changing too often which results in 
    /// jittery movement.
    /// </summary>
    void MoveToTarget()
    {
        if (Vector3.Distance(target, transform.position) >= 0.1)
        {
            agent.isStopped = false;
            if (lastMovement >= MOVEMENT_DELAY)
            {
                lastMovement -= MOVEMENT_DELAY;
                agent.SetDestination(target);
                anim.SetBool("isWalking", true);
            }
            else
            {
                lastMovement += Time.deltaTime;
            }
        }
        else
        {
            agent.isStopped = true;
            anim.SetBool("isWalking", false);
        }

        if (foodCarried != null)
        {
            //Will look like food is being carried
            foodCarried.transform.position = mouth.transform.position;
            foodCarried.transform.rotation = mouth.transform.rotation;
        }
    }

    /// <summary>
    /// Attacks the closest enemy, where the attack damage is delayed to match the attack 
    /// animation timing.
    /// </summary>
    void Attack()
    {
        anim.SetBool("isAttacking", true);
        enemyToAttack = closestEnemy;
        StartCoroutine(AttackDamage());
    }

    //Deals damage to the enemy after 0.15 seconds to match animations
    IEnumerator AttackDamage()
    {
        yield return new WaitForSeconds(0.25f);
        if(enemyToAttack != null)
        {
            enemyToAttack.GetComponent<SHealth>().UpdateHealth(attack);
        }
        yield return new WaitForSeconds(0.10f);
        anim.SetBool("isAttacking", false);
    }

    void FixedUpdate()
    {
        //Will rotate to look at target
        if (Vector3.Distance(target, transform.position) >= 0.01)
        {
            Vector3 vectorToTarget = target - transform.position;
            transform.rotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: vectorToTarget);
        }
    }

    /// <summary>
    /// Will pickup closestFood.
    /// </summary>
    void FoodPickup()
    {
        if (closestFood != null)
        {
            foodCarried = closestFood;
            foodCarried.GetComponent<Food>().carried = true;
            gameObject.GetComponent<ObjectAudioManager>().Play("Pickup");
        }
    }

    /// <summary>
    /// Will drop foodCarried.
    /// </summary>
    void FoodDrop()
    {
        foodCarried.GetComponent<Food>().carried = false;
        foodCarried = null;
        gameObject.GetComponent<ObjectAudioManager>().Play("Drop");
    }

    /// <summary>
    /// Will check if the ant is close enough to feed the queen.
    /// If so, the colony food will be increased and the food game object will be destroyed, so 
    /// that the ant is ready to pick up another piece of food in the future.
    /// </summary>
    void CheckInNest()
    {
        //Will check if ant is carrying food in nest - so food will be added to colony
        float queenDistance = Vector3.Distance(queen.transform.position, mouth.transform.position);
        if (queenDistance <= queenRange)
        {
            queen.GetComponent<BaseAntQueenAI>().food += Mathf.RoundToInt(foodCarried.GetComponent<Food>().food * foodMult);
            Destroy(foodCarried);
            foodCarried = null;

            if(queen.GetComponent<BaseAntQueenAI>().playerOnTeam == true)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("FoodDropoff");
            }
        }
    }

    public void Death()
    {
        if (foodCarried != null)
        {
            FoodDrop();
        }
        if (GameObject.Find(species + "AntQueen" + gameObject.GetComponent<SHealth>().team.ToString()))
        {
            if (type == "Soldier" || type == "Super Soldier")
            {
                queen.GetComponent<BaseAntQueenAI>().totalSoldiers -= 1;
            }
            else if (type == "Worker")
            {
                queen.GetComponent<BaseAntQueenAI>().totalWorkers -= 1;
            }
        }
        Destroy(gameObject);
    }
}
