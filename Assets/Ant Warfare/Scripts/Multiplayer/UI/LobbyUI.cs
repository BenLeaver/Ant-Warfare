using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Handles the lobby UI for the multiplayer game, including displaying the host IP, 
/// number of connected players, and showing the start game button for the host.
/// </summary>
public class LobbyUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI ipAddressText;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private TextMeshProUGUI numPlayersText;

    // Start is called before the first frame update
    void Start()
    {
        string ip = GameObject.Find("ProjectSceneManager").GetComponent<ProjectSceneManager>().GetHostIP();
        if(ip != "")
        {
            ipAddressText.text = "IP: " + ip;
        }
        else
        {
            ipAddressText.enabled = false;
        }

        if(NetworkManager.Singleton.IsServer)
        {
            Debug.Log("This is the server");
        }

        startGameButton.SetActive(false);

        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
    }

    public void StartGameButton()
    {
        GameObject.Find("ProjectSceneManager").GetComponent<ProjectSceneManager>().StartGame();
        startGameButton.SetActive(false);
    }

    /// <summary>
    /// Start button will appear for host when at least 2 players have connected.
    /// The numPlayersText text will be updated.
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        Debug.Log("On Client Connected Callback");
        if (!NetworkManager.IsServer) return;

        

        UpdateNumPlayersClientRpc(NetworkManager.ConnectedClients.Count);

        if (NetworkManager.ConnectedClients.Count >= 2)
        {
            startGameButton.SetActive(true);
        }
    }

    [ClientRpc]
    private void UpdateNumPlayersClientRpc(int connectedClients)
    {
        numPlayersText.text = $"{connectedClients}/4";
    }
}
