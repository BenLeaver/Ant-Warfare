using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles server-side spawning of food in multiplayer.
/// </summary>
public class MFoodSpawner : NetworkBehaviour
{
    public GameObject[] foodPrefabs;

    public void SpawnFood(float rX, float rY, int foodTypeIndex)
    {
        if(!IsServer)
        {
            return;
        }
        var foodInstance = Instantiate(foodPrefabs[foodTypeIndex], new Vector3(rX, rY, 0), Quaternion.identity);
        foodInstance.GetComponent<NetworkObject>().Spawn();
    }
}
