using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for health management.
/// </summary>
public interface IHealth
{
    float Health { get; set; }
    float MaxHealth { get; set; }
    int Team { get; set; }

    void UpdateHealth(float damage);
    void UpdateTeam(int newTeam);

    void ResetHealth();
}
