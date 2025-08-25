using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Controls the behavior of tutorial ants in the game.
/// Handles movement, attacking enemies, and tutorial-specific death behavior.
/// </summary>
public class AntTutorialAI : MonoBehaviour
{
    [Header("Type and Species")]
    public string type; //e.g., Worker, Soldier
    public string species; //e.g., Black, Fire

    [Header("Movement")]
    public Vector3 target = new Vector3(0, 0, 0);
    public Animator anim;
    UnityEngine.AI.NavMeshAgent agent;

    [Header("Health Script")]
    public SHealth healthScript;

    [Header("Attacking")]
    public int attack = 0;
    private GameObject closestEnemy;
    public const float ATTACK_RANGE = 2.5f;
    public const float ATTACK_DELAY = 1f;
    public float attackTimer = 5f;
    private GameObject enemyToAttack;
    

    [Header("Decision Making")]
    private float lastMovement;
    public const float MOVEMENT_DELAY = 0.1f;
    public const float RANDOM_POS_DELAY = 4f;

    [Header("Other")]
    public Transform mouth;
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
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        Ants = GameObject.FindGameObjectsWithTag("Ant");
        attackTimer += Time.deltaTime;
        FindClosestEnemy();
        MovementDecision();
        MoveToTarget();
    }

    /// <summary>
    /// Will update the movement target.
    /// </summary>
    void MovementDecision()
    {

        float closestEnemyDist = 9999;
        if (closestEnemy != null)
        {
            closestEnemyDist = Vector3.Distance(closestEnemy.transform.position, transform.position);
            MoveTowardsEnemy();
        }
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
    /// Movement target will be set to the nearest enemy ant. If the enemy is within attackRange,
    /// and this ant has waited long enough since the last attack, the enemy will be attacked.
    /// </summary>
    void MoveTowardsEnemy()
    {
        target = closestEnemy.transform.position;
        float closestDistance = Vector3.Distance(closestEnemy.transform.position, transform.position);
        if (closestDistance <= ATTACK_RANGE)
        {
            // If the enemy is within attack range, stand still instead of pushing into the enemy.
            target = transform.position;
            // Update rotation to face towards enemy ant is attacking.
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
    /// Handles death for tutorial ants, moving on to the next part of the tutorial.
    /// </summary>
    public void Death()
    {
        if (GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart == 4)
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
