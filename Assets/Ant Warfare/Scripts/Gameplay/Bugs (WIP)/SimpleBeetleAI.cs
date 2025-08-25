using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBeetleAI : MonoBehaviour
{

    [Header("Movement")]
    public Vector3 target = new Vector3(0, 0, 0);
    UnityEngine.AI.NavMeshAgent agent;

    [Header("Other")]
    private GameObject[] Ants;

    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        Ants = GameObject.FindGameObjectsWithTag("Ant");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}
