using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager for handling spawning of teams, map selection, UI, and tutorial.
/// Implements singleton pattern.
/// </summary>
public class SGameManager : MonoBehaviour
{
    public static SGameManager instance;


    [Header("Queens")]
    public GameObject blackAntQueenPrefab;
    public GameObject fireAntQueenPrefab;
    public Transform[] queenSpawns = new Transform[4];
    
    [Header("Game Settings")]
    public int enemy1;
    public int enemy2;
    public int enemy3;
    public int numberTeams;


    //Dropdown values
    //0 = Easy
    //1 = Medium
    //2 = Hard
    //3 = None

    public TMPro.TMP_Dropdown enemy1Dropdown;
    public TMPro.TMP_Dropdown enemy2Dropdown;
    public TMPro.TMP_Dropdown enemy3Dropdown;
    private TMPro.TMP_Dropdown mapDropdown;

    //Species Dropdown values
    //0 = Black Ant
    //1 = Fire Ant
    private TMPro.TMP_Dropdown playerSpeciesDropdown;
    private TMPro.TMP_Dropdown enemy1SpeciesDropdown;
    private TMPro.TMP_Dropdown enemy2SpeciesDropdown;
    private TMPro.TMP_Dropdown enemy3SpeciesDropdown;
    public string[] dropdownSpecies = new string[] { "Black", "Fire" };

    public string gameScene;
    

    [Header("Player")]
    public int[] playerTeams = new int[] { 0, 0, 0, 0}; //Index 0 will be player, other elements will be for each enemy colony
    public string[] playerSpecies = new string[] { "Black", "Black", "Black", "Black" };

    
    public GameObject player;
    public GameObject playerBlackPrefab;
    public GameObject playerFirePrefab;

    public Vector3[] nestSpawns = new Vector3[4];
    
    [Header("Game Start")]
    private bool gameStart = false;
    private bool reset = false;
    private GameObject queen; //Used temporarily to spawn in queens.

    [Header("Game UI")]
    public GameObject BlackUI;
    public GameObject FireUI;
    private GameObject tutorialUI; //Singleplayer UI only used in tutorial

    [Header("Tutorial")]
    public int tutorialPart = 0; //Starts at 0 and incremented each time new part reached
    private TMP_Text tutorialText;
    public GameObject tutorialEnemyPrefab;


