using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Handles spawning of player objects on the server.
/// </summary>
public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;

    [ServerRpc(RequireOwnership=false)]
    public void SpawnPlayerServerRpc(ulong clientId, int prefabId)
    {
        GameObject newPlayer;
        newPlayer = (GameObject)Instantiate(playerPrefabs[prefabId]);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
        Debug.Log("PlayerSpawned");
    }
}
