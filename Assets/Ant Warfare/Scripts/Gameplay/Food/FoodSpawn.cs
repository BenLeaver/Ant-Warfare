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
                Vector2 offset2D = Random.insideUnitCircle * 10f;
                float rX = offset2D.x + transform.position.x;
                float rY = offset2D.y + transform.position.y;

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
