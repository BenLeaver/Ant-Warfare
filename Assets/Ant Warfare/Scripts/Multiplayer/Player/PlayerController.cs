using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;

/// <summary>
/// Handles player movement, attacks, food interactions, and camera management. 
/// Also manages player death, spectating, and game-end behavior.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [Header("Player Info")]
    public NetworkVariable<int> playerIndex = new NetworkVariable<int>();
    public int playerTeam = 0;
    public bool playerLost = false;
    public Transform mouth;

    public Animator anim;

    [SerializeField] private TextMeshProUGUI usernameText;
    public Vector3 nestSpawn;
    [SerializeField] private string species;


    public NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>
        (default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private GameObject projectSceneManager;
    private GameObject[] Ants;
    private bool inGame = false;
    private bool spectating = false;

    [Header("Camera")]
    public GameObject playerCameraPrefab;
    [SerializeField] private GameObject playerCamera;

    [Header("UI")]
    public GameObject UIPrefab;
    [SerializeField] private GameObject playerUI;
    public GameObject SpectatorUIPrefab;
    private bool disableSpectatorUI = false;


    [Header("Health")]
    [SerializeField] private GameObject healthBar;
    public MHealth healthScript;

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] private float clockwise = 100f;
    [SerializeField] private float spectatorMoveSpeed = 20f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] public int attack = 10;
    [SerializeField] private float attackDelay = 1f;
    private GameObject closestEnemy;
    private float attackTimer = 0f;
    private bool canAttack = true;
    private GameObject enemyToAttack;

    [Header("Food")]
    private GameObject[] Food;
    public float pickupRange = 2f;
    public GameObject foodCarried;
    private GameObject queen;
    public float queenRange = 5f;
    public float foodMult = 1f;

    /// <summary>
    /// Called when the player spawns on the network. Sets up player index (on server), username synchronization, 
    /// and adds the player to the ProjectSceneManager's player list.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        DontDestroyOnLoad(this);

        projectSceneManager = GameObject.Find("ProjectSceneManager");
        if(IsServer)
        {
            playerIndex.Value = NetworkManager.ConnectedClients.Count - 1;
        }
        projectSceneManager.GetComponent<ProjectSceneManager>().AddPlayer(gameObject, playerIndex.Value, species);

        if (IsOwner) return;
        username.OnValueChanged += UpdateUsername;
        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;

    }

    // Update is called once per frame
    void Update()
    {
        if(spectating)
        {
            SpectatorUpdate();
        }
        else
        {
            WriteUsername();
            if (inGame)
            {
                Ants = GameObject.FindGameObjectsWithTag("Ant");
                attackTimer += Time.deltaTime;
                CheckEnemies();

                if (foodCarried != null)
                {
                    if(IsOwner)
                    {
                        foodCarried.transform.position = mouth.transform.position;
                        foodCarried.transform.rotation = mouth.transform.rotation;
                    }
                    CheckInNest();
                }
                if (queen == null)
                {
                    if (GameObject.Find("M" + species + "AntQueen" + playerTeam.ToString()))
                    {
                        queen = GameObject.Find("M" + species + "AntQueen" + playerTeam.ToString());
                        
                        queen.GetComponent<MBaseAntQueenAI>().m_player = gameObject;
                        if (IsServer)
                        {
                            nestSpawn = queen.GetComponent<MBaseAntQueenAI>().spawnPos;
                            SyncNestSpawnRpc(nestSpawn);
                        }
                    }
                }
            }
            CheckInput();
        }
    }

    /// <summary>
    /// Handles movement of player in spectator mode.
    /// </summary>
    private void SpectatorUpdate()
    {
        if(IsOwner)
        {
            if (disableSpectatorUI)
            {
                if (playerUI.GetComponent<MSpectatorUI>())
                {
                    playerUI.GetComponent<MSpectatorUI>().DestroySpectatorText();
                    disableSpectatorUI = false;
                    spectating = false;
                }
            }
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += new Vector3(0, spectatorMoveSpeed * Time.deltaTime, 0);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += new Vector3(0, spectatorMoveSpeed * -Time.deltaTime, 0);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += new Vector3(spectatorMoveSpeed * -Time.deltaTime, 0, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += new Vector3(spectatorMoveSpeed * Time.deltaTime, 0, 0);
            }
            if (playerCamera != null)
            {
                Camera cam = playerCamera.GetComponent<Camera>();
                if (Input.mouseScrollDelta.y == 1 && cam.orthographicSize > 10)
                {
                    cam.orthographicSize -= 1;
                }
                else if (Input.mouseScrollDelta.y == -1 && cam.orthographicSize < 50)
                {
                    cam.orthographicSize += 1;
                }
            }
            else
            {
                Debug.Log("playerCamera is null");
            }

        }
    }

    /// <summary>
    /// Will check if ant is carrying food in nest - so food will be added to the colony.
    /// </summary>
    private void CheckInNest()
    {
        float queenDistance = Vector3.Distance(queen.transform.position, mouth.transform.position);
        if (queenDistance <= queenRange)
        {
            if (IsOwner)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("FoodDropoff");
                DepositFoodServerRpc(foodCarried.GetComponent<NetworkObject>());
                foodCarried = null;
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void DepositFoodServerRpc(NetworkObjectReference foodRef)
    {
        var food = ((GameObject)foodRef);
        UpdateFoodRpc(food.GetComponent<Food>().food);
        Destroy(food);
        ResetFoodRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateFoodRpc(int foodValue)
    {
        queen.GetComponent<MBaseAntQueenAI>().food += Mathf.RoundToInt(foodValue * foodMult);
    }

    [Rpc(SendTo.Everyone)]
    public void ResetFoodRpc()
    {
        foodCarried = null;
        canAttack = true;
    }

    void FoodPickup()
    {
        Food = GameObject.FindGameObjectsWithTag("Food");
        float closestDistance = 0f;
        foreach (GameObject f in Food)
        {
            if (f.GetComponent<Food>().carried == false)
            {
                float distance = Vector3.Distance(f.transform.position, mouth.transform.position);
                if (distance <= pickupRange)
                {
                    if (closestDistance == 0 || distance < closestDistance)
                    {
                        closestDistance = distance;
                        foodCarried = f;
                    }
                }
            }
        }
        if (foodCarried != null)
        {
            UpdateFoodCarriedRpc(foodCarried.GetComponent<NetworkObject>(), false);
            PlayObjectAudioRpc("Pickup");
            if (IsOwner)
            {
                ChangeFoodOwnerServerRpc(foodCarried.GetComponent<NetworkObject>(), 
                    NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    void PlayObjectAudioRpc(string sound)
    {
        gameObject.GetComponent<ObjectAudioManager>().Play(sound);
    }

    [Rpc(SendTo.Everyone)]
    void UpdateFoodCarriedRpc(NetworkObjectReference foodRef, bool drop)
    {
        var food = ((GameObject)foodRef);
        if (drop)
        {
            canAttack = true;
            food.GetComponent<Food>().carried = false;
        }
        else
        {
            canAttack = false;
            food.GetComponent<Food>().carried = true;
        }
    }

    [ServerRpc]
    void ChangeFoodOwnerServerRpc(NetworkObjectReference foodRef, ulong clientID, bool giveServer=false)
    {
        var food = ((GameObject)foodRef);
        if (giveServer)
        {
            food.GetComponent<NetworkObject>().RemoveOwnership();
        }
        else
        {
            food.GetComponent<NetworkObject>().ChangeOwnership(clientID);
        }
    }

    void FoodDrop()
    {
        if (IsOwner)
        {
            //Will return ownership of food to server
            ChangeFoodOwnerServerRpc(foodCarried.GetComponent<NetworkObject>(),
    NetworkManager.Singleton.LocalClientId, true);
            PlayObjectAudioRpc("Drop");
        }
        UpdateFoodCarriedRpc(foodCarried.GetComponent<NetworkObject>(), true);
        foodCarried = null;
    }

    private void CheckEnemies()
    {
        float closestDistance = 0f;
        foreach (GameObject a in Ants)
        {
            if (a.GetComponent<MHealth>().team != playerTeam)
            {
                float distance = Vector3.Distance(a.transform.position, mouth.transform.position);
                if (closestDistance == 0 || distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = a;
                }
            }
        }
        
        if (closestDistance <= attackRange && closestDistance != 0)
        {
            if (canAttack)
            {
                if(attackTimer >= attackDelay && closestEnemy != null)
                {
                    attackTimer = 0f;
                    Attack();
                }
            }
        }
    }

    private void Attack()
    {
        enemyToAttack = closestEnemy;
        StartCoroutine(AttackDamage());
    }

    IEnumerator AttackDamage()
    {
        anim.SetBool("isAttacking", true);
        yield return new WaitForSeconds(0.25f);
        if (enemyToAttack != null)
        {
            enemyToAttack.GetComponent<MHealth>().UpdateHealth(attack);
        }
        yield return new WaitForSeconds(0.10f);
        anim.SetBool("isAttacking", false);
    }

    [Rpc(SendTo.Everyone)]
    public void StartAnimationRpc(string animationBoolName)
    {
        anim.SetBool(animationBoolName, true);
    }

    [Rpc(SendTo.Everyone)]
    public void StopAnimationRpc(string animationBoolName)
    {
        anim.SetBool(animationBoolName, false);
    }

    private void CheckInput()
    {
        bool isWalking = false;

        if (IsOwner)
        {
            
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.up * Time.deltaTime * moveSpeed;
                isWalking = true;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += transform.up * Time.deltaTime * -moveSpeed;
                isWalking = true;
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(0, 0, Time.deltaTime * -clockwise);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(0, 0, Time.deltaTime * clockwise);
            }

            if (isWalking)
            {
                anim.SetBool("isWalking", true);
            }
            else
            {
                anim.SetBool("isWalking", false);
            }
            // FOR TESTING ONLY - DELETE LATER!!
            // Pressing K makes the ant take damage.
            if (Input.GetKey(KeyCode.K))
            {
                KillSelfRpc();
            }

        }

        if(inGame)
        {
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
            if (IsOwner)
            {
                Camera cam = playerCamera.GetComponent<Camera>();
                if (Input.mouseScrollDelta.y == 1 && cam.orthographicSize > 10)
                {
                    cam.orthographicSize -= 1;
                }
                else if (Input.mouseScrollDelta.y == -1 && cam.orthographicSize < 50)
                {
                    cam.orthographicSize += 1;
                }
            }
        }
    }

    // FOR TESTING ONLY - DELETE LATER!!
    [Rpc(SendTo.Everyone)]
    void KillSelfRpc()
    {
        healthScript.UpdateHealth(1);
    }

    private void WriteUsername()
    {
        if (!IsOwner) return;
        if (username.Value != "") return;

        username.Value = GameObject.Find("ProjectSceneManager").GetComponent<
            ProjectSceneManager>().GetUsername();
        usernameText.text = username.Value.ToString();
    }

    private void UpdateUsername(FixedString32Bytes previous, FixedString32Bytes current)
    {
        usernameText.text = current.ToString();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        usernameText.text = username.Value.ToString();
    }

    /// <summary>
    /// Called when the game is started to activate the player.
    /// </summary>
    /// <param name="rTeam">The random team index (1-4) given to the player</param>
    public void ActivatePlayer(int rTeam)
    {
        playerTeam = rTeam;
        if (IsOwner)
        {
            InstantiateCamera();
            playerUI = Instantiate(UIPrefab);
            playerUI.GetComponent<Multiplayer_UI>().SetQueen(playerTeam, species);
            playerUI.GetComponent<Multiplayer_UI>().UpdatePlayerMarkerTexts(gameObject);
        }
        healthBar.SetActive(true);
        GetComponent<MHealth>().UpdateTeam(playerTeam);
        inGame = true;
        playerLost = false;
    }

    /// <summary>
    /// Instantiates player camera and updates it's target to this player.
    /// </summary>
    public void InstantiateCamera()
    {
        playerCamera = Instantiate(playerCameraPrefab);
        playerCamera.GetComponent<CameraFollow>().target = this.GetComponent<Transform>();
    }

    /// <summary>
    /// Handles player death and transformation into spectator mode.
    /// </summary>
    public void Death()
    {
        Debug.Log("Death()");
        if (queen == null)
        {
            ColonyDied();
        }
        else if(queen.GetComponent<MBaseAntQueenAI>().food >= 0)
        {
            if (foodCarried != null)
            {
                FoodDrop();
            }
            queen.GetComponent<MBaseAntQueenAI>().food -= 30;
            healthScript.SetHealthToMax();
            transform.position = nestSpawn;
        }
        else
        {
            ColonyDied();
        }
    }

    /// <summary>
    /// The nestSpawn is synced from the server. This is because nestSpawn is only guaranteed 
    /// to be correct for server at first (the host player chooses the map). 
    /// </summary>
    [Rpc(SendTo.Everyone)]
    public void SyncNestSpawnRpc(Vector3 correctNestSpawn)
    {
        nestSpawn = correctNestSpawn;
    }

    /// <summary>
    /// Handles game end sequence when the scene is changed to MEndScreen.
    /// </summary>
    /// <param name="winningTeam">Index of the team which has won.</param>
    /// <param name="winningUsername">Username of the player who has won.</param>
    public void GameEnd(int winningTeam, string winningUsername)
    {
        if(!IsOwner)
        {
            return;
        }
        inGame = false;
        if(playerUI.GetComponent<MSpectatorUI>())
        {
            playerUI.GetComponent<MSpectatorUI>().DestroySpectatorText();
            spectating = false;
        }
        else
        {
            //This branch occurs when client dies after/at the same time as GameEnd().
            //Therefore spectator UI is being instantiated on same frame as when it is trying to be accessed.
            //So text will be disabled the frame after in SpectatorUpdate().
            disableSpectatorUI = true;
        }
        playerCamera.GetComponent<Camera>().enabled = false;
        playerCamera.GetComponent<AudioListener>().enabled = false;
        GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("GameMusic");
        GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("MenuMusic");
        if(playerLost)
        {
            Debug.Log("Player Lost");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Lose");
        }
        else
        {
            Debug.Log("Player Won");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Win");
            StartSpectator();
            DeleteTeamServerRPC(playerTeam);
        }
        GameObject.Find("EndCanvas").GetComponent<MEndScreenUI>().UpdateWinner(winningTeam, winningUsername);
    }

    public void ColonyDied()
    {
        Debug.Log("Colony Died!");
        playerLost = true;
        StartSpectator();
        DeleteTeamServerRPC(playerTeam);
        projectSceneManager.GetComponent<ProjectSceneManager>().PlayerDied();

    }

    void StartSpectator()
    {
        spectating = true;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
        usernameText.text = "";
        Destroy(healthBar);
        
        gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        gameObject.tag = "Untagged";
        if (IsOwner)
        {
            Destroy(playerUI);
            playerUI = Instantiate(SpectatorUIPrefab);
        }
    }

    /// <summary>
    /// Deletes all ants remaining on a team.
    /// Must run after StartSpectator() so tag of player is changed and the player on this team 
    /// will be turned into a spectator rather than being deleted.
    /// </summary>
    /// <param name="team">The team where all ants will be destroyed.</param>
    [ServerRpc(RequireOwnership = false)]
    public void DeleteTeamServerRPC(int team)
    {
        Debug.Log($"DeleteTeamServerRPC() called for team {team}");
        if(!IsServer)
        {
            return;
        }
        Ants = GameObject.FindGameObjectsWithTag("Ant");
        foreach (GameObject a in Ants)
        {
            if(a.GetComponent<MHealth>().team == team)
            {
                Destroy(a);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void playFoodDropoffRpc()
    {
        if (IsOwner)
        {
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("FoodDropoff");
        }
    }

    [Rpc(SendTo.Everyone)]
    public void playSpawnRpc()
    {
        if (IsOwner)
        {
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Spawn");
        }
    }
}
