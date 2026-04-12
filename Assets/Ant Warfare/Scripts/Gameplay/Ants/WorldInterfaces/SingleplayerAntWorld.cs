using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Ant.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SingleplayerAntWorld : MonoBehaviour, IAntWorld
{
    [SerializeField] private AntStateManager stateManager;
    [SerializeField] private AntContext context;

    private NavMeshAgent agent;
    private Animator anim;
    public SHealth healthScript;
    public Transform mouth;

    public GameObject queen;
    private BaseAntQueenAI queenScript;
    private GameObject player;
    public GameObject foodCarried;

    public float queenRange = 5f;
    public AntType Type;
    public string species;
    public int attackDamage;

    [SerializeField] private float sightRange = 20f;
    public float SightRange => sightRange;
    [SerializeField] private float attackRange = 2.5f;
    public float AttackRange => attackRange;
    [SerializeField] private float attackDelay = 1f;
    public float AttackDelay => attackDelay;
    [SerializeField] private float pickupRange = 2f;
    public float PickupRange => pickupRange;


    public Vector3 Position => transform.position;

    


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
    /// Sets references to queen, queenScript and player. 
    /// 
    /// After that initialises the state manager system 
    /// (which will automatically check for nearby pheromones).
    /// </summary>
    public void InitializeAntFromQueen(GameObject q)
    {
        queen = q;
        queenScript = q.GetComponent<BaseAntQueenAI>();
        player = queenScript.player;

        context = new AntContext(this);
        stateManager = new AntStateManager(context);

        stateManager.Initialize();
    }

    private void OnEnable()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.RegisterUnit(gameObject);
        }
    }

    private void OnDisable()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.UnregisterUnit(gameObject);
        }
    }

    public float DeltaTime => Time.deltaTime;

    /// <summary>
    /// Will be called from the player to make ant follow player.
    /// </summary>
    public void PlayerFollowStart(int subtype)
    {
        stateManager.StartPlayerIR(subtype);
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
        CheckQueenExists();
        CheckUpgradesOnUpdate();
        stateManager.Tick(Time.deltaTime);

        if (foodCarried != null)
        {
            foodCarried.transform.position = mouth.transform.position;
            foodCarried.transform.rotation = mouth.transform.rotation;
        }
    }

    private void CheckQueenExists()
    {
        if (queen != null) return;

        //Will kill themselves if no queen
        Death();
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

        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime);
    }

    public void SetDestination(Vector3 target)
    {
        if (!agent.enabled) return;

        agent.isStopped = false;
        agent.SetDestination(target);
    }

    public void StopMovement()
    {
        if (!agent.enabled) return;

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
            return transform.position;
        }
        for (int i=0; i<attempts; i++)
        {
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            float dist = Random.Range(minRange, maxRange);

            Vector3 candidate = origin + new Vector3(dir2D.x, dir2D.y, 0f) * dist;

            //Debug.Log($"Candidate {candidate}, valid {NavMesh.SamplePosition(candidate, out NavMeshHit h, snapDistance, agent.areaMask)}");

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, snapDistance, agent.areaMask)) continue;

            
            Vector3 flattened = new Vector3(hit.position.x, hit.position.y, 0f);
            //Debug.Log($"Flattened {flattened}");
            return flattened;
            //if (HasValidPath(flattened))
                //Debug.Log($"Valid path found for {candidate}");
                //return flattened;
        }
        Debug.LogWarning("No valid navmesh point found");
        return transform.position;
    }

    private bool HasValidPath(Vector3 target)
    {
        if (agent.isOnNavMesh) return false;

        NavMeshPath _path = new NavMeshPath();
        NavMesh.CalculatePath(agent.transform.position, target, NavMesh.AllAreas, _path);
        return _path.status == NavMeshPathStatus.PathComplete;
    }

    public GameObject FindFriendlyQueen()
    {
        if (queen != null)
        {
            return queen;
        }
        else
        {
            return GameObject.Find(species + "AntQueen" + gameObject.GetComponent<SHealth>().team.ToString());
        }
    }

    public float GetFriendlyQueenDist()
    {
        GameObject q = FindFriendlyQueen();
        return Vector3.Distance(q.transform.position, Position);
    }

    public GameObject FindFriendlyPlayer()
    {
        return player;
    }

    public GameObject FindClosestEnemy()
    {
        float closestDistance = -1f;
        GameObject closestEnemy = null;

        foreach (GameObject a in UnitManager.Instance.AllUnits)
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
        return closestEnemy;
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
        GameObject q = FindFriendlyQueen();
        int size = q.GetComponent<BaseAntQueenAI>().colonySize;

        if (size < 0) return 0;

        return size; 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3 target)>> GetPheromonesNearby()
    {
        Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3)>> pheromones 
            = new Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3)>>();


        List<GameObject> markers = queen.GetComponent<ColonyPheromonesManager>().markers;
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
            float weight = (20f-dist) * strength;
            Vector3 target = mData.target;


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

        List<GameObject> markers = queen.GetComponent<ColonyPheromonesManager>().markers;
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

    public void PlacePheromone(PheromoneSubtype subtype, Vector3 target)
    {
        //queen.GetComponent<ColonyPheromonesManager>().PlaceMarker(
        //    subtype, transform.position, transform.rotation, target);
        queen.GetComponent<ColonyPheromonesManager>().PlacePathMarker(subtype, transform.position, transform.rotation);
    }

    public void Attack(GameObject targetEnemy)
    {
        anim.SetBool("isAttacking", true);
        StartCoroutine(AttackDamage(targetEnemy));
    }

    IEnumerator AttackDamage(GameObject targetEnemy)
    {
        yield return new WaitForSeconds(0.25f);
        if (targetEnemy != null)
        {
            var health = targetEnemy.GetComponent<SHealth>();
            if (health != null) health.UpdateHealth(attackDamage);
        }
        yield return new WaitForSeconds(0.10f);
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
            queen.GetComponent<BaseAntQueenAI>().food += Mathf.RoundToInt(foodCarried.GetComponent<Food>().food * foodMult);
            Destroy(foodCarried);
            foodCarried = null;

            if (queen.GetComponent<BaseAntQueenAI>().playerOnTeam == true)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("FoodDropoff");
            }
        }
    }

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
        isDying = true;
        Destroy(gameObject);
    }

    /// <summary>
    /// Checks and updates the fortress upgrade attack buff if necessary.
    /// </summary>
    public void UpdateFortressBuff()
    {
        if (Vector3.Distance(transform.position, queen.transform.position) < 30f && !fortressBuffApplied)
        {
            fortressBuffApplied = true;
            attackDamage += 15;
        }
        else if (Vector3.Distance(transform.position, queen.transform.position) >= 30f && fortressBuffApplied)
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
}
