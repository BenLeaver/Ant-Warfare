using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

/// <summary>
/// Core abstract controller for an individual ant's AI state machine.
/// 
/// This class owns:
/// -   All state instances (Search, Guard, FollowPath, etc.)
/// -   The active state
/// -   An interrupt stack allowing temporary state overrides for food, enemy and player follow interrupts.
/// -   Pheromone evaluation logic that decides which state the ant should enter next based 
///     on the weight and mean targets of the nearby pheromone types.
/// 
/// 
/// The manager (and its states) use the IAntWorld (sometimes via the AntContext) for perception 
/// and performing actions.
/// 
/// Each state can have multiple subtypes, further customising behaviour. 
/// For example, in the FollowPath state the subtype determines which interrupts are enabled.
/// </summary>
public class AntStateManager
{
    private AntContext context;

    public AntSearchState searchState;
    public AntGuardState guardState; 
    public AntFollowPathState followPathState; 
    public AntConfusedState confusedState; 
    public AntFoodIRState foodIRState; 
    public AntEnemyIRState enemyIRState; 
    public AntCarryFoodIRState carryFoodIRState; 
    public AntFollowPlayerIRState followPlayerIRState;

    private Stack<AntStateStackEntry> interruptStack = new();
    private BaseAntState currentState;

    public AntStateType CurrentStateType;

    Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3)>> pheromones;
    [SerializeField] private float pheromoneMajorityThreshold = 3f;

    // Anti-infinite-loop guard
    private int lastStateChangeFrame = -1;
    private int stateChangesThisFrame = 0;
    private const int MaxStateChangesPerFrame = 5;

    public AntStateManager(AntContext context)
    {
        this.context = context;

        searchState = new AntSearchState(this, context, AntStateType.Search);
        guardState = new AntGuardState(this, context, AntStateType.Guard);
        followPathState = new AntFollowPathState(this, context, AntStateType.FollowPath);
        confusedState = new AntConfusedState(this, context, AntStateType.Confused);
        foodIRState = new AntFoodIRState(this, context, AntStateType.FoodIR);
        enemyIRState = new AntEnemyIRState(this, context, AntStateType.EnemyIR);
        carryFoodIRState = new AntCarryFoodIRState(this, context, AntStateType.FoodIR);
        followPlayerIRState = new AntFollowPlayerIRState(this, context, AntStateType.FollowPlayerIR);
    }

    /// <summary>
    /// Called to immediately decide the first state.
    /// </summary>
    public void Initialize()
    {
        DecideNextState();
    }

    /// <summary>
    /// Called every frame on Update from the AntWorld and updates the active state.
    /// </summary>
    public void Tick(float deltaTime)
    {
        context.AttackTimer += deltaTime;
        currentState?.UpdateState(deltaTime);
    }

    /// <summary>
    /// Switches to a new state.
    /// </summary>
    public void ChangeState(BaseAntState newState, Vector3 target = default, int subtype = 0)
    {
        // Infinite loop guard
        if (Time.frameCount == lastStateChangeFrame)
        {
            stateChangesThisFrame++;

            if (stateChangesThisFrame > MaxStateChangesPerFrame)
            {
                Debug.LogError(
                    $"[AntStateManager] Ant changed state " +
                    $"{stateChangesThisFrame} times in one frame. " +
                    $"Forcing confused state to prevent infinite loop."
                );

                newState = confusedState;
            }
        }
        else
        {
            lastStateChangeFrame = Time.frameCount;
            stateChangesThisFrame = 1;
        }

        // State Change logic
        context.World.StopMovement();
        currentState = newState;
        currentState.EnterState(target, subtype);
        CurrentStateType = newState.StateType;
    }

    /// <summary>
    /// Temporarily interrupts the current state and pushes the current state data onto the stack.
    /// </summary>
    public void PushInterrupt(BaseAntState interruptState, Vector3 target = default, int subtype=0)
    {
        if (currentState != null)
        {
            var snapshot = CaptureSnapshot(currentState);

            interruptStack.Push(new AntStateStackEntry
            {
                State = currentState,
                Snapshot = snapshot
            });
        }

        currentState = interruptState;
        currentState.EnterState(target, subtype);
        CurrentStateType = interruptState.StateType;
    }

    /// <summary>
    /// Returns to the previous interrupted state, restoring its snapshot.
    /// If no interrupted state exists, the ant returns to normal decision-making.
    /// </summary>
    public void PopInterrupt()
    {
        if (interruptStack.Count == 0)
        {
            ForceReturnToThinking();
            return;
        }

        var entry = interruptStack.Pop();

        RestoreSnapshot(entry);

        currentState = entry.State;
        currentState.EnterState(isResuming: true);
        CurrentStateType = entry.State.StateType;
    }

    /// <summary>
    /// Permanently clears previous data on the stack.
    /// </summary>
    public void HardInterrupt(BaseAntState forcedState, Vector3 target = default, int subtype = 0)
    {
        interruptStack.Clear();

        currentState = forcedState;
        currentState.EnterState(target, subtype);
        CurrentStateType = forcedState.StateType;
    }

    /// <summary>
    /// Takes a snapshot for the current state to be stored on the stack.
    /// </summary>
    private AntContextSnapshot CaptureSnapshot(BaseAntState state)
    {
        return new AntContextSnapshot
        {
            LocalTarget = context.LocalTarget,
            UltimateTarget = context.UltimateTarget,
            StateType = state.StateType,
            StateSubType = state.SubType
        };
    }

    /// <summary>
    /// Restores the local and ultimate targets from the snapshot.
    /// </summary>
    private void RestoreSnapshot(AntStateStackEntry entry)
    {
        context.LocalTarget = entry.Snapshot.LocalTarget;
        context.UltimateTarget = entry.Snapshot.UltimateTarget;
    }

    /// <summary>
    /// Clears all interrupts and decides next state.
    /// </summary>
    public void ForceReturnToThinking()
    {
        interruptStack.Clear();
        DecideNextState();
    }

    /// <summary>
    /// If not carrying food, begins following the player.
    /// </summary>
    /// <param name="subtype"></param>
    public bool StartPlayerIR(int subtype)
    {
        // If ant is already carrying food back to nest, can't follow player.
        if (currentState == carryFoodIRState) return false;

        HardInterrupt(followPlayerIRState, subtype: subtype);
        return true;
    }

    /// <summary>
    /// Exits from the player interrupt state.
    /// </summary>
    public void StopPlayerIR()
    {
        ForceReturnToThinking();
    }

    /// <summary>
    /// Will make a decision based on nearby pheromones to determine which state to enter.
    /// </summary>
    public void DecideNextState()
    {
        pheromones = context.EvaluatePheromones();

        var pheromoneStats = new List<(PheromoneType type, float sum, Vector3 meanTarget, PheromoneSubtype subtype)>();

        // Get stats for each pheromone type
        foreach (var kvp in pheromones)
        {
            var stats = GetStats(kvp.Key);
            pheromoneStats.Add((kvp.Key, stats.totalWeight, stats.meanTarget, stats.strongestSubtype));
        }

        // Sort descending by sum
        pheromoneStats.Sort((a, b) => b.sum.CompareTo(a.sum));


        if (pheromoneStats.Count == 0)
        {
            // If no pheromones exist nearby -> Enter Search State.
            ChangeState(searchState);
            return;
        }

        // If nearby pheromone strength is very low -> Enter Search State.
        if (pheromoneStats[0].sum <= pheromoneMajorityThreshold)
        {
            ChangeState(searchState);
            return;
        }

        // If there is more than one pheromone type, check that there is a clear majority.
        // If there is no clear majority -> Enter Confused State.
        // The confused state is designed to handle cases where the ant is on the 'border'
        // between two opposing instructions, and prevents the ant constantly changing it's mind.
        if (pheromoneStats.Count >= 2)
        {
            if (pheromoneStats[0].sum - pheromoneStats[1].sum <= pheromoneMajorityThreshold)
            {
                // Enter confused state
                ChangeState(confusedState);
                return;
            }
        }

        // Enter state given by strongest pheromone type.
        // Need to include subtype, defining some of the specifics/parameters about what the ant can do.
        PheromoneSubtype sub = pheromoneStats[0].subtype;
        Vector3 T = context.World.FindClosestReachablePoint(pheromoneStats[0].meanTarget);
        if (sub == PheromoneSubtype.UnifiedFollowPath || sub == PheromoneSubtype.SearchPath || sub == PheromoneSubtype.FoodPath)
        {
            ChangeState(followPathState, T);
        }
        else if (sub == PheromoneSubtype.SoldierAttackPath)
        {
            ChangeState(followPathState, T, 1);
        }
        else if (sub == PheromoneSubtype.WorkerGatheringPath)
        {
            ChangeState(followPathState, T, 2);
        }
        else if (sub == PheromoneSubtype.GuardPoint)
        {
            ChangeState(guardState, T);
        }
        else
        {
            Debug.LogWarning("No Next State was decided.");
        }
    }

    /// <summary>
    /// Computes the total weight, weighted mean target, and strongest subtype for a pheromone type.
    /// </summary>
    private (float totalWeight, Vector3 meanTarget, PheromoneSubtype strongestSubtype) GetStats(PheromoneType type)
    {
        if (!pheromones.TryGetValue(type, out var inner) || inner.Count == 0)
            return (0f, Vector3.zero, default);

        float total = 0f;
        Vector3 weightedSum = Vector3.zero;

        // For calculating strongest subtype
        float maxWeight = float.MinValue;
        PheromoneSubtype strongestSubtype = default;

        foreach (var kvp in inner)
        {
            float w = kvp.Key;
            
            var (subtype, target) = kvp.Value;

            total += w;
            weightedSum += target * w;

            if (w > maxWeight)
            {
                maxWeight = w;
                strongestSubtype = subtype;
            }
        }

        Vector3 meanTarget = total > 0f ? weightedSum / total : Vector3.zero;
        return (total, meanTarget, strongestSubtype);
    }

    /// <summary>
    /// Will return whether the ant can place pheromones, checking whether the pheromone density is lower than the pheromoneMajorityThreshold.
    /// </summary>
    public bool CanPlacePheromones()
    {
        var pheromones = context.EvaluatePheromones();

        foreach (var kvp in pheromones)
        {
            var stats = GetStats(kvp.Key);
            if (stats.totalWeight > pheromoneMajorityThreshold)
            {
                // Strength of nearby pheromones is too high.
                return false;
            }
        }

        return true;
    }
}
