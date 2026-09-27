using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Acts as context for the stage beetle's finite state machine.
/// Holds state instances, runs updates, and provides shared utilities.
/// </summary>
public class StagStateManager : BugStateManager
{
    [Header("NavMesh & Movement")]
    public float moveSpeed = 2f;
    public float fastMoveSpeed = 3f;

    [Header("Attacks")]
    public float simpleAttackDamage = 50f;
    public float throwDamage = 40f;
    public GameObject throwImpactEffect;


    public StagIdleState IdleState { get; private set; }
    public StagMoveState MoveState { get; private set; }
    public StagMoveTowardsEnemyState MoveTowardsEnemyState { get; private set; }
    public StagAttackIdleState AttackIdleState { get; private set; }
    public StagSimpleAttackState SimpleAttackState { get; private set; }
    public StagThrowState ThrowState { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        IdleState = new StagIdleState(this);
        MoveState = new StagMoveState(this);
        MoveTowardsEnemyState = new StagMoveTowardsEnemyState(this);
        AttackIdleState = new StagAttackIdleState(this);
        SimpleAttackState = new StagSimpleAttackState(this);
        ThrowState = new StagThrowState(this);

        currentState = IdleState;
        currentState.EnterState();
    }

    public override void Update()
    {
        base.Update();
    }

    public Vector3 GetEnemyThrowTarget(GameObject thrownAnt)
    {
        const float maxThrowRange = 20f;
        const float minThrowRange = 10f;
        const int attempts = 100;

        UnitInfo targetEnemy = FindFurthestEnemyInRange(maxThrowRange, minThrowRange, thrownAnt);
        Vector3 targetPos;

        if (targetEnemy != null)
        {
            // Throw ant towards another enemy within the range.
            targetPos = targetEnemy.transform.position;
            float rX = Random.Range(-1.5f, 1.5f);
            float rY = Random.Range(-1.5f, 1.5f);
            targetPos.x += rX;
            targetPos.y += rY;

            if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas) 
                && HasValidPath(hit.position))
            {
                return hit.position;
            }
            // If path is invalid (unlikely), fall through to random logic
        }
        
        for (int i = 0; i < attempts; i++)
        {
            // Throw ant in a random direction
            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(maxThrowRange * 0.5f, maxThrowRange);

            Vector3 candidate =
                myInfo.transform.position +
                new Vector3(dir.x, dir.y, 0f) * dist;

            if (!UnityEngine.AI.NavMesh.SamplePosition(candidate, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                continue;

            if (HasValidPath(hit.position))
                return hit.position;
        }

        Debug.LogWarning($"[{name}] GetEnemyThrowTarget failed: no reachable target found.");

        return myInfo.transform.position;
    }
}
