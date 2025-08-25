using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Handles health of ants in Multiplayer.
/// </summary>
public class MHealth : NetworkBehaviour
{
    public float maxHealth = 100f;
    public float health = 100f;
    public int team = 1;
    public Slider slider;
    public Image fill;

    public GameObject deathObject;
    public GameObject hitParticleEffect;
    public bool alreadyDead = false;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        slider.value = health;
        UpdateTeam(team);
    }

    /// <summary>
    /// Resets health to maximum value, and sets alreadyDead to false.
    /// </summary>
    public void SetHealthToMax()
    {
        alreadyDead = false;
        if (!IsServer)
        {
            return;
        }
        health = maxHealth;
        slider.value = health;
        HealthSyncRpc(health);
    }

    /// <summary>
    /// Updates health based on a given amount of damage.
    /// 
    /// Called when another ant attacks this ant. Provide a negative damage value to heal.
    /// </summary>
    /// <param name="damage"></param>
    public void UpdateHealth(float damage)
    {
        if (damage > 0)
        {
            AttackSoundRpc();
        }
        if (!IsServer)
        {
            return;
        }
        health -= damage;
        slider.value = health;
        HealthSyncRpc(health);
    }

    [Rpc(SendTo.Everyone)]
    void AttackSoundRpc()
    {
        gameObject.GetComponent<ObjectAudioManager>().Play("Attack");
        Instantiate(hitParticleEffect, transform.position, Quaternion.identity);
    }

    /// <summary>
    /// Synchronizes value of health for all clients.
    /// Also checks whether ant should die.
    /// </summary>
    /// <param name="syncedHealth"></param>
    [Rpc(SendTo.Everyone)]
    public void HealthSyncRpc(float syncedHealth)
    {
        health = syncedHealth;
        slider.value = health;
        if (health <= 0 && !alreadyDead)
        {
            alreadyDead = true;
            HandleDeath();
        }
    }
    
    /// <summary>
    /// Calls Death() on the right ant script.
    /// </summary>
    private void HandleDeath()
    {
        Debug.Log("HandleDeath()");
        Instantiate(deathObject, gameObject.transform.position, gameObject.transform.rotation);
        if (GetComponent<PlayerController>())
        {
            Debug.Log("GetComponent<PlayerController>()");
            gameObject.GetComponent<PlayerController>().Death();
        }
        else if (GetComponent<MAntBaseAI>())
        {
            gameObject.GetComponent<MAntBaseAI>().Death();
        }
        else if (GetComponent<MBaseAntQueenAI>())
        {
            gameObject.GetComponent<MBaseAntQueenAI>().Death();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Updates value of team and ensures color of health bar changes accordingly.
    /// </summary>
    /// <param name="newTeam"></param>
    public void UpdateTeam(int newTeam)
    {
        team = newTeam;
        if (team == 1)
        {
            //Red
            fill.color = new Color32(209, 55, 44, 255);
        }
        if (team == 2)
        {
            //Green
            fill.color = new Color32(59, 219, 60, 255);
        }
        if (team == 3)
        {
            //Blue
            fill.color = new Color32(59, 144, 219, 255);
        }
        if (team == 4)
        {
            //Purple
            fill.color = new Color32(143, 59, 219, 255);
        }
    }
}