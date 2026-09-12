using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SiegeEnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 270f;

    [Header("Attack Position Settings")]
    [SerializeField] private float stopGap = 0.15f;
    [SerializeField] private float arrivalTolerance = 0.25f;

    [Header("Navigation Settings")]
    [SerializeField] private float repathInterval = 0.1f;
    [SerializeField] private float targetSearchInterval = 0.25f;
    [SerializeField] private float navMeshSampleDistance = 2f;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private string debugCurrentTarget;
    [SerializeField] private bool debugIsTargetingDefenseTower;
    [SerializeField] private bool debugIsAtTarget;
    [SerializeField] private bool debugIsOnNavMesh;
    [SerializeField] private bool debugAgentIsStopped;
    [SerializeField] private bool debugHasPath;
    [SerializeField] private bool debugPathPending;
    [SerializeField] private float debugRemainingDistance;
    [SerializeField] private float debugStoppingDistance;
    [SerializeField] private float debugVelocity;
    [SerializeField] private NavMeshPathStatus debugPathStatus;

    private NavMeshAgent agent;
    private Collider enemyCollider;

    private DefenseTowerHealth currentDefenseTower;
    private Collider currentDefenseTowerCollider;

    private CoreTowerHealth coreTower;
    private Collider coreTowerCollider;

    private float repathTimer;
    private float targetSearchTimer;

    public bool IsAtTarget { get; private set; }

    public bool IsTargetingDefenseTower
    {
        get
        {
            return currentDefenseTower != null &&
                   !currentDefenseTower.IsDestroyed;
        }
    }

    public DefenseTowerHealth CurrentDefenseTower
    {
        get
        {
            return currentDefenseTower;
        }
    }

    public CoreTowerHealth CurrentCoreTower
    {
        get
        {
            return coreTower;
        }
    }

    public Collider CurrentTargetCollider
    {
        get
        {
            if (IsTargetingDefenseTower)
            {
                return currentDefenseTowerCollider;
            }

            return coreTowerCollider;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        FindCoreTower();
        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.acceleration = 20f;
        agent.autoBraking = true;
        agent.autoRepath = true;

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.updateUpAxis = true;

        agent.isStopped = false;

        agent.obstacleAvoidanceType =
            ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        agent.avoidancePriority =
            Random.Range(20, 81);

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

        targetSearchTimer = 0f;
        repathTimer = 0f;

        if (enemyCollider == null)
        {
            Debug.LogWarning(
                "SiegeEnemyMovement: Enemy Collider was not found."
            );
        }

        if (coreTower == null)
        {
            Debug.LogWarning(
                "SiegeEnemyMovement: CoreTower was not found."
            );
        }

        if (coreTowerCollider == null)
        {
            Debug.LogWarning(
                "SiegeEnemyMovement: CoreTower Collider was not found."
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

        UpdateDefenseTowerSearch();

        if (IsTargetingDefenseTower)
        {
            UpdateCurrentTarget(
                currentDefenseTower.transform,
                currentDefenseTowerCollider
            );

            return;
        }

        UpdateCoreTowerTarget();
    }

    private void LateUpdate()
    {
        UpdateDebugInfo();
    }

    private void FindCoreTower()
    {
        coreTower =
            FindAnyObjectByType<CoreTowerHealth>();

        if (coreTower == null)
        {
            return;
        }

        coreTowerCollider =
            coreTower.GetComponent<Collider>();

        if (coreTowerCollider == null)
        {
            coreTowerCollider =
                coreTower.GetComponentInChildren<Collider>();
        }
    }

    private void UpdateDefenseTowerSearch()
    {
        if (currentDefenseTower != null &&
            !currentDefenseTower.IsDestroyed)
        {
            return;
        }

        ClearDefenseTowerTarget();

        targetSearchTimer -= Time.deltaTime;

        if (targetSearchTimer > 0f)
        {
            return;
        }

        FindNearestDefenseTower();

        targetSearchTimer =
            targetSearchInterval;
    }

    private void FindNearestDefenseTower()
    {
        DefenseTowerHealth[] defenseTowers =
            FindObjectsByType<DefenseTowerHealth>();

        DefenseTowerHealth nearestTower = null;
        Collider nearestTowerCollider = null;

        float nearestDistanceSqr =
            float.MaxValue;

        foreach (DefenseTowerHealth tower
                 in defenseTowers)
        {
            if (tower == null ||
                tower.IsDestroyed)
            {
                continue;
            }

            Collider towerCollider =
                tower.GetComponent<Collider>();

            if (towerCollider == null)
            {
                towerCollider =
                    tower.GetComponentInChildren<Collider>();
            }

            if (towerCollider == null)
            {
                continue;
            }

            Vector3 closestPoint =
                towerCollider.ClosestPoint(
                    transform.position
                );

            Vector3 direction =
                closestPoint -
                transform.position;

            direction.y = 0f;

            float distanceSqr =
                direction.sqrMagnitude;

            if (distanceSqr <
                nearestDistanceSqr)
            {
                nearestDistanceSqr =
                    distanceSqr;

                nearestTower = tower;
                nearestTowerCollider =
                    towerCollider;
            }
        }

        if (nearestTower == null)
        {
            return;
        }

        SetDefenseTowerTarget(
            nearestTower,
            nearestTowerCollider
        );
    }

    private void SetDefenseTowerTarget(
        DefenseTowerHealth defenseTower,
        Collider defenseTowerCollider
    )
    {
        ClearDefenseTowerTarget();

        currentDefenseTower =
            defenseTower;

        currentDefenseTowerCollider =
            defenseTowerCollider;

        currentDefenseTower.Destroyed +=
            HandleDefenseTowerDestroyed;

        IsAtTarget = false;
        repathTimer = 0f;

        agent.isStopped = false;
        agent.ResetPath();

        Debug.Log(
            gameObject.name +
            " selected siege target: " +
            currentDefenseTower.gameObject.name
        );
    }

    private void HandleDefenseTowerDestroyed(
        DefenseTowerHealth destroyedTower
    )
    {
        if (destroyedTower !=
            currentDefenseTower)
        {
            return;
        }

        ClearDefenseTowerTarget();

        targetSearchTimer = 0f;
        repathTimer = 0f;
    }

    private void ClearDefenseTowerTarget()
    {
        if (currentDefenseTower != null)
        {
            currentDefenseTower.Destroyed -=
                HandleDefenseTowerDestroyed;
        }

        currentDefenseTower = null;
        currentDefenseTowerCollider = null;

        IsAtTarget = false;
    }

    private void UpdateCoreTowerTarget()
    {
        if (coreTower == null ||
            coreTower.IsDestroyed)
        {
            StopAgent();
            return;
        }

        if (coreTowerCollider == null)
        {
            StopAgent();
            return;
        }

        UpdateCurrentTarget(
            coreTower.transform,
            coreTowerCollider
        );
    }

    private void UpdateCurrentTarget(
        Transform targetTransform,
        Collider targetCollider
    )
    {
        if (targetTransform == null ||
            targetCollider == null)
        {
            StopAgent();
            return;
        }

        if (HasReachedTarget(
            targetCollider))
        {
            IsAtTarget = true;

            agent.isStopped = true;

            if (agent.hasPath)
            {
                agent.ResetPath();
            }

            RotateTowards(
                targetTransform.position
            );

            return;
        }

        IsAtTarget = false;
        agent.isStopped = false;

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            SetDestinationNearTarget(
                targetCollider
            );

            repathTimer = repathInterval;
        }
    }

    private bool HasReachedTarget(
        Collider targetCollider
    )
    {
        Vector3 closestPoint =
            targetCollider.ClosestPoint(
                transform.position
            );

        Vector3 direction =
            closestPoint -
            transform.position;

        direction.y = 0f;

        float desiredDistance =
            GetEnemyHorizontalRadius() +
            stopGap +
            arrivalTolerance;

        return direction.magnitude <=
               desiredDistance;
    }

    private void SetDestinationNearTarget(
        Collider targetCollider
    )
    {
        Vector3 closestPoint =
            targetCollider.ClosestPoint(
                transform.position
            );

        Vector3 outwardDirection =
            transform.position -
            closestPoint;

        outwardDirection.y = 0f;

        if (outwardDirection.sqrMagnitude <
            0.001f)
        {
            outwardDirection =
                transform.position -
                targetCollider.bounds.center;

            outwardDirection.y = 0f;
        }

        if (outwardDirection.sqrMagnitude <
            0.001f)
        {
            outwardDirection =
                -transform.forward;
        }

        outwardDirection.Normalize();

        Vector3 desiredPosition =
            closestPoint +
            outwardDirection *
            (
                GetEnemyHorizontalRadius() +
                stopGap
            );

        if (!NavMesh.SamplePosition(
            desiredPosition,
            out NavMeshHit hit,
            navMeshSampleDistance,
            agent.areaMask))
        {
            agent.ResetPath();
            return;
        }

        agent.stoppingDistance =
            arrivalTolerance;

        agent.SetDestination(
            hit.position
        );
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

    private void RotateTowards(
        Vector3 targetPosition
    )
    {
        Vector3 direction =
            targetPosition -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    private void StopAgent()
    {
        IsAtTarget = false;

        if (agent == null ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void UpdateDebugInfo()
    {
        debugIsTargetingDefenseTower =
            IsTargetingDefenseTower;

        debugIsAtTarget =
            IsAtTarget;

        if (IsTargetingDefenseTower)
        {
            debugCurrentTarget =
                currentDefenseTower.gameObject.name;
        }
        else if (coreTower != null &&
                 !coreTower.IsDestroyed)
        {
            debugCurrentTarget =
                coreTower.gameObject.name;
        }
        else
        {
            debugCurrentTarget =
                "None";
        }

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

        debugPathStatus =
            agent.pathStatus;
    }

    private void OnDisable()
    {
        ClearDefenseTowerTarget();
    }
}