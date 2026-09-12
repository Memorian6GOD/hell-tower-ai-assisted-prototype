using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 240f;
    [SerializeField] private float stoppingDistance = 2.2f;

    [Header("Navigation Settings")]
    [SerializeField] private float repathInterval = 0.1f;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool debugIsPlayerAlive;
    [SerializeField] private bool debugIsOnNavMesh;
    [SerializeField] private bool debugIsChasingPlayer;
    [SerializeField] private bool debugIsAtPlayer;
    [SerializeField] private bool debugIsMovementLocked;
    [SerializeField] private float debugDistanceToPlayer;
    [SerializeField] private float debugRemainingDistance;

    private PlayerHealth playerHealth;
    private NavMeshAgent agent;

    private float repathTimer;

    public bool IsAtPlayer { get; private set; }
    public bool IsMovementLocked { get; private set; }

    public float DistanceToPlayer
    {
        get;
        private set;
    } = Mathf.Infinity;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        playerHealth =
            FindAnyObjectByType<PlayerHealth>();

        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.acceleration = 20f;
        agent.stoppingDistance = stoppingDistance;

        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.updateRotation = false;

        agent.obstacleAvoidanceType =
            ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        agent.avoidancePriority = 10;

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "BossMovement: PlayerHealth was not found."
            );
        }
    }

    private void Update()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
        {
            IsAtPlayer = false;
            StopMovement();
            return;
        }

        if (playerHealth == null ||
            playerHealth.IsDead)
        {
            DistanceToPlayer = Mathf.Infinity;
            IsAtPlayer = false;

            StopMovement();
            return;
        }

        UpdateDistanceToPlayer();

        IsAtPlayer =
            DistanceToPlayer <= stoppingDistance;

        if (IsMovementLocked)
        {
            StopMovement();
            return;
        }

        if (IsAtPlayer)
        {
            StopMovement();
            RotateTowardsPlayer();
            return;
        }

        ChasePlayer();
    }

    private void LateUpdate()
    {
        UpdateDebugInfo();
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            agent.SetDestination(
                playerHealth.transform.position
            );

            repathTimer = repathInterval;
        }

        RotateTowardsMovementDirection();
    }

    private void StopMovement()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void UpdateDistanceToPlayer()
    {
        Vector3 directionToPlayer =
            playerHealth.transform.position -
            transform.position;

        directionToPlayer.y = 0f;

        DistanceToPlayer =
            directionToPlayer.magnitude;
    }

    private void RotateTowardsMovementDirection()
    {
        Vector3 movementDirection =
            agent.desiredVelocity;

        movementDirection.y = 0f;

        RotateTowardsDirection(
            movementDirection
        );
    }

    private void RotateTowardsPlayer()
    {
        if (playerHealth == null)
        {
            return;
        }

        Vector3 directionToPlayer =
            playerHealth.transform.position -
            transform.position;

        directionToPlayer.y = 0f;

        RotateTowardsDirection(
            directionToPlayer
        );
    }

    private void RotateTowardsDirection(
        Vector3 direction
    )
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    public void SetMovementLocked(bool isLocked)
    {
        IsMovementLocked = isLocked;

        if (isLocked)
        {
            StopMovement();
        }
        else
        {
            repathTimer = 0f;
        }
    }

    private void UpdateDebugInfo()
    {
        debugIsPlayerAlive =
            playerHealth != null &&
            !playerHealth.IsDead;

        debugIsOnNavMesh =
            agent != null &&
            agent.isOnNavMesh;

        debugIsChasingPlayer =
            debugIsPlayerAlive &&
            debugIsOnNavMesh &&
            !IsAtPlayer &&
            !IsMovementLocked;

        debugIsAtPlayer = IsAtPlayer;
        debugIsMovementLocked = IsMovementLocked;
        debugDistanceToPlayer = DistanceToPlayer;

        if (agent != null &&
            agent.isOnNavMesh &&
            !float.IsInfinity(
                agent.remainingDistance
            ))
        {
            debugRemainingDistance =
                agent.remainingDistance;
        }
        else
        {
            debugRemainingDistance = 0f;
        }
    }
}