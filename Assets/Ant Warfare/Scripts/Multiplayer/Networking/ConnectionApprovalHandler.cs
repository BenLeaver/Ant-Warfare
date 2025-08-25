using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Handles connection approval for clients attempting to join the server.
/// Limits the maximum number of players.
/// </summary>
public class ConnectionApprovalHandler : NetworkBehaviour
{

    [SerializeField] private const int maxPlayers = 4;

    public void Start()
    {
        NetworkManager.ConnectionApprovalCallback = ApprovalCheck;
    }

    /// <summary>
    /// Checks whether a player can be approved to join the game.
    /// If max number of players is already reached, no new players can join.
    /// </summary>
    /// <param name="request">ConnectionApprovalRequest from the client.</param>
    /// <param name="response">ConnectionApprovalResponse to the client.</param>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        response.PlayerPrefabHash = null;
        response.Approved = true;
        response.CreatePlayerObject = false;

        if(NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            response.Approved = false;
            response.Reason = "Server is Full";
        }

        response.Pending = false;
    }
}
