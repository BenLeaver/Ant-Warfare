using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles periodic spawning of food objects in the game.
/// Can spawn food in singleplayer or via a networked spawner in multiplayer.
/// </summary>
public class FoodSpawn : MonoBehaviour
{
    public GameObject[] foodPrefabs;
    private float foodTimer = 15f;
    public int batchSize = 5;
    public bool multiplayer = false;
    public int foodTypeIndex;

    void Start()
    {
        if(GameObject.Find("ProjectSceneManager"))
        {
            multiplayer = true;
        }
    }

    void Update()
    {
        foodTimer += Time.deltaTime;
        if (foodTimer >= 15f)
        {
            for (int i = 0; i < batchSize; i++)
            {
                float rX = Random.Range(-10f, 10f) + transform.position.x;
                float rY = Random.Range(-10f, 10f) + transform.position.y;
                if (!multiplayer)
                {
                    Instantiate(foodPrefabs[foodTypeIndex], new Vector3(rX, rY, 0), Quaternion.identity);
                }
                else
                {
                    GameObject.Find("ProjectSceneManager").GetComponent<MFoodSpawner>().SpawnFood(rX, rY, foodTypeIndex);
                }
            }
            foodTimer = 0;
        }
    }
}
