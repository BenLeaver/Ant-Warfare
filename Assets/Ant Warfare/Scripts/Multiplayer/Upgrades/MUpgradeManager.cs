using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;
using Unity.Netcode;

/// <summary>
/// Communicates with the Upgrader system to synchronise upgrades across the network.
/// </summary>
public class MUpgradeManager : MonoBehaviour
{
    public GameObject multiplayerUI;
    private GameObject[] Ants;
    private int thisTeam;

    /// <summary>
    /// Applies the effect of an upgrade.
    /// </summary>
    /// <param name="name">The name of the upgrade to apply.</param>
    public void ApplyUpgrade(string name)
    {
        Ants = GameObject.FindGameObjectsWithTag("Ant");
        thisTeam = multiplayerUI.GetComponent<Multiplayer_UI>().playerTeam;
        
        if (GameObject.Find("ProjectSceneManager"))
        {
            GameObject.Find("ProjectSceneManager").GetComponent<Upgrader>().SyncUpgradeRpc(thisTeam, name);
        }
    }
}
