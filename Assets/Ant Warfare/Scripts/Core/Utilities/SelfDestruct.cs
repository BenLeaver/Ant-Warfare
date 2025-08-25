using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys the GameObject it is attached to after a given amount of time.
/// </summary>
public class SelfDestruct : MonoBehaviour
{
    public float destroyTime = 5f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyTime)
        {
            Destroy(gameObject);
        }
    }
}
