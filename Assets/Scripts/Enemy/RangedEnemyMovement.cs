using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Player Aggro Settings")]
    [SerializeField, Min(0f)]
    private float aggroConfirmationTime = 0.3f;

    [Header("Core Tower Slot Settings")]
    [SerializeField] private float slotArrivalDistance = 0.2f;

    [Header("Navigation Settings")]
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
    [SerializeField] private float debugAggroRadius;
    [SerializeField] private float debugDisengageRadius;

    [SerializeField] private bool debugIsConfirmingAggro;
    [SerializeField] private float debugAggroConfirmationTimer;

    [SerializeField] private float debugRemainingDistance;
    [SerializeField] private float debugVelocity;

    [SerializeField] private Vector3 debugAgentDestination;

    [SerializeField] private NavMeshPathStatus debugPathStatus;

    [SerializeField] private string debugCurrentTarget = "CoreTower";
    [SerializeField] private int debugTargetSwitchCount;
    [SerializeField] private string debugLastTargetSwitch = "None";

    private CoreTowerHealth coreTower;
    private CoreTowerRangedAttackSlots rangedAttackSlots;

    private PlayerHealth playerHealth;
    private PlayerAggro playerAggro;

    private Collider enemyCollider;
    private NavMeshAgent agent;

    private Transform currentCoreTowerSlot;

    private float aggroConfirmationTimer;

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
                coreTower.GetComponent<
                    CoreTowerRangedAttackSlots
                >();
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
                "RangedEnemyMovement: CoreTower " +
                "was not found.",
                this
            );
        }

        if (rangedAttackSlots == null)
        {
            Debug.LogWarning(
                "RangedEnemyMovement: " +
                "CoreTowerRangedAttackSlots " +
                "was not found.",
                this
            );
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "RangedEnemyMovement: PlayerHealth " +
                "was not found.",
                this
            );
        }

        if (playerAggro == null)
        {
            Debug.LogWarning(
                "RangedEnemyMovement: PlayerAggro " +
                "was not found.",
                this
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

    private void BeginTargetingPlayer()
    {
        LogTargetSwitch(
            "CoreTower",
            "Player"
        );

        IsTargetingPlayer = true;
        IsAtTarget = true;

        hasCoreTowerDestination = false;

        agent.isStopped = true;
        agent.ResetPath();

        aggroConfirmationTimer = 0f;
    }

    private void UpdatePlayerTarget()
    {
        IsAtTarget = true;
        agent.isStopped = true;

        RotateTowardsPlayer();
    }

    private void StopTargetingPlayer()
    {
        LogTargetSwitch(
            "Player",
            "CoreTower"
        );

        IsTargetingPlayer = false;
        IsAtTarget = false;

        hasCoreTowerDestination = false;

        agent.isStopped = false;
        agent.ResetPath();

        aggroConfirmationTimer = 0f;
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
        }

        if (currentCoreTowerSlot == null)
        {
            StopAgent();

            return;
        }

        if (!hasCoreTowerDestination)
        {
            SetDestinationToPoint(
                currentCoreTowerSlot.position
            );
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

        if (float.IsInfinity(
                agent.remainingDistance
            ))
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
        bool sampleSucceeded =
            NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit hit,
                navMeshSampleDistance,
                agent.areaMask
            );

        if (!sampleSucceeded)
        {
            hasCoreTowerDestination = false;

            Debug.LogWarning(
                $"{name}: Could not find a NavMesh " +
                "point for the assigned ranged " +
                "CoreTower slot.",
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
                $"{name}: NavMeshAgent rejected " +
                "the ranged CoreTower slot " +
                "destination.",
                this
            );
        }
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

    private void LogTargetSwitch(
        string oldTarget,
        string newTarget
    )
    {
        debugTargetSwitchCount++;

        debugCurrentTarget =
            newTarget;

        debugLastTargetSwitch =
            $"{oldTarget} -> {newTarget}";

        float distanceToPlayer =
            GetCurrentDistanceToPlayer();

        float aggroRadius =
            playerAggro != null
                ? playerAggro.AggroRadius
                : 0f;

        float disengageRadius =
            playerAggro != null
                ? playerAggro.DisengageRadius
                : 0f;

        Debug.Log(
            $"{name}: ranged target changed " +
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

        debugHasCoreTowerSlot =
            currentCoreTowerSlot != null;

        debugHasCoreTowerDestination =
            hasCoreTowerDestination;

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