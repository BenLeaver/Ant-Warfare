using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Component which can be added to an ant to handle throwing it.
/// </summary>
public class ThrownAnt : MonoBehaviour
{
    private Vector3 start;
    private Vector3 target;
    private float duration;
    private float elapsed;

    private SHealth health;
    private NavMeshAgent agent;
    private Collider col;
    private AntBaseAI ai;

    private float damage;
    private bool finished;

    private GameObject impactParticleEffect;

    /// <summary>
    /// Initialise the thrown ant.
    /// </summary>
    /// <param name="targetPos">The target where the ant will be thrown to.</param>
    /// <param name="flightTime">The time the ant will spend in flight.</param>
    /// <param name="impactDamage">The damage the ant will deal to itself and others on impact.</param>
    public void Initialise(Vector3 targetPos, float flightTime, float impactDamage, GameObject impactParticleEffect)
    {
        start = transform.position;
        target = targetPos;
        duration = flightTime;
        damage = impactDamage;

        elapsed = 0f;
        finished = false;

        health = GetComponent<SHealth>();
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<Collider>();
        ai = GetComponent<AntBaseAI>();

        if (agent) agent.enabled = false;
        if (col) col.enabled = false;
        if (ai) ai.enabled = false;

        this.impactParticleEffect = impactParticleEffect;
    }

    void Update()
    {
        if (finished) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        Vector3 pos = Vector3.Lerp(start, target, t);

        // Generate arc for height using sin curve.
        float height = 3f * Mathf.Sin(t * Mathf.PI);

        pos.z = 0;
        pos += Vector3.up * height;

        transform.position = pos;

        // Spin effect
        transform.Rotate(Vector3.forward, 720f * Time.deltaTime);

        if (t >= 1f)
        {
            Land();
        }
    }

    private void Land()
    {
        finished = true;
        Instantiate(impactParticleEffect, transform.position, Quaternion.identity);

        if (agent)
        {
            agent.enabled = true;
            agent.Warp(transform.position); // snap safely to navmesh
        }

        if (col) col.enabled = true;
        if (ai) ai.enabled = true;

        health?.UpdateHealth(damage);
        DamageAntsInRange(2f);

        GameObject camera = GameObject.FindWithTag("PlayerCamera");
        if (camera != null)
        {
            CameraFollow cam = camera.GetComponent<CameraFollow>();
            cam?.ShakeAtPosition(transform.position);
        }

        Destroy(this);
    }

    private void DamageAntsInRange(float damageRange)
    {
        foreach (GameObject a in UnitManager.Instance.AllUnits)
        {
            float dist = Vector3.Distance(transform.position, a.transform.position);
            if (dist <= damageRange)
            {
                if (a != this.gameObject && a.GetComponent<SHealth>())
                {
                    a.GetComponent<SHealth>().UpdateHealth(damage / 2);
                }
            }
        }
    }
}
