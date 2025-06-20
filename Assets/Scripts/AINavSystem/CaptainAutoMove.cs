using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CaptainAutoMove : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(target.position);
    }

    void Update()
    {
        if (agent.remainingDistance > 0.1f)
        {
            GetComponent<Animator>().SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            GetComponent<Animator>().SetFloat("Speed", 0);
        }
    }
}
