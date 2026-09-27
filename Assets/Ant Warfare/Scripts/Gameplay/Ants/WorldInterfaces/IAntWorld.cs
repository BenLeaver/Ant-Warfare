using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Provides an interface between the ant's AI logic and the world it operates in (singleplayer or multiplayer).
/// 
/// Abstracts all world-level operations an ant may need:
/// -   Movement and navigation
/// -   Perception queries
/// -   Combat interactions
/// -   Food handling
/// -   Pheromone placement and sensing
/// -   Player-follow interrupt hooks
/// </summary>
public interface IAntWorld
{
    AntType Type { get; set; }
    float DeltaTime { get; }

    // Control
    void Enable();
    void Disable();
    void InitializeAntFromQueen(UnitInfo q);

    // Movement
    void SetRotation(Vector3 direction);
    void SetDestination(Vector3 target);
    void StopMovement();
    bool HasReached(Vector3 target, float threshold);
    bool HasActivePath();
    Vector3 GetReachableNavMeshPoint(Vector3 origin, float minRange, float maxRange,
    int attempts = 50, float snapDistance = 2f);
    Vector3 FindClosestReachablePoint(Vector3 target);

    // Perception
    Vector3 Position { get; }
    Vector3 MouthPosition { get; }
    UnitInfo FindClosestEnemy();
    GameObject FindClosestFood();
    GameObject FindFriendlyQueen();
    float GetFriendlyQueenDist();
    GameObject FindFriendlyPlayer();
    float SightRange { get; }
    int GetFriendlyColonySize();
    
    // Combat
    void Attack(UnitInfo targetEnemy);
    float AttackDelay { get; }
    float AttackRange { get; }
    public void Death();

    // Food
    void FoodPickup(GameObject food);
    void FoodDrop();
    void CheckInNest();
    bool CheckValid(GameObject food);
    float PickupRange { get; }

    //Pheromones
    Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3 target)>> GetPheromonesNearby();
    Dictionary<float, (PheromoneSubtype subtype, Vector3 target)> GetFoodReturnPathPheromones();
    void PlacePheromone(PheromoneSubtype subtype);
    void PlacePheromone(PheromoneSubtype subtype, Quaternion markerRotation);

    // Player Follow Interrupt
    public bool PlayerFollowStart(int subtype);
    public void PlayerFollowStop();

    // Upgrades
    void UpgradeAttack(float mult);
    void UpgradeSpeed(float mult);
    void UpgradeFoodMult(float mult);
    void ActivateFirstAid();
}
