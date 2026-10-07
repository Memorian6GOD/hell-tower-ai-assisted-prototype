using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Player Attack Position Settings")]
    [SerializeField] private float stopGap = 0.15f;
    [SerializeField] private float arrivalTolerance = 0.25f;
    [SerializeField] private float resumeExtraDistance = 0.45f;

    [Header("Player Aggro Settings")]
    [SerializeField, Min(0f)]
    private float aggroConfirmationTime = 0.3f;

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

    [SerializeField] private bool debugHasPlayerDestination;
    [SerializeField] private bool debugHasPath;
    [SerializeField] private bool debugPathPending;

    [SerializeField] private float debugDistanceToPlayer;
    [SerializeField] private float debugAggroRadius;
    [SerializeField] private float debugDisengageRadius;

    [SerializeField] private bool debugIsConfirmingAggro;
    [SerializeField] private float debugAggroConfirmationTimer;

    [SerializeField] private float debugRemainingDistance;
    [SerializeField] private float debugStoppingDistance;
    [SerializeField] private float debugVelocity;

    [SerializeField] private Vector3 debugAgentDestination;

    [SerializeField] private NavMeshPathStatus debugPathStatus;

    [SerializeField] private bool debugLastPlayerSampleSuccess;
    [SerializeField] private bool debugLastPlayerDestinationAccepted;

    [SerializeField] private string debugCurrentTarget = "CoreTower";
    [SerializeField] private int debugTargetSwitchCount;
    [SerializeField] private string debugLastTargetSwitch = "None";

    private CoreTowerHealth coreTower;
    private Collider coreTowerCollider;
    private CoreTowerAttackSlots coreTowerAttackSlots;

    private PlayerHealth playerHealth;
    private PlayerAggro playerAggro;
    private Collider playerCollider;

    private Collider enemyCollider;
    private NavMeshAgent agent;

    private Transform currentCoreTowerSlot;

    private float repathTimer;
    private float aggroConfirmationTimer;

    private bool hasCoreTowerDestination;
    private bool hasPlayerDestination;

    public bool IsAtTarget { get; private set; }
    public bool IsTargetingPlayer { get; private set; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();

        coreTower = FindAnyObjectByType<CoreTowerHealth>();

        if (coreTower != null)
        {
            coreTowerCollider =
                coreTower.GetComponent<Collider>();

            coreTowerAttackSlots =
                coreTower.GetComponent<CoreTowerAttackSlots>();
        }

        playerHealth =
            FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerAggro =
                playerHealth.GetComponent<PlayerAggro>();

            playerCollider =
                playerHealth.GetComponent<Collider>();
        }

        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.acceleration = 20f;
        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.updateRotation = true;

        agent.obstacleAvoidanceType =
            ObstacleAvoidanceType.NoObstacleAvoidance;

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
                "EnemyMovement: CoreTower was not found."
            );
        }

        if (coreTowerCollider == null)
        {
            Debug.LogWarning(
                "EnemyMovement: CoreTower Collider was not found."
            );
        }

        if (coreTowerAttackSlots == null)
        {
            Debug.LogWarning(
                "EnemyMovement: CoreTowerAttackSlots was not found."
            );
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "EnemyMovement: PlayerHealth was not found."
            );
        }

        if (playerAggro == null)
        {
            Debug.LogWarning(
                "EnemyMovement: PlayerAggro was not found."
            );
        }

        if (playerCollider == null)
        {
            Debug.LogWarning(
                "EnemyMovement: Player Collider was not found."
            );
        }

        if (playerAggro != null)
        {
            debugAggroRadius =
                playerAggro.AggroRadius;

            debugDisengageRadius =
                playerAggro.DisengageRadius;
        }

        aggroConfirmationTimer = 0f;

        debugCurrentTarget = "CoreTower";
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
                LogTargetSwitch(
                    "CoreTower",
                    "Player"
                );

                ReleaseCoreTowerSlot();

                IsAtTarget = false;
                hasPlayerDestination = false;

                agent.isStopped = false;
                agent.ResetPath();

                repathTimer = 0f;
                aggroConfirmationTimer = 0f;
            }

            IsTargetingPlayer = true;

            UpdatePlayerTarget();

            return;
        }

        if (IsTargetingPlayer)
        {
            LogTargetSwitch(
                "Player",
                "CoreTower"
            );

            IsTargetingPlayer = false;
            IsAtTarget = false;
            hasPlayerDestination = false;

            agent.isStopped = false;
            agent.ResetPath();

            repathTimer = 0f;
            aggroConfirmationTimer = 0f;
            hasCoreTowerDestination = false;
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
            playerAggro == null ||
            playerCollider == null)
        {
            ResetAggroConfirmation();

            return false;
        }

        if (playerHealth.IsDead)
        {
            ResetAggroConfirmation();

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
            ResetAggroConfirmation();

            return distanceToPlayer <=
                   playerAggro.DisengageRadius;
        }

        if (distanceToPlayer >
            playerAggro.AggroRadius)
        {
            ResetAggroConfirmation();

            return false;
        }

        if (aggroConfirmationTime <= 0f)
        {
            aggroConfirmationTimer = 0f;

            return true;
        }

        aggroConfirmationTimer +=
            Time.deltaTime;

        if (aggroConfirmationTimer >=
            aggroConfirmationTime)
        {
            aggroConfirmationTimer =
                aggroConfirmationTime;

            return true;
        }

        return false;
    }

    private void ResetAggroConfirmation()
    {
        aggroConfirmationTimer = 0f;
    }

    private void UpdatePlayerTarget()
    {
        if (playerCollider == null)
        {
            StopAgent();
            return;
        }

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            hasPlayerDestination =
                SetDestinationToPlayer();

            repathTimer = repathInterval;
        }

        if (!hasPlayerDestination)
        {
            IsAtTarget = false;
            agent.isStopped = false;
            return;
        }

        if (agent.pathPending)
        {
            return;
        }

        if (float.IsInfinity(agent.remainingDistance))
        {
            IsAtTarget = false;
            agent.isStopped = false;
            return;
        }

        float attackDistance =
            agent.stoppingDistance +
            arrivalTolerance;

        float resumeDistance =
            agent.stoppingDistance +
            resumeExtraDistance;

        if (IsAtTarget)
        {
            if (agent.remainingDistance >
                resumeDistance)
            {
                IsAtTarget = false;
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;

                RotateTowardsPlayer();

                return;
            }
        }

        if (agent.remainingDistance <=
            attackDistance)
        {
            IsAtTarget = true;
            agent.isStopped = true;

            RotateTowardsPlayer();

            return;
        }

        IsAtTarget = false;
        agent.isStopped = false;
    }

    private bool SetDestinationToPlayer()
    {
        if (playerCollider == null)
        {
            debugLastPlayerSampleSuccess = false;
            debugLastPlayerDestinationAccepted = false;

            return false;
        }

        Vector3 targetPosition =
            playerCollider.bounds.center;

        targetPosition.y =
            transform.position.y;

        bool sampleSuccess =
            NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit hit,
                navMeshSampleDistance,
                agent.areaMask
            );

        debugLastPlayerSampleSuccess =
            sampleSuccess;

        if (!sampleSuccess)
        {
            debugLastPlayerDestinationAccepted = false;

            return false;
        }

        agent.stoppingDistance =
            GetDesiredCenterDistance(
                playerCollider
            );

        bool destinationAccepted =
            agent.SetDestination(
                hit.position
            );

        debugLastPlayerDestinationAccepted =
            destinationAccepted;

        return destinationAccepted;
    }

    private void UpdateCoreTowerTarget()
    {
        if (coreTower == null ||
            coreTowerAttackSlots == null)
        {
            StopAgent();
            return;
        }

        if (currentCoreTowerSlot == null)
        {
            currentCoreTowerSlot =
                coreTowerAttackSlots.ClaimClosestSlot(this);

            IsAtTarget = false;
            hasCoreTowerDestination = false;

            if (currentCoreTowerSlot != null)
            {
                SetDestinationToPoint(
                    currentCoreTowerSlot.position
                );
            }
        }

        if (currentCoreTowerSlot == null)
        {
            StopAgent();
            return;
        }

        if (!hasCoreTowerDestination)
        {
            IsAtTarget = false;
            agent.isStopped = false;
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

            Debug.LogWarning(
                $"{name}: Could not find a NavMesh point " +
                "for the assigned CoreTower slot.",
                this
            );

            return;
        }

        agent.stoppingDistance = 0f;

        bool destinationAccepted =
            agent.SetDestination(
                hit.position
            );

        hasCoreTowerDestination =
            destinationAccepted;

        if (!destinationAccepted)
        {
            Debug.LogWarning(
                $"{name}: NavMeshAgent rejected the " +
                "CoreTower slot destination.",
                this
            );
        }
    }

    private float GetDesiredCenterDistance(
        Collider targetCollider
    )
    {
        float enemyRadius =
            GetEnemyHorizontalRadius();

        float targetRadius =
            GetTargetHorizontalRadius(
                targetCollider
            );

        return
            enemyRadius +
            targetRadius +
            stopGap;
    }

    private float GetEnemyHorizontalRadius()
    {
        if (enemyCollider == null)
        {
            return agent != null
                ? agent.radius
                : 0f;
        }

        Vector3 extents =
            enemyCollider.bounds.extents;

        return Mathf.Max(
            extents.x,
            extents.z
        );
    }

    private float GetTargetHorizontalRadius(
        Collider targetCollider
    )
    {
        if (targetCollider == null)
        {
            return 0f;
        }

        Vector3 extents =
            targetCollider.bounds.extents;

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

    private void ReleaseCoreTowerSlot()
    {
        if (coreTowerAttackSlots != null)
        {
            coreTowerAttackSlots.ReleaseSlot(this);
        }

        currentCoreTowerSlot = null;
        hasCoreTowerDestination = false;
    }

    private void StopAgent()
    {
        IsAtTarget = false;
        hasCoreTowerDestination = false;
        hasPlayerDestination = false;

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void LogTargetSwitch(
        string oldTarget,
        string newTarget
    )
    {
        float distanceToPlayer =
            GetCurrentDistanceToPlayer();

        debugTargetSwitchCount++;

        debugCurrentTarget =
            newTarget;

        debugLastTargetSwitch =
            $"{oldTarget} -> {newTarget}";

        float aggroRadius =
            playerAggro != null
                ? playerAggro.AggroRadius
                : 0f;

        float disengageRadius =
            playerAggro != null
                ? playerAggro.DisengageRadius
                : 0f;

        Debug.Log(
            $"{name}: target changed " +
            $"{oldTarget} -> {newTarget}. " +
            $"Distance to Player = " +
            $"{distanceToPlayer:F2}. " +
            $"Aggro Radius = " +
            $"{aggroRadius:F2}. " +
            $"Disengage Radius = " +
            $"{disengageRadius:F2}. " +
            $"Aggro confirmation = " +
            $"{aggroConfirmationTime:F2}s. " +
            $"Switch count = " +
            $"{debugTargetSwitchCount}.",
            this
        );
    }

    private float GetCurrentDistanceToPlayer()
    {
        if (playerHealth == null)
        {
            return -1f;
        }

        Vector3 direction =
            playerHealth.transform.position -
            transform.position;

        direction.y = 0f;

        return direction.magnitude;
    }

    private void UpdateDebugInfo()
    {
        debugIsTargetingPlayer =
            IsTargetingPlayer;

        debugIsAtTarget =
            IsAtTarget;

        debugHasPlayerDestination =
            hasPlayerDestination;

        debugCurrentTarget =
            IsTargetingPlayer
                ? "Player"
                : "CoreTower";

        if (playerAggro != null)
        {
            debugAggroRadius =
                playerAggro.AggroRadius;

            debugDisengageRadius =
                playerAggro.DisengageRadius;
        }

        debugAggroConfirmationTimer =
            aggroConfirmationTimer;

        debugIsConfirmingAggro =
            !IsTargetingPlayer &&
            aggroConfirmationTimer > 0f;

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

        debugStoppingDistance =
            agent.stoppingDistance;

        debugVelocity =
            agent.velocity.magnitude;

        debugAgentDestination =
            agent.destination;

        debugPathStatus =
            agent.pathStatus;

        debugDistanceToPlayer =
            GetCurrentDistanceToPlayer();
    }

    private void OnDisable()
    {
        ResetAggroConfirmation();
        ReleaseCoreTowerSlot();
    }

    private void OnValidate()
    {
        aggroConfirmationTime =
            Mathf.Max(
                0f,
                aggroConfirmationTime
            );
    }
}