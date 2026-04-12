using UnityEngine;

namespace Ant.AI
{

    /// <summary>
    /// Type/class of ant. Used for tuning behaviour.
    /// </summary>
    public enum AntType
    {
        Worker,
        Soldier,
        Player,
        Queen
    }

    /// <summary>
    /// Identifies the current logical state of the ant.
    /// Used for snapshot restoration and debugging.
    /// </summary>
    public enum AntStateType
    {
        Search,
        Guard,
        FollowPath,
        FollowPlayerIR,
        Confused,
        FoodIR,
        EnemyIR,
        CarryFoodIR
    }
}
