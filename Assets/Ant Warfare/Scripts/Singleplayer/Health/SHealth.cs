using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles health of ants in Singleplayer.
/// </summary>
public class SHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health = 100f;
    public int team = 1;
    public Slider slider;
    public Image fill;

    public GameObject deathObject;
    public GameObject hitParticleEffect;
    
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        slider.value = health;
        if(team == 1)
        {
            // Red
            fill.color = new Color32(209, 55, 44, 255);
        }
        if (team == 2)
        {
            // Green
            fill.color = new Color32(59, 219, 60, 255);
        }
        if (team == 3)
        {
            // Blue
            fill.color = new Color32(59, 144, 219, 255);
        }
        if (team == 4)
        {
            // Purple
            fill.color = new Color32(143, 59, 219, 255);
        }
    }

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

        health -= damage;
        if (damage > 0)
        {
            gameObject.GetComponent<ObjectAudioManager>().Play("Attack");
        }
        
        if (health <= 0)
        {
            Instantiate(deathObject, gameObject.transform.position, gameObject.transform.rotation);
            
            if (GetComponent<AntBaseAI>())
            {
                gameObject.GetComponent<AntBaseAI>().Death();
            }
            else if (GetComponent<Player_Singleplayer>())
            {
                gameObject.GetComponent<Player_Singleplayer>().Death();
            }
            else if (GetComponent<BaseAntQueenAI>())
            {
                gameObject.GetComponent<BaseAntQueenAI>().Death();
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
        slider.value = health;
    }

    public void UpdateTeam(int newTeam)
    {
        team = newTeam;
        if (team == 1)
        {
            // Red
            fill.color = new Color32(209, 55, 44, 255);
        }
        if (team == 2)
        {
            // Green
            fill.color = new Color32(59, 219, 60, 255);
        }
        if (team == 3)
        {
            // Blue
            fill.color = new Color32(59, 144, 219, 255);
        }
        if (team == 4)
        {
            // Purple
            fill.color = new Color32(143, 59, 219, 255);
        }
    }
}
