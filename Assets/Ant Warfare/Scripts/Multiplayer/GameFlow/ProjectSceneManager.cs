using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Manages scene transitions, player initialization, and game flow for the project. 
/// Implements a persistent singleton network manager to maintain state across scenes.
/// Handles player teams, species, nests, queens, and end-of-game logic.
/// </summary>
public class ProjectSceneManager : SingletonNetworkPersistent<ProjectSceneManager>
{

    public NetworkList<int> playerTeams;
    public Vector3[] nestSpawns = new Vector3[4];
    public Transform[] queenSpawns = new Transform[4];
    public GameObject blackAntQueenPrefab;
    public GameObject fireAntQueenPrefab;
    private GameObject queen;

    [SerializeField] private GameObject[] players = new GameObject[4];
    [SerializeField] private string[] playerSpecies = new string[4];
    [SerializeField] private int playersRemaining = 0;
    

    [SerializeField] private string hostIPAddress;
    [SerializeField] private string username;

    private int speciesIndex;
    private string sceneName;
    private Scene loadedScene;
    private bool hostToBeLoaded = false;
    private bool gameSceneLoaded = false;
    private string mapName;
    [SerializeField] private string[] maps = { "Simple Arena", "Corridor" };

    public void SetMap(int mIndex)
    {
        mapName = maps[mIndex];
    }

    public override void Awake()
    {
        base.Awake();
        playerTeams = new NetworkList<int>();
    }

    public void Update()
    {
        if(gameSceneLoaded)
        {
            gameSceneLoaded = false;
            LoadGame();
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            if (IsServer)
            {
                AllPlayersLeaveClientRpc();
            }
        }
    }

    [ClientRpc]
    public void AllPlayersLeaveClientRpc()
    {
        if(NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        SceneManager.LoadScene("MainMenu");
        Cleanup();
    }

    public void Cleanup()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        Destroy(this.gameObject);
    }

