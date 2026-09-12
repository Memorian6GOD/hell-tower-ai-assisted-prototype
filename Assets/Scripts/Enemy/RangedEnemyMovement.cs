using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Core Tower Slot Settings")]
    [SerializeField] private float slotArrivalDistance = 0.2f;

    [Header("Navigation Settings")]
    [SerializeField] private float repathInterval = 0.1f;
    [SerializeField] private float navMeshSampleDistance = 2f;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool debugIsOnNavMesh;
    [SerializeField] private bool debugIsTargetingPlayer;
    [SerializeField] private bool debugIsAtTarget;
    [SerializeField] private bool debugAgentIsStopped;
    [SerializeField] private bool debugHasCoreTowerSlot;
    [SerializeField] private bool debugHasCoreTowerDestination;
    [SerializeField] private bool debugHasPath;
    [SerializeField] private bool debugPathPending;

    [SerializeField] private float debugDistanceToPlayer;
    [SerializeField] private float debugRemainingDistance;
    [SerializeField] private float debugVelocity;

    [SerializeField] private Vector3 debugAgentDestination;
    [SerializeField] private NavMeshPathStatus debugPathStatus;

    private CoreTowerHealth coreTower;
    private CoreTowerRangedAttackSlots rangedAttackSlots;

    private PlayerHealth playerHealth;
    private PlayerAggro playerAggro;

    private Collider enemyCollider;
    private NavMeshAgent agent;

    private Transform currentCoreTowerSlot;

    private float repathTimer;
    private bool hasCoreTowerDestination;

    public bool IsAtTarget { get; private set; }
    public bool IsTargetingPlayer { get; private set; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();

        coreTower =
            FindAnyObjectByType<CoreTowerHealth>();

        if (coreTower != null)
        {
            rangedAttackSlots =
                coreTower.GetComponent<CoreTowerRangedAttackSlots>();
        }

        playerHealth =
            FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerAggro =
                playerHealth.GetComponent<PlayerAggro>();
        }

        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.acceleration = 20f;
        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.updateRotation = true;

        agent.obstacleAvoidanceType =
            ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        agent.avoidancePriority =
            Random.Range(20, 81);

        if (enemyCollider != null)
        {
            float enemyRadius =
                GetEnemyHorizontalRadius();

            if (enemyRadius > 0f)
            {
                agent.radius = enemyRadius;
            }
        }

        if (coreTower == null)
        {
            Debug.LogWarning(
                "RangedEnemyMovement: CoreTower was not found."
            );
        }

        if (rangedAttackSlots == null)
        {
            Debug.LogWarning(
                "RangedEnemyMovement: CoreTowerRangedAttackSlots was not found."
            );
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "RangedEnemyMovement: PlayerHealth was not found."
            );
        }

        if (playerAggro == null)
        {
            Debug.LogWarning(
                "RangedEnemyMovement: PlayerAggro was not found."
            );
        }
    }

    private void Update()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
        {
            IsAtTarget = false;

            return;
        }

        bool shouldTargetPlayer =
            ShouldTargetPlayer();

        if (shouldTargetPlayer)
        {
            if (!IsTargetingPlayer)
            {
                BeginTargetingPlayer();
            }

            UpdatePlayerTarget();

            return;
        }

        if (IsTargetingPlayer)
        {
            StopTargetingPlayer();
        }

        UpdateCoreTowerTarget();
    }

    private void LateUpdate()
    {
        UpdateDebugInfo();
    }

    private bool ShouldTargetPlayer()
    {
        if (playerHealth == null ||
            playerAggro == null)
        {
            return false;
        }

        if (playerHealth.IsDead)
        {
            return false;
        }

        Vector3 directionToPlayer =
            playerHealth.transform.position -
            transform.position;

        directionToPlayer.y = 0f;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        if (IsTargetingPlayer)
        {
            return distanceToPlayer <=
                   playerAggro.DisengageRadius;
        }

        return distanceToPlayer <=
               playerAggro.AggroRadius;
    }

    private void BeginTargetingPlayer()
    {
        IsTargetingPlayer = true;
        IsAtTarget = true;

        hasCoreTowerDestination = false;
        repathTimer = 0f;

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void UpdatePlayerTarget()
    {
        IsAtTarget = true;
        agent.isStopped = true;

        RotateTowardsPlayer();
    }

    private void StopTargetingPlayer()
    {
        IsTargetingPlayer = false;
        IsAtTarget = false;

        hasCoreTowerDestination = false;
        repathTimer = 0f;

        agent.isStopped = false;
        agent.ResetPath();
    }

    private void UpdateCoreTowerTarget()
    {
        if (coreTower == null ||
            rangedAttackSlots == null)
        {
            StopAgent();

            return;
        }

        if (currentCoreTowerSlot == null)
        {
            currentCoreTowerSlot =
                rangedAttackSlots.ClaimClosestSlot(
                    gameObject
                );

            IsAtTarget = false;
            hasCoreTowerDestination = false;
            repathTimer = 0f;
        }

        if (currentCoreTowerSlot == null)
        {
            StopAgent();

            return;
        }

        if (HasReachedCoreTowerSlot())
        {
            IsAtTarget = true;
            agent.isStopped = true;

            RotateTowardsCoreTower();

            return;
        }

        IsAtTarget = false;
        agent.isStopped = false;

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            SetDestinationToPoint(
                currentCoreTowerSlot.position
            );

            repathTimer = repathInterval;
        }
    }

    private bool HasReachedCoreTowerSlot()
    {
        if (!hasCoreTowerDestination)
        {
            return false;
        }

        if (agent.pathPending)
        {
            return false;
        }

        if (float.IsInfinity(agent.remainingDistance))
        {
            return false;
        }

        return agent.remainingDistance <=
               slotArrivalDistance;
    }

    private void SetDestinationToPoint(
        Vector3 targetPosition
    )
    {
        if (!NavMesh.SamplePosition(
            targetPosition,
            out NavMeshHit hit,
            navMeshSampleDistance,
            agent.areaMask))
        {
            hasCoreTowerDestination = false;

            return;
        }

        agent.stoppingDistance = 0f;

        bool destinationAccepted =
            agent.SetDestination(
                hit.position
            );

        hasCoreTowerDestination =
            destinationAccepted;
    }

    private float GetEnemyHorizontalRadius()
    {
        if (enemyCollider == null)
        {
            if (agent != null)
            {
                return agent.radius;
            }

            return 0f;
        }

        Vector3 extents =
            enemyCollider.bounds.extents;

        return Mathf.Max(
            extents.x,
            extents.z
        );
    }

    private void RotateTowardsPlayer()
    {
        if (playerHealth == null)
        {
            return;
        }

        Vector3 direction =
            playerHealth.transform.position -
            transform.position;

        direction.y = 0f;

        RotateTowardsDirection(direction);
    }

    private void RotateTowardsCoreTower()
    {
        if (coreTower == null)
        {
            return;
        }

        Vector3 direction =
            coreTower.transform.position -
            transform.position;

        direction.y = 0f;

        RotateTowardsDirection(direction);
    }

    private void RotateTowardsDirection(
        Vector3 direction
    )
    {
        if (direction.sqrMagnitude < 0.001f)
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

    private void StopAgent()
    {
        IsAtTarget = false;
        hasCoreTowerDestination = false;

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void ReleaseCoreTowerSlot()
    {
        if (rangedAttackSlots != null)
        {
            rangedAttackSlots.ReleaseSlot(
                gameObject
            );
        }

        currentCoreTowerSlot = null;
        hasCoreTowerDestination = false;
    }

    private void UpdateDebugInfo()
    {
        debugIsTargetingPlayer =
            IsTargetingPlayer;

        debugIsAtTarget =
            IsAtTarget;

        debugHasCoreTowerSlot =
            currentCoreTowerSlot != null;

        debugHasCoreTowerDestination =
            hasCoreTowerDestination;

        if (agent == null)
        {
            debugIsOnNavMesh = false;

            return;
        }

        debugIsOnNavMesh =
            agent.isOnNavMesh;

        if (!agent.isOnNavMesh)
        {
            return;
        }

        debugAgentIsStopped =
            agent.isStopped;

        debugHasPath =
            agent.hasPath;

        debugPathPending =
            agent.pathPending;

        debugRemainingDistance =
            agent.remainingDistance;

        debugVelocity =
            agent.velocity.magnitude;

        debugAgentDestination =
            agent.destination;

        debugPathStatus =
            agent.pathStatus;

        if (playerHealth != null)
        {
            Vector3 direction =
                playerHealth.transform.position -
                transform.position;

            direction.y = 0f;

            debugDistanceToPlayer =
                direction.magnitude;
        }
    }

    private void OnDisable()
    {
        ReleaseCoreTowerSlot();
    }
}