using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Ant.AI;

/// <summary>
/// Implementation of IAntWorld for singleplayer, acting as a bridge between the world and the 
/// ant AI state machine.
/// 
/// Initialises and stores a reference to both the state manager and the context.
/// 
/// Each worker/soldier ant owns an instance of this class.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SingleplayerAntWorld : MonoBehaviour, IAntWorld
{
    [SerializeField] private AntStateManager stateManager;
    [SerializeField] private AntContext context;

    private UnitInfo myInfo;

    private NavMeshAgent agent;
    private Animator anim;
    public SHealth healthScript;
    public Transform mouth;

    public UnitInfo queen;
    private BaseAntQueenAI queenScript;
    private GameObject player;
    public GameObject foodCarried;

    public float queenRange = 5f;
    [SerializeField]
    private AntType type;

    public AntType Type => type;

    public string species;
    public int attackDamage;

    [SerializeField] private float sightRange = 20f;
    public float SightRange => sightRange;
    [SerializeField] private float attackRange = 1f;
    public float AttackRange => attackRange;
    [SerializeField] private float attackDelay = 1f;
    public float AttackDelay => attackDelay;
    [SerializeField] private float pickupRange = 1f;
    public float PickupRange => pickupRange;

    public Vector3 Position => myInfo.transform.position;
    public Vector3 MouthPosition => mouth.position;

    private bool isDying = false;

    [Header("Upgrades")]
    private bool fortressBuffApplied = false;
    public bool firstAid = false;
    public float foodMult = 1f;
    private float lastHealTime;


    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        healthScript = GetComponent<SHealth>();
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Called by the queen when this ant is spawned.
    /// 
    /// Sets references to queen, queenScript and player. 
    /// 
    /// After that initialises the state manager system 
    /// (which will automatically check for nearby pheromones).
    /// </summary>
    public void InitializeAntFromQueen(UnitInfo q)
    {
        queen = q;
        queenScript = q.queenScript;
        player = queenScript.player;

        // Initialise Team
        GetComponent<IHealth>().UpdateTeam(q.team);

        // Register unit in manager
        myInfo = UnitManager.Instance.RegisterUnit(gameObject);

        // Initialise state system.
        context = new AntContext(this);
        stateManager = new AntStateManager(context);

        stateManager.Initialize();

        // Initialise AI brain debugging system.
        if (GetComponent<AntAIDebug>())
        {
            GetComponent<AntAIDebug>().context = context;
            GetComponent<AntAIDebug>().manager = stateManager;
        }
    }

    private void OnEnable()
    {
        //if (UnitManager.Instance != null)
        //{
        //    myInfo = UnitManager.Instance.RegisterUnit(gameObject);
        //}
    }

    private void OnDisable()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.UnregisterUnit(myInfo);
        }
    }

    public float DeltaTime => Time.deltaTime;

    /// <summary>
    /// Will be called from the player to start player-follow interrupt.
    /// 
    /// Subtypes:
    /// 0: Never Interrupt
    /// 1: Only interrupt to pick up food
    /// 2: Only interrupt to attack nearby enemies
    /// </summary>
    public bool PlayerFollowStart(int subtype)
    {
        return stateManager.StartPlayerIR(subtype);
    }

    /// <summary>
    /// Will be called from the player to stop ant following player.
    /// </summary>
    public void PlayerFollowStop()
    {
        stateManager.StopPlayerIR();
    }

    // Update is called once per frame
    void Update()
    {
        CheckUpgradesOnUpdate();
        stateManager.Tick(Time.deltaTime);

        // Keep carried food attached to mouth
        if (foodCarried != null)
        {
            foodCarried.transform.position = mouth.transform.position;
            foodCarried.transform.rotation = mouth.transform.rotation;
        }
    }

    private void CheckUpgradesOnUpdate()
    {
        if (queenScript.fortress)
        {
            UpdateFortressBuff();
        }

        if (firstAid)
        {
            FirstAid();
        }
    }

    public void SetRotation(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        float rotationSpeed = 10f;

        direction.z = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction.normalized);

        myInfo.transform.rotation = Quaternion.Slerp(
            myInfo.transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime);
    }

    public void SetDestination(Vector3 target)
    {
        if (!agent.enabled) return;

        anim.SetBool("isWalking", true);
        agent.isStopped = false;
        agent.SetDestination(target);
    }

    public void StopMovement()
    {
        if (!agent.enabled) return;

        anim.SetBool("isWalking", false);
        agent.isStopped = true;
        agent.ResetPath();
    }

    public bool HasReached(Vector3 target, float threshold)
    {
        if (!agent.enabled) return true;

        if (agent.pathPending) return false;

        return agent.remainingDistance <= threshold;
    }

    public bool HasActivePath()
    {
        return (agent.hasPath || agent.pathPending);
    }

    public Vector3 GetReachableNavMeshPoint(Vector3 origin, float minRange, float maxRange,
    int attempts = 50, float snapDistance = 2f)
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Ant is not on NavMesh");
            return myInfo.transform.position;
        }

        // Ensure the origin is on the navmesh.
        if (!NavMesh.SamplePosition(origin, out NavMeshHit originHit, Mathf.Infinity, agent.areaMask))
        {
            origin = agent.transform.position;
        }
        else
        {
            origin = originHit.position;
        }

        for (int i=0; i<attempts; i++)
        {
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            float t = Random.value;
            float dist = Mathf.Lerp(minRange, maxRange, t * t);

            Vector3 candidate = origin + new Vector3(dir2D.x, dir2D.y, 0f) * dist;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, snapDistance, agent.areaMask)) continue;

            Vector3 flattened = new Vector3(hit.position.x, hit.position.y, 0f);

            if (HasValidPath(flattened))
                return flattened;
        }
        Debug.LogWarning($"No valid navmesh point found for origin {origin}, maxRange {maxRange}");
        return myInfo.transform.position;
    }

    private bool HasValidPath(Vector3 target)
    {
        NavMeshPath _path = new NavMeshPath();
        NavMesh.CalculatePath(agent.transform.position, target, NavMesh.AllAreas, _path);
        return _path.status == NavMeshPathStatus.PathComplete;
    }

    /// <summary>
    /// Finds the closest reachable point on the current navmesh to the target.
    /// </summary>
    public Vector3 FindClosestReachablePoint(Vector3 target)
    {
        NavMeshHit hit;

        // Snap target to navmesh
        if (!NavMesh.SamplePosition(target, out hit, 5f, NavMesh.AllAreas))
            return agent.transform.position; // fallback to current position

        // Check if reachable
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(agent.transform.position, hit.position, NavMesh.AllAreas, path);

        if (path.status == NavMeshPathStatus.PathComplete)
            return hit.position;

        // Not reachable -> find closest reachable point
        // Try sampling points around the target
        for (float r = 2f; r <= 20f; r += 2f)
        {
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * r;
                Vector3 candidate = target + offset;

                if (NavMesh.SamplePosition(candidate, out hit, 1f, NavMesh.AllAreas))
                {
                    NavMesh.CalculatePath(agent.transform.position, hit.position, NavMesh.AllAreas, path);
                    if (path.status == NavMeshPathStatus.PathComplete)
                        return hit.position;
                }
            }
        }

        return agent.transform.position;
    }



    public GameObject FindFriendlyQueen()
    {
        return queen.go;
    }

    public float GetFriendlyQueenDist()
    {
        return Vector3.Distance(queen.transform.position, MouthPosition);
    }

    public GameObject FindFriendlyPlayer()
    {
        return player;
    }

    public UnitInfo FindClosestEnemy()
    {
        return UnitManager.Instance.GetClosestEnemyUnit(myInfo.transform.position, myInfo.team, 20f);
    }

    public GameObject FindClosestFood()
    {
        GameObject[] Food = GameObject.FindGameObjectsWithTag("Food");
        GameObject closestFood = null;
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
        return closestFood;
    }

    public int GetFriendlyColonySize()
    {
        int size = queenScript.colonySize;

        if (size < 0) return 0;

        return size; 
    }

    public Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3 target)>> GetPheromonesNearby()
    {
        Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3)>> pheromones 
            = new Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3)>>();

        List<GameObject> markers = queen.go.GetComponent<ColonyPheromonesManager>().markers;
        foreach (GameObject m in markers)
        {
            var mData = m.GetComponent<MarkerData>();

            if (!mData.affectSoldiers && Type == AntType.Soldier) continue;
            if (!mData.affectWorkers && Type == AntType.Worker) continue;

            // Ignore food return path pheromones -> should only affect ants carrying food. 
            if (mData.affectFoodCarriers) continue; 

            float dist = Vector3.Distance(transform.position, m.transform.position);
            
            if (dist > 20f) continue;

            PheromoneType type = mData.type;
            PheromoneSubtype subtype = mData.subtype;
            float strength = mData.strength;
            Vector3 target = mData.target;

            float weight = (20f - dist) * strength;


            if (!pheromones.TryGetValue(type, out var innerDict))
            {
                innerDict = new Dictionary<float, (PheromoneSubtype, Vector3)>();
                pheromones[type] = innerDict;
            }
            innerDict[weight] = (subtype, target);
        }
        return pheromones;
    }

    public Dictionary<float, (PheromoneSubtype subtype, Vector3 target)> GetFoodReturnPathPheromones()
    {
        Dictionary<float, (PheromoneSubtype, Vector3)> foodReturnPheromones
            = new Dictionary<float, (PheromoneSubtype, Vector3)>();

        List<GameObject> markers = queen.go.GetComponent<ColonyPheromonesManager>().markers;
        foreach (GameObject m in markers)
        {
            var mData = m.GetComponent<MarkerData>();

            if (!mData.affectFoodCarriers) continue;

            float dist = Vector3.Distance(transform.position, m.transform.position);
            if (dist > 20f) continue;

            PheromoneSubtype subtype = mData.subtype;
            float strength = mData.strength;
            float weight = (20f - dist) * strength;
            Vector3 target = mData.target;

            foodReturnPheromones[weight] = (subtype, target);
        }

        return foodReturnPheromones;
    }

    public void PlacePheromone(PheromoneSubtype subtype)
    {
        PlacePheromone(subtype, myInfo.transform.rotation);
    }

    public void PlacePheromone(PheromoneSubtype subtype, Quaternion markerRotation)
    {
        queen.go.GetComponent<ColonyPheromonesManager>().PlacePathMarker(subtype, myInfo.transform.position, markerRotation);
    }

    public void Attack(UnitInfo targetEnemy)
    {
        anim.SetBool("isAttacking", true);
        StartCoroutine(AttackDamage(targetEnemy));
    }

    IEnumerator AttackDamage(UnitInfo targetEnemy)
    {
        var attacker = myInfo.go;

        yield return new WaitForSeconds(0.25f);

        // Check attacker is still alive
        if (attacker == null || !attacker.activeInHierarchy)
            yield break;

        if (!targetEnemy.IsAliveAndActive())
        {
            anim.SetBool("isAttacking", false);
            yield break;
        }

        if (targetEnemy.health != null) targetEnemy.health.UpdateHealth(attackDamage);

        yield return new WaitForSeconds(0.10f);

        if (attacker == null || !attacker.activeInHierarchy)
            yield break;

        anim.SetBool("isAttacking", false);
    }

    public void FoodPickup(GameObject food)
    {
        if (!food) return;

        if (food.GetComponent<Food>().carried == false)
        {
            foodCarried = food;
            food.GetComponent<Food>().carried = true;
            gameObject.GetComponent<ObjectAudioManager>().Play("Pickup");
        }
    }

    public void FoodDrop()
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
    public void CheckInNest()
    {
        if (foodCarried == null) return;

        //Will check if ant is carrying food in nest - so food will be added to colony
        float queenDistance = Vector3.Distance(queen.transform.position, mouth.transform.position);
        if (queenDistance <= queenRange)
        {
            queenScript.food += Mathf.RoundToInt(foodCarried.GetComponent<Food>().food * foodMult);
            Destroy(foodCarried);
            foodCarried = null;

            if (queenScript.playerOnTeam == true)
            {
                AudioManager.instance.Play("FoodDropoff");
            }
        }
    }

    /// <summary>
    /// Returns whether the food target game object is still vaild to be picked up.
    /// </summary>
    public bool CheckValid(GameObject food)
    {
        if (food == null) return false;

        if (food.GetComponent<Food>().carried == true) return false;

        return true;
    }

    public void Death()
    {
        if (isDying) return;

        if (foodCarried != null)
        {
            FoodDrop();
        }
        if (queen != null)
        {
            if (Type == AntType.Soldier)
            {
                queenScript.totalSoldiers -= 1;
            }
            else if (Type == AntType.Worker)
            {
                queenScript.totalWorkers -= 1;
            }
        }

        UnitManager.Instance.UnregisterUnit(myInfo);

        isDying = true;
        Destroy(gameObject);
    }

    /// <summary>
    /// Checks and updates the fortress upgrade attack buff if necessary.
    /// </summary>
    public void UpdateFortressBuff()
    {
        if (Vector3.Distance(myInfo.transform.position, queen.transform.position) < 30f && !fortressBuffApplied)
        {
            fortressBuffApplied = true;
            attackDamage += 15;
        }
        else if (Vector3.Distance(myInfo.transform.position, queen.transform.position) >= 30f && fortressBuffApplied)
        {
            fortressBuffApplied = false;
            attackDamage -= 15;
        }
    }

    /// <summary>
    /// Allows for soldier to heal if it has first aid upgrade.
    /// </summary>
    public void FirstAid()
    {
        if (Type == AntType.Soldier)
        {
            if (healthScript.Health < healthScript.MaxHealth)
            {
                lastHealTime += Time.deltaTime;
                if (lastHealTime > 1f)
                {
                    lastHealTime -= 1f;
                    if (healthScript.Health + 2 > healthScript.MaxHealth)
                    {
                        healthScript.Health = healthScript.MaxHealth;
                    }
                    else
                    {
                        healthScript.Health += 2;
                    }
                }
            }
        }
    }

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
    }

    // ---- Upgrade Methods ----

    public void UpgradeAttack(float mult)
    {
        int current = attackDamage;

        // Only apply multiplier on top of base damage without fortress attack buff.
        if (fortressBuffApplied)
        {
            current -= 15;
            attackDamage = Mathf.RoundToInt(current * mult);
            attackDamage += 15;
        }
        else
        {
            attackDamage = Mathf.RoundToInt(current * mult);
        }
    }

    public void UpgradeSpeed(float mult)
    {
        agent.speed *= mult;
    }

    public void UpgradeFoodMult(float mult)
    {
        foodMult *= mult;
    }

    public void ActivateFirstAid()
    {
        firstAid = true;
    }
}
