using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Handles LAN connection setup for the multiplayer game, including hosting and joining games.
/// Provides methods to start a host, start a client, validate IP addresses, and return to the main menu.
/// </summary>
public class LANConnection : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI ipAddressText;
    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] TMPro.TMP_Dropdown speciesDropdown;
    [SerializeField] TMPro.TMP_Dropdown mapDropdown;

    [Header("Connection Data")]
    [SerializeField] private string ipAddress;
    [SerializeField] private ProjectSceneManager sceneManager;
    [SerializeField] private string lobbySceneName = "LANLobby";

    /// <summary>
    /// Exits the multiplayer session, destroys network-related objects, and loads the MainMenu scene.
    /// </summary>
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        Destroy(GameObject.Find("ProjectSceneManager"));
    }

    // Start is called before the first frame update
    void Start()
    {
        ipAddress = "0.0.0.0";
        SetIpAddress();
    }

    /// <summary>
    /// Starts the host session, sets player species and map, and changes to the lobby scene.
    /// </summary>
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        GetLocalIPAddress();
        sceneManager = GameObject.Find("ProjectSceneManager").GetComponent<ProjectSceneManager>();
        sceneManager.SetSpecies(speciesDropdown.value);
        sceneManager.SetMap(mapDropdown.value);
        sceneManager.SetHostIP(ipAddress);
        sceneManager.SetUsername(usernameInputField.text);
        sceneManager.ChangeScene(lobbySceneName);
    }

    /// <summary>
    /// Returns the local IPv4 address of this device.
    /// </summary>
    /// <returns>The local IP address as a string.</returns>
    /// <exception cref="System.Exception">Thrown if no IPv4 address is found.</exception>
    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach(var ip in host.AddressList)
        {
            if(ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = ip.ToString();
                return ipAddress.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    /// <summary>
    /// Starts a client connection using the IP address entered in the input field.
    /// </summary>
    public void StartClient()
    {
        ipAddress = ipInputField.text;
        if (IsIPValid())
        {
            SetIpAddress();
            sceneManager = GameObject.Find("ProjectSceneManager").GetComponent<ProjectSceneManager>();
            sceneManager.SetSpecies(speciesDropdown.value);
            sceneManager.SetUsername(usernameInputField.text);
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            ipInputField.text = "";
        }
    }

    /// <summary>
    /// Updates the Unity Transport component with the current IP address.
    /// </summary>
    private void SetIpAddress()
    {
        if(NetworkManager.Singleton.TryGetComponent(out UnityTransport transport))
        {
            transport.ConnectionData.Address = ipAddress;
        }
    }

    /// <summary>
    /// Validates the IP address format.
    /// </summary>
    /// <returns>
    /// True if the IP address is valid and not just a numeric value; false otherwise.
    /// </returns>
    /// <remarks>
    /// This method does not fully guarantee that the IP is reachable or correct; it only checks formatting.
    /// </remarks>
    private bool IsIPValid()
    {
        IPAddress address;
        if (IPAddress.TryParse(ipAddress, out address))
        {
            if (int.TryParse(ipAddress, out _))
            {
                Debug.LogWarning("Invalid IP Address.");
                return false;
            }
            return true;
        }
        Debug.LogWarning("Invalid IP Address.");
        return false;
    }
}