    void Awake ()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Loading")
        {
            SceneManager.LoadScene("MainMenu");
        }
        if (gameStart == true)
        {
            SpawnTeams();
            gameStart = false;
            reset = false; //Ready for next time game is reset
        }
        if (SceneManager.GetActiveScene().name == "SGameSettings" && reset == false)
        {
            enemy1Dropdown = GameObject.Find("EnemyDropdown1").GetComponent<TMPro.TMP_Dropdown>();
            enemy2Dropdown = GameObject.Find("EnemyDropdown2").GetComponent<TMPro.TMP_Dropdown>();
            enemy3Dropdown = GameObject.Find("EnemyDropdown3").GetComponent<TMPro.TMP_Dropdown>();

            playerSpeciesDropdown = GameObject.Find("PlayerSpeciesDropdown").GetComponent<TMPro.TMP_Dropdown>();
            enemy1SpeciesDropdown = GameObject.Find("ESpeciesDropdown1").GetComponent<TMPro.TMP_Dropdown>();
            enemy2SpeciesDropdown = GameObject.Find("ESpeciesDropdown2").GetComponent<TMPro.TMP_Dropdown>();
            enemy3SpeciesDropdown = GameObject.Find("ESpeciesDropdown3").GetComponent<TMPro.TMP_Dropdown>();
            reset = true;
        }
        if(SceneManager.GetActiveScene().name == "Tutorial")
        {
            //Will run through different parts of the tutorial
            
            if(tutorialPart == 0)
            {
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("GameMusic");
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("MenuMusic");
                StartCoroutine(TutorialStart());
            }
            else if (tutorialPart == 1)
            {
                tutorialText.text = "Use WASD to move";
                if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                {
                    tutorialPart = 2;
                }
            }
            else if (tutorialPart == 2)
            {
                tutorialText.text = "Use mouse wheel to zoom in and out";
                if(Input.mouseScrollDelta.y == 1 || Input.mouseScrollDelta.y == -1)
                {
                    tutorialPart = 3;
                }
            }
            else if (tutorialPart == 3)
            {
                tutorialText.text = "Fight the enemy ant - you will attack automatically when an enemy is near your mouth";
                Instantiate(tutorialEnemyPrefab, new Vector3(20, 0, 0), Quaternion.identity);
                tutorialPart = 4;
            }
            else if (tutorialPart == 5)
            {
                tutorialUI = Instantiate(BlackUI);
                player = GameObject.Find("Player_Singleplayer(Black)");
                player.GetComponent<Player_Singleplayer>().inTutorial = true;
                tutorialPart = 6;
            }
            else if (tutorialPart == 6)
            {
                tutorialText.text = "Pick up food by pressing E when near a food pellet";
                if(player.GetComponent<Player_Singleplayer>().foodCarried != null)
                {
                    tutorialPart = 7;
                }
            }
            else if (tutorialPart == 7)
            {
                tutorialText.text = "Drop food by pressing E again";
                if (player.GetComponent<Player_Singleplayer>().foodCarried == null)
                {
                    tutorialPart = 8;
                }
            }
            else if (tutorialPart == 8)
            {
                tutorialText.text = "Pick up the food again and carry it towards your queen in the nest";
            }
            else if (tutorialPart == 9)
            {
                tutorialText.text = "Buy workers by pressing the button in the top left";
                tutorialUI.GetComponent<Singleplayer_UI>().inTutorial = true;
            }
            else if (tutorialPart == 10)
            {
                tutorialText.text = "Press 1 to command your ants to follow you";
                if(Input.GetKeyDown(KeyCode.Alpha1))
                {
                    tutorialPart = 11;
                }
            }
            else if (tutorialPart == 11)
            {
                tutorialText.text = "Press 0 to allow your ants to act on their own and gather food or attack enemies";
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    tutorialPart = 12;
                }
            }
            else if (tutorialPart == 12)
            {
                tutorialText.text = "Press 2 to command your ants to retreat to the nest";
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    tutorialPart = 13;
                }
            }
            else if (tutorialPart == 13)
            {
                tutorialText.text = "Press 4 to command your ants to gather food";
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    tutorialPart = 14;
                }
            }
            else if (tutorialPart == 14)
            {
                tutorialText.text = "Press 3 to command your ants to attack enemies";
                Instantiate(tutorialEnemyPrefab, new Vector3(60, 0, 0), Quaternion.identity);
                Instantiate(tutorialEnemyPrefab, new Vector3(60, 0, 0), Quaternion.identity);
                Instantiate(tutorialEnemyPrefab, new Vector3(60, 0, 0), Quaternion.identity);
                tutorialPart = 15;
            }
            else if (tutorialPart == 18)
            {
                tutorialText.text = "Press U or the upgrade button to open the upgrades menu. You can buy 1 upgrade for each tier.";
                if (Input.GetKeyDown(KeyCode.U) || tutorialUI.GetComponent<Singleplayer_UI>().isUpgradeUIActive())
                {
                    tutorialPart = 19;
                }
            }
            else if (tutorialPart >= 19)
            {
                //Tutorial finished
                if(tutorialPart == 19)
                {
                    GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Win");
                    tutorialPart = 20;
                }
                tutorialText.text = "Tutorial Complete! Press Esc to leave.";
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    tutorialPart = 0;
                    GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("MenuMusic");
                    GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("GameMusic");
                    SceneManager.LoadScene("MainMenu");
                }
            }
            
        }
    }

    public void LoadGame()
    {
        GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("MenuMusic");
        GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("GameMusic");

        enemy1 = enemy1Dropdown.value;
        enemy2 = enemy2Dropdown.value;
        enemy3 = enemy3Dropdown.value;

        UpdateSpecies();

        FindMap();
        SceneManager.LoadScene(gameScene);

        

        StartCoroutine(SpawnDelay());

    }

    /// <summary>
    /// Updates playerSpecies[] using values from dropdowns.
    /// </summary>
    public void UpdateSpecies()
    {
        playerSpecies[0] = dropdownSpecies[playerSpeciesDropdown.value];
        playerSpecies[1] = dropdownSpecies[enemy1SpeciesDropdown.value];
        playerSpecies[2] = dropdownSpecies[enemy2SpeciesDropdown.value];
        playerSpecies[3] = dropdownSpecies[enemy3SpeciesDropdown.value];
    }

    IEnumerator SpawnDelay()
    {
        //Queen game objects are spawning in on the same frame as they are trying to be found, so a delay of one frame must be used
        yield return null;

        UpdateNestSpawns();

        CalculateNumberOfTeams();

        GenerateTeams();

        CreateUI();

        gameStart = true;
    }

    void CalculateNumberOfTeams()
    {
        numberTeams = 4;
        if (enemy2 == 3)
        {
            numberTeams -= 1;
        }
        if (enemy3 == 3)
        {
            numberTeams -= 1;
        }
    }

    /// <summary>
    /// Generates a random team index for each of the players.
    /// </summary>
    void GenerateTeams()
    {
        for (int i = 0; i < playerTeams.Length; i++)
        {
            playerTeams[i] = 0;
        }
        for (int i = 0; i < numberTeams; i++)
        {
            bool uniqueTeam = false;
            int rTeam = 1;
            while (uniqueTeam == false)
            {
                rTeam = Random.Range(1, 5); //Generates random team 1 to 4
                uniqueTeam = true;
                foreach (int n in playerTeams) //Loops through all assigned teams
                {
                    //Checks random team hasn't yet been assigned
                    if (rTeam == n)
                    {
                        uniqueTeam = false;
                    }
                }
            }
            playerTeams[i] = rTeam;
        }
    }

    void SpawnTeams()
    {
        foreach (int team in playerTeams)
        {
            if (team != 0)
            {
                if (team == playerTeams[0])
                {
                    //This is the team of the real player
                    if(playerSpecies[0] == "Black")
                    {
                        player = Instantiate(playerBlackPrefab);
                    }
                    else if(playerSpecies[0] == "Fire")
                    {
                        player = Instantiate(playerFirePrefab);
                    }
                    player.transform.position = nestSpawns[team - 1];
                    player.GetComponent<Player_Singleplayer>().nestSpawn = nestSpawns[team - 1];
                    player.GetComponent<SHealth>().UpdateTeam(team);

                    //Also need to delete pre-existing camera
                    Destroy(GameObject.Find("LoadingCamera"));

                    if(playerSpecies[0] == "Black")
                    {
                        queen = Instantiate(blackAntQueenPrefab, queenSpawns[team - 1].position, queenSpawns[team - 1].rotation); //Instantiate queen with correct transform
                    }
                    else if(playerSpecies[0] == "Fire")
                    {
                        queen = Instantiate(fireAntQueenPrefab, queenSpawns[team - 1].position, queenSpawns[team - 1].rotation);
                    }
                    queen.name = playerSpecies[0] + "AntQueen" + team;//Rename queen so it can be correctly accessed
                    queen.GetComponent<BaseAntQueenAI>().playerOnTeam = true; //Queen will know it is on player's team
                    queen.GetComponent<BaseAntQueenAI>().player = player;
                    queen.GetComponent<SHealth>().UpdateTeam(team);
                    queen.GetComponent<BaseAntQueenAI>().spawnPos = nestSpawns[team - 1];
                }
                else if(team == playerTeams[1])
                {
                    if (playerSpecies[1] == "Black")
                    {
                        queen = Instantiate(blackAntQueenPrefab, queenSpawns[team - 1].position, queenSpawns[team - 1].rotation);
                    }
                    else if (playerSpecies[1] == "Fire")
                    {
                        queen = Instantiate(fireAntQueenPrefab, queenSpawns[team - 1].position, queenSpawns[team - 1].rotation);
                    }
                    queen.name = playerSpecies[1] + "AntQueen" + team;
                    queen.GetComponent<BaseAntQueenAI>().difficulty = enemy1;
                    queen.GetComponent<SHealth>().UpdateTeam(team);
                    queen.GetComponent<BaseAntQueenAI>().spawnPos = nestSpawns[team - 1];
                }
                else if (team == playerTeams[2])
                {
                    if (playerSpecies[2] == "Black")
                    {
                        queen = Instantiate(blackAntQueenPrefab, queenSpawns[team - 1].position, queenSpawns[team - 1].rotation);
                    }
                    else if (playerSpecies[2] == "Fire")
                    {
                        queen = Instantiate(fireAntQueenPrefab, queenSpawns[team - 1].position, queenSpawns[team - 1].rotation);
                    }
                    queen.name = playerSpecies[2] + "AntQueen" + team;
                    queen.GetComponent<BaseAntQueenAI>().difficulty = enemy2;
                    queen.GetComponent<SHealth>().UpdateTeam(team);
                    queen.GetComponent<BaseAntQueenAI>().spawnPos = nestSpawns[team - 1];
                }
                else if (team == playerTeams[3])
                {
                    if (playerSpecies[3] == "Black")
                    {
                        queen = Instantiate(blackAntQueenPrefab, queenSpawns[team - 1].position, queenSpawns[team - 1].rotation);
                    }
                    else if (playerSpecies[3] == "Fire")
                    {
                        queen = Instantiate(fireAntQueenPrefab, queenSpawns[team - 1].position, queenSpawns[team - 1].rotation);
                    }
                    queen.name = playerSpecies[3] + "AntQueen" + team;
                    queen.GetComponent<BaseAntQueenAI>().difficulty = enemy3;
                    queen.GetComponent<SHealth>().UpdateTeam(team);
                    queen.GetComponent<BaseAntQueenAI>().spawnPos = nestSpawns[team - 1];
                }
            }
        }
    }

    public void TeamDied()
    {
        numberTeams -= 1;
        if(numberTeams == 1)
        {
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Win");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Stop("GameMusic");
            GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("MenuMusic");
            SceneManager.LoadScene("WinMenu");
        }
    }


    public void FindMap()
    {
        mapDropdown = GameObject.Find("MapDropdown").GetComponent<TMPro.TMP_Dropdown>();
        gameScene = mapDropdown.options[mapDropdown.value].text;
    }

    public void UpdateNestSpawns()
    {
        queenSpawns[0] = GameObject.Find("QueenSpawn1").GetComponent<Transform>();
        queenSpawns[1] = GameObject.Find("QueenSpawn2").GetComponent<Transform>();
        queenSpawns[2] = GameObject.Find("QueenSpawn3").GetComponent<Transform>();
        queenSpawns[3] = GameObject.Find("QueenSpawn4").GetComponent<Transform>();

        if (gameScene == "Simple Arena")
        {
            nestSpawns[0] = new Vector3(0, 50, 0);
            nestSpawns[1] = new Vector3(50, 0, 0);
            nestSpawns[2] = new Vector3(0, -50, 0);
            nestSpawns[3] = new Vector3(-50, 0, 0);
        }
        else if (gameScene == "Corridor")
        {
            nestSpawns[0] = new Vector3(-70, 20, 0);
            nestSpawns[1] = new Vector3(-70, -20, 0);
            nestSpawns[2] = new Vector3(70, 20, 0);
            nestSpawns[3] = new Vector3(70, -20, 0);
        }
    }

    public void CreateUI()
    {
        if(playerSpecies[0] == "Black")
        {
            Instantiate(BlackUI);
        }
        else if (playerSpecies[0] == "Fire")
        {
            Instantiate(FireUI);
        }
    }

    IEnumerator TutorialStart()
    {
        enemy1 = 0;
        enemy2 = 3;
        enemy3 = 3;
        numberTeams = 2;

        playerTeams[0] = 1;
        playerTeams[1] = 2;

        playerSpecies[0] = "Black";
        yield return null;

        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            tutorialText = GameObject.Find("TutorialText").GetComponent<TMP_Text>();
            player = GameObject.Find("Player_Singleplayer(Black)");
            tutorialPart = 1;
        }
    }
}
