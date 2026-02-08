using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEnemyAIOld : MonoBehaviour
{
    [Header("Type and Species")]
    public string type; //Capital e.g., Worker, Soldier
    public string species; //Capital e.g., Black, Fire

    [Header("Movement")]
    public Vector3 target = new Vector3(0, 0, 0); //This will store directly where the ant is moving towards
    public Vector3 mainTarget = new Vector3(0, 0, 0); //This will store the overall goal the ant is moving towards
    public float separationDistance = 1f;
    private bool tooClose = false;
    private float timeSinceTarget = 0f;
    private float timeBetweenMovement = 0.5f;
    public Animator anim;
    UnityEngine.AI.NavMeshAgent agent;

    [Header("Health Script")]
    public SHealth healthScript;

    [Header("Attacking")]
    public int attack = 0;
    private GameObject closestEnemy;
    public float sightRange = 20f;
    public float attackRange = 1f;
    public float attackDelay = 1f;
    private bool canAttack = true;
    public float attackTimer = 0f;

    [Header("Decision Making")]
    public bool chasingEnemy;
    public bool canChase = true; //Not to be confused with canAttack - determines whether CheckEnemies can be run
    public bool followingPlayer = false;
    private float distanceToMainTarget = 0f;
    public bool canFollowPlayer = true;

    [Header("Other")]
    public Transform mouth;
    public bool autonomous = true; //If false will only follow orders from player
    private GameObject[] Ants;

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
        transform.position = new Vector3(transform.position.x, transform.position.y, 0); //Will reset z position to 0
        Ants = GameObject.FindGameObjectsWithTag("Ant");
        attackTimer += Time.deltaTime;
        Brain();
        Movement();
    }

    void Brain()
    {
        chasingEnemy = false;

        timeBetweenMovement = 0f; //Enables instant reactions for pinpointing food and enemies
        
        if (canChase)
        {
            CheckEnemies();
        }

        if (chasingEnemy == false)
        {
            float rX = Random.Range(-10f, 10f);
            float rY = Random.Range(-10f, 10f);
            mainTarget = new Vector3(rX, rY, 0);
            timeBetweenMovement = 2f; //Increased so is not constantly moving around target
                                      //If not chasing enemy or moving towards food will move randomly
        }

    }

    void Movement()
    {
        //Will move towards main target
        //Will try to avoid other ants and will move randomly sometimes - so movement feels more realistic
        anim.SetBool("isWalking", false); //Set as false until walking takes place
        tooClose = false;
        timeSinceTarget += Time.deltaTime;
        Vector3 randomTarget = new Vector3(transform.position.x + Random.Range(-5f, 5f), transform.position.y + Random.Range(-5f, 5f), 0); //Will randomly generate new target
        foreach (GameObject a in Ants)
        {
            if (a != gameObject && a.GetComponent<SHealth>().team == healthScript.team)
            {
                //Ant is on same team
                float distance = Vector3.Distance(a.transform.position, this.transform.position);
                if (distance <= separationDistance && timeSinceTarget >= timeBetweenMovement)
                {
                    tooClose = true;
                    randomTarget = new Vector3(transform.position.x + Random.Range(-5f, 5f), transform.position.y + Random.Range(-5f, 5f), 0); //Will randomly generate new target
                    float newDist = Vector3.Distance(a.transform.position, randomTarget);
                    while (newDist <= separationDistance) //New target must be away from friendly ants
                    {
                        randomTarget = new Vector3(transform.position.x + Random.Range(-5f, 5f), transform.position.y + Random.Range(-5f, 5f), 0);
                        newDist = Vector3.Distance(a.transform.position, randomTarget);
                    }
                    target = randomTarget;
                    timeSinceTarget = 0f;
                }
            }
        }
        if (target != mainTarget && tooClose == false && timeSinceTarget >= timeBetweenMovement)
        {
            //If the ant is no longer too close to another, it will continue moving towards the goal.
            target = mainTarget;
            timeSinceTarget = 0f;
        }
        if (Vector3.Distance(target, transform.position) >= 1f && Vector3.Distance(mainTarget, transform.position) >= 1f)
        {
            //Will move if not at target
            //transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            agent.SetDestination(target);
            anim.SetBool("isWalking", true);
        }
        distanceToMainTarget = Vector3.Distance(this.transform.position, mainTarget);
    }

    void CheckEnemies()
    {
        float closestDistance = 0f;
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<SHealth>().team != healthScript.team)
            {
                float distance = Vector3.Distance(a.transform.position, mouth.transform.position);
                if (distance <= sightRange)
                {
                    //Enemy visible
                    if (closestDistance == 0 || distance < closestDistance)
                    {
                        //This enemy is the current closest enemy
                        closestDistance = distance;
                        closestEnemy = a;
                    }
                }
            }
        }
        if (closestDistance <= attackRange && closestDistance != 0)
        {
            if (canAttack == true)
            {
                chasingEnemy = true;
                if (attackTimer >= attackDelay && closestEnemy != null)
                {
                    attackTimer = 0f;
                    Attack();
                }
            }
        }
        else if (canAttack == true && closestEnemy != null)
        {
            mainTarget = closestEnemy.transform.position;
            chasingEnemy = true;
        }
    }

    void Attack()
    {
        closestEnemy.GetComponent<SHealth>().UpdateHealth(attack);
    }

    void FixedUpdate()
    {
        //Will rotate to look at target
        Vector3 vectorToTarget = target - transform.position;
        transform.rotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: vectorToTarget); //LookRotation points the object's local z+ axis towards the first argument (forward);
    }


    public void Death()
    {
        //Add code to update tutorial manager
        if(GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart == 4)
        {
            GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart = 5;
        }
        else if (GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart >= 14)
        {
            GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart += 1;
        }
        Destroy(gameObject);
    }
}
