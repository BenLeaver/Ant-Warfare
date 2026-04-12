using UnityEngine;

namespace Ant.AI
{
    /// <summary>
    /// Snapshot of the ant's working memory at the moment
    /// an interrupt is pushed onto the stack.
    /// Stores INTENT — not mechanics.
    /// </summary>
    public struct AntContextSnapshot
    {

        // Navigation memory
        public Vector3 LocalTarget;
        public Vector3 UltimateTarget;

        // State identity
        public AntStateType StateType;
        public int StateSubType;
    }

    /// <summary>
    /// Represents a paused state + its snapshot.
    /// This is what gets pushed onto the interrupt stack.
    /// </summary>
    public struct AntStateStackEntry
    {
        public BaseAntState State;
        public AntContextSnapshot Snapshot;
    }
}
