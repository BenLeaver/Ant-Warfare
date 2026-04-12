using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAntWorld
{
    float DeltaTime { get; }

    //int Team { get; }
    //bool IsServer { get; }

    // Movement
    Vector3 Position { get; }
    void SetRotation(Vector3 direction);
    void SetDestination(Vector3 target);
    void StopMovement();
    bool HasReached(Vector3 target, float threshold);
    bool HasActivePath();
    Vector3 GetReachableNavMeshPoint(Vector3 origin, float minRange, float maxRange,
    int attempts = 50, float snapDistance = 2f);

    // Perception
    GameObject FindClosestEnemy();
    GameObject FindClosestFood();
    GameObject FindFriendlyQueen();
    float GetFriendlyQueenDist();
    GameObject FindFriendlyPlayer();
    float SightRange { get; }
    int GetFriendlyColonySize();
    
    // Combat
    void Attack(GameObject targetEnemy);
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
    void PlacePheromone(PheromoneSubtype subtype, Vector3 target);

    // Player Follow Interrupt
    public void PlayerFollowStart(int subtype);
    public void PlayerFollowStop();
}
