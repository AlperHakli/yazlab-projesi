using UnityEngine;
using UnityEngine.AI;

public class SecurityAI : MonoBehaviour
{
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    public AIState currentState;
    private NavMeshAgent agent;
    private Animator animator;
    private Health health;
    private HitscanGun gun;
    private Transform player;

    [Header("AI Ayarlarý")]
    public float sightRange = 20f;
    public float attackRange = 10f;
    public LayerMask visionMask;
    public float chaseUpdateInterval = 0.25f;
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 8.0f;
    public float aimRotationOffset = 45.0f;

    [Header("AI Performans")]
    public float decisionUpdateInterval = 0.2f;
    private float decisionUpdateTimer = 0f;

    [Header("Patrol Ayarlarý")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 3f;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private float chaseUpdateTimer = 0f;

    public static bool isAlarmTriggered = false;

    private bool internalCanSeePlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();
        gun = GetComponentInChildren<HitscanGun>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent.speed = patrolSpeed;
        currentState = AIState.Patrol;
        GoToNextPatrolPoint();

        chaseUpdateTimer = Random.Range(0f, chaseUpdateInterval);
        decisionUpdateTimer = Random.Range(0f, decisionUpdateInterval);

        if (animator != null)
        {
            animator.SetBool("isAlerted", false);
        }
    }

    void Update()
    {
        if (health.CurrentHealth <= 0 || player == null)
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
            }
            return;
        }

        chaseUpdateTimer -= Time.deltaTime;
        decisionUpdateTimer -= Time.deltaTime;

        HandleStateTransitions();
        ExecuteCurrentState();
        UpdateAnimation();
    }

    private void HandleStateTransitions()
    {
        if (decisionUpdateTimer <= 0f)
        {
            internalCanSeePlayer = CanSeePlayer();
            decisionUpdateTimer = decisionUpdateInterval;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isAlarmTriggered || internalCanSeePlayer)
        {
            if (distanceToPlayer <= attackRange)
            {
                if (currentState != AIState.Attack)
                {
                    chaseUpdateTimer = -1f;
                    agent.speed = chaseSpeed;
                }
                currentState = AIState.Attack;
            }
            else
            {
                if (currentState != AIState.Chase)
                {
                    chaseUpdateTimer = -1f;
                    agent.speed = chaseSpeed;
                }
                currentState = AIState.Chase;
            }
        }
        else
        {
            if (currentState == AIState.Chase || currentState == AIState.Attack)
            {
                agent.speed = patrolSpeed;
                currentState = AIState.Patrol;
                GoToNextPatrolPoint();
            }
        }
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.Idle:
                IdleState();
                break;
            case AIState.Patrol:
                PatrolState();
                break;
            case AIState.Chase:
                ChaseState();
                break;
            case AIState.Attack:
                AttackState();
                break;
        }
    }

    private void IdleState()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            currentState = AIState.Patrol;
            GoToNextPatrolPoint();
        }
    }

    private void PatrolState()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = AIState.Idle;
            waitTimer = patrolWaitTime;
            agent.SetDestination(transform.position);
        }
    }

    private void ChaseState()
    {
        if (chaseUpdateTimer <= 0f)
        {
            agent.SetDestination(player.position);
            chaseUpdateTimer = chaseUpdateInterval;
        }
    }

    private void AttackState()
    {
        if (chaseUpdateTimer <= 0f)
        {
            agent.SetDestination(player.position);
            chaseUpdateTimer = chaseUpdateInterval;
        }

        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            Quaternion offset = Quaternion.Euler(0, aimRotationOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * offset, Time.deltaTime * agent.angularSpeed);
        }

        if (gun != null)
        {
            if (animator != null) animator.SetTrigger("Attack");
            gun.TryToShoot();
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        bool isAlerted = (currentState == AIState.Chase || currentState == AIState.Attack);

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isAlerted", isAlerted);
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > sightRange)
        {
            return false;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, sightRange, visionMask))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
}