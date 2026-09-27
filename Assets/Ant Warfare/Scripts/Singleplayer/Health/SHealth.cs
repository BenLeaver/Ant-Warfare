using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles health of ants in Singleplayer.
/// </summary>
public class SHealth : MonoBehaviour, IHealth
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health = 100f;
    [SerializeField] private Slider slider;
    [SerializeField] private Image fill;

    public GameObject deathObject;
    public GameObject hitParticleEffect;

    public float Health 
    {
        get => health;
        set => health = value;
    }

    public float MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }

    public int Team { get; set; } = -1;

    /// <summary>
    /// Updates health of this ant, given a damage value. To heal the ant instead of attacking, 
    /// provide a negative damage value.
    /// </summary>
    /// <param name="damage">Amount of health to lose.</param>
    public void UpdateHealth(float damage)
    {
        if (damage > 0)
        {
            Instantiate(hitParticleEffect, transform.position, Quaternion.identity);
        }

        Health -= damage;
        slider.value = Health;

        if (Health <= 0)
        {
            HandleDeath();
        }
        else if (damage > 0)
        {
            if (GetComponent<ObjectAudioManager>())
            {
                gameObject.GetComponent<ObjectAudioManager>().Play("Attack");
            }
            if (GetComponent<BeetleStateManager>())
            {
                gameObject.GetComponent<BeetleStateManager>().DamageTaken();
            }
        }
    }

    public void ResetHealth()
    {
        Health = MaxHealth;
        slider.value = Health;
    }

    private void HandleDeath()
    {
        Instantiate(deathObject, transform.position, transform.rotation);

        if (GetComponent<IAntWorld>() != null)
        {
            gameObject.GetComponent<IAntWorld>().Death();
        }
        else if (GetComponent<Player_Singleplayer>())
        {
            gameObject.GetComponent<Player_Singleplayer>().Death();
        }
        else if (GetComponent<BaseAntQueenAI>())
        {
            gameObject.GetComponent<BaseAntQueenAI>().Death();
        }
        else if (GetComponent<BugStateManager>())
        {
            gameObject.GetComponent<BugStateManager>().Death();
        }
        else if (GetComponent<AntTutorialAI>())
        {
            gameObject.GetComponent<AntTutorialAI>().Death();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialise the health bar, setting the team colour and setting fill to max.
    /// </summary>
    public void UpdateTeam(int newTeam)
    {
        //Debug.Log($"Updating team to {newTeam} for {gameObject.name}");
        Team = newTeam;
        if (Team == 1)
        {
            // Red
            fill.color = new Color32(209, 55, 44, 255);
        }
        else if (Team == 2)
        {
            // Green
            fill.color = new Color32(59, 219, 60, 255);
        }
        else if (Team == 3)
        {
            // Blue
            fill.color = new Color32(59, 144, 219, 255);
        }
        else if (Team == 4)
        {
            // Purple
            fill.color = new Color32(143, 59, 219, 255);
        }
        else if (Team == 5)
        {
            // Grey
            fill.color = new Color32(128, 128, 128, 155);
        }

        Health = MaxHealth;
        slider.value = Health;
    }
}
