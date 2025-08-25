using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a temporary object created when an entity dies.
/// Plays a death sound, spawns a particle effect, and self-destructs after a short duration.
/// </summary>
public class DeathObject : MonoBehaviour
{
    public GameObject deathParticleEffect;
    private float deathTimer = 0f;

    void Awake()
    {
        gameObject.GetComponent<ObjectAudioManager>().Play("Death");
    }

    void Start()
    {
        Instantiate(deathParticleEffect, transform.position, Quaternion.identity);
    }

    void Update()
    {
        deathTimer += Time.deltaTime;
        if(deathTimer >= 5f)
        {
            Destroy(gameObject);
        }
    }
}
