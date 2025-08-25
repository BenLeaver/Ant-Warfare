using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the player's ant movement, camera control, combat, and food collection mechanics in singleplayer mode.
/// </summary>
public class Player_Singleplayer : MonoBehaviour
{
    [Header("Player Data")]
    public string species;
    public bool inTutorial = false;

    [Header("Movement")]
    public float moveSpeed = 5.0f;
    public float clockwise = 100.0f;

    public SHealth healthScript;
    public Vector3 nestSpawn;
    public Animator anim;

    [Header("Camera")]
    public GameObject playerCameraPrefab;
    public Camera cam;

    [Header("Attack")]
    GameObject[] Ants;
    private GameObject closestEnemy;
    public Transform mouth;
    public float attackRange = 1f;
    public float sightRange = 50f;
    public int attack = 10;
    public float attackTimer = 0f;
    public float attackDelay = 1f;
    private bool canAttack = true;
    private GameObject enemyToAttack;
    private bool fortressBuffApplied = false;
    
    [Header("Food")]
    GameObject[] Food;
    public float pickupRange = 2f;
    public GameObject foodCarried;
    public float queenRange = 5f;
    public float foodMult = 1f;
    private GameObject queen;

    // Start is called before the first frame update
    void Start()
    {
        Ants = GameObject.FindGameObjectsWithTag("Ant");
        InitialiseCamera();
    }

    void InitialiseCamera()
    {
        var camera = Instantiate(playerCameraPrefab);
        camera.GetComponent<CameraFollow>().target = this.GetComponent<Transform>();
        cam = camera.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Ants = GameObject.FindGameObjectsWithTag("Ant");
        
        attackTimer += Time.deltaTime;
        CheckInput();
        CheckEnemies();
        if (foodCarried != null)
        {
            foodCarried.transform.position = mouth.transform.position;
            foodCarried.transform.rotation = mouth.transform.rotation;
            CheckInNest();
        }
        if(queen == null)
        {
            queen = GameObject.Find(species + "AntQueen" + gameObject.GetComponent<SHealth>().team.ToString());
        }
        if (queen.GetComponent<BaseAntQueenAI>().fortress)
        {
            UpdateFortressBuff();
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

    public void CheckInput()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            if(SceneManager.GetActiveScene().name == "Tutorial")
            {
                GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart = 0;
            }
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("GameMusic");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("MenuMusic");
            SceneManager.LoadScene("MainMenu");
        }
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += transform.up * Time.deltaTime * moveSpeed;
            anim.SetBool("isWalking", true);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position += transform.up * Time.deltaTime * -moveSpeed;
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
        if(Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, 0, Time.deltaTime * -clockwise);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, 0, Time.deltaTime * clockwise);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (foodCarried == null)
            {
                FoodPickup();
            }
            else
            {
                FoodDrop();
            }
        }
        if (Input.mouseScrollDelta.y == 1 && cam.orthographicSize > 10)
        {
            cam.orthographicSize -= 1;
        }
        else if (Input.mouseScrollDelta.y == -1 && cam.orthographicSize < 50)
        {
            cam.orthographicSize += 1;
        }
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
                    if (closestDistance == 0 || distance < closestDistance)
                    {
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
                if (attackTimer >= attackDelay && closestEnemy != null)
                {
                    attackTimer = 0f;
                    Attack();
                }
            }
        }
    }

    void Attack()
    {
        enemyToAttack = closestEnemy;
        StartCoroutine(AttackDamage());
    }

    /// <summary>
    /// Deals damage to the enemy after 0.15 seconds to match animations
    /// </summary>
    IEnumerator AttackDamage()
    {
        anim.SetBool("isAttacking", true);
        yield return new WaitForSeconds(0.25f);
        if (enemyToAttack != null)
        {
            enemyToAttack.GetComponent<SHealth>().UpdateHealth(attack);
        }
        yield return new WaitForSeconds(0.10f);
        anim.SetBool("isAttacking", false);
    }

    void FoodPickup()
    {
        Food = GameObject.FindGameObjectsWithTag("Food");
        float closestDistance = 0f;
        foreach(GameObject f in Food)
        {
            if(f.GetComponent<Food>().carried == false)
            {
                float distance = Vector3.Distance(f.transform.position, mouth.transform.position);
                if(distance <= pickupRange)
                {
                    if(closestDistance == 0 || distance < closestDistance)
                    {
                        closestDistance = distance;
                        foodCarried = f;
                    }
                }
            }
        }
        if(foodCarried != null)
        {
            foodCarried.GetComponent<Food>().carried = true;
            canAttack = false;
            gameObject.GetComponent<ObjectAudioManager>().Play("Pickup");
        }
    }

    void FoodDrop()
    {
        foodCarried.GetComponent<Food>().carried = false;
        foodCarried = null;
        canAttack = true;
        gameObject.GetComponent<ObjectAudioManager>().Play("Drop");
    }

    void CheckInNest()
    {
        float queenDistance = Vector3.Distance(queen.transform.position, mouth.transform.position);
        if(queenDistance <= queenRange)
        {
            queen.GetComponent<BaseAntQueenAI>().food += Mathf.RoundToInt(foodCarried.GetComponent<Food>().food * foodMult);
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("FoodDropoff");
            Destroy(foodCarried);
            foodCarried = null;
            canAttack = true;

            if(inTutorial)
            {
                if(GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart == 8)
                {
                    GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart = 9;
                }
            }
        }
    }

    public void Death()
    {
        if(queen.GetComponent<BaseAntQueenAI>().food >= 0)
        {
            //Respawns player
            transform.position = nestSpawn;
            queen.GetComponent<BaseAntQueenAI>().food -= 30;
            healthScript.health = healthScript.maxHealth;
        }
        else
        {
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("GameMusic");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("MenuMusic");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Lose");
            SceneManager.LoadScene("LoseMenu");
        }
    }
}
