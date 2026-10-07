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
    private UnitInfo closestEnemy;
    public Transform mouth;
    public float attackRange = 1f;
    public float sightRange = 50f;
    public int attack = 10;
    public float attackTimer = 0f;
    public float attackDelay = 1f;
    private bool canAttack = true;
    private UnitInfo enemyToAttack;
    private bool fortressBuffApplied = false;
    
    [Header("Food")]
    GameObject[] Food;
    public float pickupRange = 1f;
    public GameObject foodCarried;
    public float queenRange = 5f;
    public float foodMult = 1f;
    public GameObject queen;


    public UnitInfo myInfo;
    private UnitInfo queenInfo;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseCamera();
        queenInfo = queen.GetComponent<BaseAntQueenAI>().myInfo;
        if (queenInfo == null)
        {
            Debug.LogWarning("Player was unable to get queen unit info.");
        }
    }

    public void Initialise(int team, Vector3 nestSpawn, GameObject queenRef)
    {
        this.nestSpawn = nestSpawn;
        this.queen = queenRef;
        transform.position = nestSpawn;

        healthScript.UpdateTeam(team);

        myInfo = UnitManager.Instance.RegisterUnit(gameObject);
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
        attackTimer += Time.deltaTime;
        CheckInput();
        CheckEnemies();
        if (foodCarried != null)
        {
            foodCarried.transform.position = mouth.transform.position;
            foodCarried.transform.rotation = mouth.transform.rotation;
            CheckInNest();
        }
        if (queenInfo.queenScript.fortress)
        {
            UpdateFortressBuff();
        }
    }

    /// <summary>
    /// Checks and updates the fortress upgrade attack buff if necessary.
    /// </summary>
    public void UpdateFortressBuff()
    {
        if (Vector3.Distance(transform.position, queenInfo.transform.position) < 30f && !fortressBuffApplied)
        {
            fortressBuffApplied = true;
            attack += 15;
        }
        else if (Vector3.Distance(transform.position, queenInfo.transform.position) >= 30f && fortressBuffApplied)
        {
            fortressBuffApplied = false;
            attack -= 15;
        }
    }

    public void CheckInput()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            if (UnitManager.Instance != null)
                UnitManager.Instance.ClearAllUnits();

            if(SceneManager.GetActiveScene().name == "Tutorial")
            {
                GameObject.Find("SGameManager").GetComponent<SGameManager>().tutorialPart = 0;
            }
            AudioManager.instance.Stop("GameMusic");
            AudioManager.instance.Play("MenuMusic");

            SceneManager.LoadScene("MainMenu");
        }
        if(Input.GetKey(KeyCode.W))
        {
            myInfo.transform.position += myInfo.transform.up * Time.deltaTime * moveSpeed;
            anim.SetBool("isWalking", true);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            myInfo.transform.position += myInfo.transform.up * Time.deltaTime * -moveSpeed;
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
        if(Input.GetKey(KeyCode.D))
        {
            myInfo.transform.Rotate(0, 0, Time.deltaTime * -clockwise);
        }
        if (Input.GetKey(KeyCode.A))
        {
            myInfo.transform.Rotate(0, 0, Time.deltaTime * clockwise);
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
        if (Input.mouseScrollDelta.y == 1 && cam.orthographicSize > 5)
        {
            cam.orthographicSize -= 1;
        }
        else if (Input.mouseScrollDelta.y == -1 && cam.orthographicSize < 100)
        {
            cam.orthographicSize += 1;
        }
    }

    void CheckEnemies()
    {
        // Check whether the player is allowed to attack.
        if (!canAttack) return;
        if (attackTimer < attackDelay) return;

        // Get closest enemy unit in attack range (if any).
        closestEnemy = UnitManager.Instance.GetClosestEnemyUnit(mouth.transform.position, myInfo.team, attackRange);
        if (!closestEnemy.IsAliveAndActive()) return;

        // Attack
        attackTimer = 0f;
        Attack();
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
        var attacker = gameObject;

        anim.SetBool("isAttacking", true);
        yield return new WaitForSeconds(0.25f);

        if (attacker == null) yield break;
        if (!attacker.activeInHierarchy) yield break;

        if (!enemyToAttack.IsAliveAndActive())
        {
            anim.SetBool("isAttacking", false);
            yield break;
        }

        enemyToAttack.health.UpdateHealth(attack);

        yield return new WaitForSeconds(0.10f);

        if (attacker == null) yield break;
        if (!attacker.activeInHierarchy) yield break;

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
        float queenDistance = Vector3.Distance(queenInfo.transform.position, mouth.transform.position);
        if(queenDistance <= queenRange)
        {
            queenInfo.queenScript.food += Mathf.RoundToInt(foodCarried.GetComponent<Food>().food * foodMult);
            AudioManager.instance.Play("FoodDropoff");
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
        if(queenInfo.queenScript.food >= 0)
        {
            //Respawns player
            if (foodCarried != null) FoodDrop();
            myInfo.transform.position = nestSpawn;
            queenInfo.queenScript.food -= 30;
            healthScript.ResetHealth();
        }
        else
        {
            if (UnitManager.Instance != null)
                UnitManager.Instance.ClearAllUnits();
            AudioManager.instance.Stop("GameMusic");
            AudioManager.instance.Play("MenuMusic");
            AudioManager.instance.Play("Lose");
            SceneManager.LoadScene("LoseMenu");
        }
    }

    public void UpgradeAttack(float mult)
    {
        int current = attack;

        // Only apply multiplier on top of base damage without fortress attack buff.
        if (fortressBuffApplied)
        {
            current -= 15;
            attack = Mathf.RoundToInt(current * mult);
            attack += 15;
        }
        else
        {
            attack = Mathf.RoundToInt(current * mult);
        }
    }
}
