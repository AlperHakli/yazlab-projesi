using UnityEngine;
using UnityEngine.AI;

public class VisitorAI : MonoBehaviour
{
    [Header("Dolaþma Ayarlarý")]
    public float wanderRadius = 10f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    private NavMeshAgent agent;
    private Animator animator;


    private enum AIState
    {
        Walking,
        Idle
    }
    private AIState currentState;
    private float idleTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        agent.avoidancePriority = Random.Range(0, 1000);


        currentState = AIState.Idle;
        idleTimer = Random.Range(minWaitTime, maxWaitTime);


    }

    void Update()
    {

        if (currentState == AIState.Idle)
        {

            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                StartWalkingState(); 
            }
        }
        else if (currentState == AIState.Walking)
        {

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {

                StartIdleState();
            }

            else if (!agent.pathPending && (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid))
            {

                StartIdleState();
            }
        }


        UpdateAnimation();
    }

    void GoToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }


    void UpdateAnimation()
    {
        if (animator == null) return;


        float intendedSpeed = agent.desiredVelocity.magnitude;


        animator.SetBool("isWalking", intendedSpeed > 0.1f);
    }


    void StartIdleState()
    {
        currentState = AIState.Idle;
        idleTimer = Random.Range(minWaitTime, maxWaitTime);

    }

    void StartWalkingState()
    {
        currentState = AIState.Walking;
        GoToRandomPoint();
    }
}