    public bool GetIsServer()
    {
        return (IsServer);
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            hostToBeLoaded = true;
        }
        else
        {
            GetComponent<PlayerSpawner>().SpawnPlayerServerRpc(NetworkManager.LocalClientId, speciesIndex);
        }
        NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
        base.OnNetworkSpawn();
    }

    public void ChangeScene(string aSceneName)
    {
        UpdateSceneNameClientRpc(aSceneName);
        sceneName = aSceneName;
        if (IsServer && !string.IsNullOrEmpty(aSceneName))
        {
            loadedScene = SceneManager.GetActiveScene();
            LoadScene();
        }
    }

    public void SetSpecies(int aSpeciesIndex)
    {
        speciesIndex = aSpeciesIndex;
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        if(sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
        {
            //Will unload previous scene after new one loaded
            UnloadScene();
        }
        if(sceneEvent.SceneEventType == SceneEventType.UnloadEventCompleted)
        {
            foreach (string mapName in maps)
            {
                if (SceneManager.GetActiveScene().name == mapName)
                {
                    gameSceneLoaded = true;
                }
            }
        }
    }

    private void LoadScene()
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
   
    private void UnloadScene()
    {
        // Ensure only the server calls this when the NetworkObject is
        // spawned and the scene is loaded.
        if (!IsServer || !IsSpawned || !loadedScene.IsValid() || !loadedScene.isLoaded)
        {
            return;
        }

        NetworkManager.SceneManager.UnloadScene(loadedScene);

        OnSceneChanged();
    }

    private void OnSceneChanged()
    {
        if (hostToBeLoaded && sceneName == "LANLobby")
        {
            GetComponent<PlayerSpawner>().SpawnPlayerServerRpc(NetworkManager.LocalClientId, speciesIndex);
            hostToBeLoaded = false;
        }
        else if (sceneName == "MEndScreen")
        {
            GameEndClientRpc(GetWinningTeam(), GetWinningUsername());
        }
    }

    public void SetHostIP(string ip)
    {
        hostIPAddress = ip;
    }

    public string GetHostIP()
    {
        return hostIPAddress;
    }

    public void SetUsername(string aUsername)
    {
        if(aUsername == "")
        {
            username = "Anonymous";
        }
        else if(aUsername.Length <= 20)
        {
            username = aUsername;
        }
        else
        {
            username = aUsername.Substring(0, 20);
        }
    }

    public string GetUsername()
    {
        return username;
    }

    public void StartGame()
    {
        ChangeScene(mapName);
        if(IsServer)
        {
            GenerateTeams();
        }
    }

    private void LoadGame()
    {
        Debug.Log("Load Game Called");
        Destroy(GameObject.Find("LoadingCamera"));
        UpdateNestSpawns();
        SpawnTeams();
    }

    void GenerateTeams()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                playersRemaining += 1;
                bool uniqueTeam = false;
                int rTeam = 1;
                while(uniqueTeam == false)
                {
                    rTeam = Random.Range(1, 5);
                    uniqueTeam = true;
                    foreach (int n in playerTeams)
                    {
                        if (rTeam == n)
                        {
                            uniqueTeam = false;
                        }
                    }
                }
                playerTeams.Add(rTeam);
                Debug.Log("Created: " + rTeam.ToString());
            }
        }
    }

    void SpawnTeams()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                players[i].GetComponent<PlayerController>().ActivatePlayer(playerTeams[i]);
                players[i].transform.position = nestSpawns[playerTeams[i] - 1];

                if(IsServer)
                {
                    if (playerSpecies[i] == "Black")
                    {
                        var queenInstance = Instantiate(blackAntQueenPrefab, queenSpawns[playerTeams[i] - 1].position, queenSpawns[playerTeams[i] - 1].rotation);
                        queenInstance.GetComponent<NetworkObject>().Spawn();
                        UpdateQueenClientRpc(queenInstance.GetComponent<NetworkObject>(), i);
                    }
                    if (playerSpecies[i] == "Fire")
                    {
                        var queenInstance = Instantiate(fireAntQueenPrefab, queenSpawns[playerTeams[i] - 1].position, queenSpawns[playerTeams[i] - 1].rotation);
                        queenInstance.GetComponent<NetworkObject>().Spawn();
                        UpdateQueenClientRpc(queenInstance.GetComponent<NetworkObject>(), i);
                    }
                }
            }
        }
    }

    [ClientRpc]
    private void UpdateQueenClientRpc(NetworkObjectReference queenRef, int index)
    {
        var queen = ((GameObject)queenRef);
        queen.name = "M" + playerSpecies[index] + "AntQueen" + playerTeams[index];
        queen.GetComponent<MBaseAntQueenAI>().SetPlayerOnTeam(true);
        queen.GetComponent<MHealth>().UpdateTeam(playerTeams[index]);
        queen.GetComponent<MBaseAntQueenAI>().spawnPos = nestSpawns[playerTeams[index] - 1];
    }

    public void AddPlayer(GameObject newPlayer, int playerIndex, string species)
    {
        players[playerIndex] = newPlayer;
        playerSpecies[playerIndex] = species;
    }

    [Rpc(SendTo.Everyone)]
    public void GameEndClientRpc(int winningTeam, string winningUsername)
    {
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                Debug.Log($"GameEnd() called for player {winningUsername} with team {winningTeam}");
                player.GetComponent<PlayerController>().GameEnd(winningTeam, winningUsername);
            }
        }
    }

    private string GetWinningUsername()
    {
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                if (player.GetComponent<PlayerController>().playerLost == false)
                {
                    //This is the winning player
                    return player.GetComponent<PlayerController>().username.Value.ToString();
                }
            }
        }
        return "";
    }

    private int GetWinningTeam()
    {
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                if (player.GetComponent<PlayerController>().playerLost == false)
                {
                    //This is the winning player
                    return player.GetComponent<MHealth>().team;
                }
            }
        }
        // All Colonies Died (Draw)
        return -1;
    }

    /// <summary>
    /// Reduce playersRemaining by 1. If there is now only 1 player left, change to the end game 
    /// screen.
    /// </summary>
    public void PlayerDied()
    {
        if (!IsServer)
        {
            return;
        }

        playersRemaining -= 1;
        if(playersRemaining == 1)
        {
            ChangeScene("MEndScreen");
        }
    }

    [ClientRpc]
    void UpdateSceneNameClientRpc(string name)
    {
        sceneName = name;
    }

    private void UpdateNestSpawns()
    {
        queenSpawns[0] = GameObject.Find("QueenSpawn1").GetComponent<Transform>();
        queenSpawns[1] = GameObject.Find("QueenSpawn2").GetComponent<Transform>();
        queenSpawns[2] = GameObject.Find("QueenSpawn3").GetComponent<Transform>();
        queenSpawns[3] = GameObject.Find("QueenSpawn4").GetComponent<Transform>();

        if (sceneName == "Simple Arena")
        {
            nestSpawns[0] = new Vector3(0, 50, 0);
            nestSpawns[1] = new Vector3(50, 0, 0);
            nestSpawns[2] = new Vector3(0, -50, 0);
            nestSpawns[3] = new Vector3(-50, 0, 0);
        }
        else if (sceneName == "Corridor")
        {
            nestSpawns[0] = new Vector3(-70, 20, 0);
            nestSpawns[1] = new Vector3(-70, -20, 0);
            nestSpawns[2] = new Vector3(70, 20, 0);
            nestSpawns[3] = new Vector3(70, -20, 0);
        }
    }
}
