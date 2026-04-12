using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.AI;

public class AntStateManager
{
    private AntContext context;

    public AntSearchState searchState;
    public AntGuardState guardState; // DONE
    public AntFollowPathState followPathState; // DONE
    public AntConfusedState confusedState; // TODO
    public AntFoodIRState foodIRState; // TODO
    public AntEnemyIRState enemyIRState; // DONE
    public AntCarryFoodIRState carryFoodIRState; 
    public AntFollowPlayerIRState followPlayerIRState;

    private Stack<AntStateStackEntry> interruptStack = new();
    private BaseAntState currentState;

    public AntStateType CurrentStateType;

    Dictionary<PheromoneType, Dictionary<float, (PheromoneSubtype subtype, Vector3)>> pheromones;
    [SerializeField] private float pheromoneMajorityThreshold = 3f;

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

    public void Initialize()
    {
        DecideNextState();
    }

    public void Tick(float deltaTime)
    {
        context.AttackTimer += deltaTime;
        currentState?.UpdateState(deltaTime);
    }

    public void ChangeState(BaseAntState newState, Vector3 target = default, int subtype = 0)
    {
        context.World.StopMovement();
        currentState = newState;
        currentState.EnterState(target, subtype);
    }

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
    }

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
    }

    /// <summary>
    /// Permanently clears previous data on the stack.
    /// </summary>
    public void HardInterrupt(BaseAntState forcedState, Vector3 target = default, int subtype = 0)
    {
        interruptStack.Clear();

        currentState = forcedState;
        currentState.EnterState(target, subtype);
    }

    /// <summary>
    /// Takes a snapshot for the current state to be stored on the stack.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
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

    private void RestoreSnapshot(AntStateStackEntry entry)
    {
        context.LocalTarget = entry.Snapshot.LocalTarget;
        context.UltimateTarget = entry.Snapshot.UltimateTarget;
    }

    public void ForceReturnToThinking()
    {
        interruptStack.Clear();
        DecideNextState();
    }

    public void StartPlayerIR(int subtype)
    {
        // If ant is already carrying food back to nest, can't follow player.
        if (currentState == carryFoodIRState) return;

        PushInterrupt(followPlayerIRState, subtype: subtype);
    }

    public void StopPlayerIR()
    {
        PopInterrupt();
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
        Vector3 T = pheromoneStats[0].meanTarget;
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
}
