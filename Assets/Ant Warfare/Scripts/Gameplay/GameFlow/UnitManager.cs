using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages and keeps track of Ant and Bug units in the game.
/// </summary>
public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    public List<GameObject> ants = new List<GameObject>();
    public List<GameObject> bugs = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Returns all units (Ants and Bugs).
    /// </summary>
    public IEnumerable<GameObject> AllUnits
    {
        get
        {
            foreach (var a in ants) yield return a;
            foreach (var b in bugs) yield return b;
        }
    }

    /// <summary>
    /// Returns all Ant units.
    /// </summary>
    public IEnumerable<GameObject> Ants
    {
        get
        {
            foreach (var a in ants) yield return a;
        }
    }

    /// <summary>
    /// Returns all Bug units.
    /// </summary>
    public IEnumerable<GameObject> Bugs
    {
        get
        {
            foreach (var b in bugs) yield return b;
        }
    }

    /// <summary>
    /// Adds a unit to the manager if it is an Ant or Bug.
    /// </summary>
    /// <param name="unit">The unit to register.</param>
    public void RegisterUnit(GameObject unit)
    {
        string tag = unit.tag;
        if (tag == "Ant" && !ants.Contains(unit))
        {
            ants.Add(unit);
        }
        else if (tag == "Bug" && !bugs.Contains(unit))
        {
            bugs.Add(unit);
        }
    }

    /// <summary>
    /// Removes a unit from the manager.
    /// </summary>
    /// <param name="unit">The unit to unregister.</param>
    public void UnregisterUnit(GameObject unit)
    {
        string tag = unit.tag;
        if (tag == "Ant")
        {
            ants.Remove(unit);
        }
        else if (tag == "Bug")
        {
            bugs.Remove(unit);
        }
    }
}
