using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Target Distance Settings")]
    [SerializeField] private float stopGap = 0.15f;
    [SerializeField] private float resumeGap = 0.35f;

    [Header("Navigation Settings")]
    [SerializeField] private float repathInterval = 0.1f;
    [SerializeField] private float navMeshSampleDistance = 2f;

    private CoreTowerHealth coreTower;
    private Collider coreTowerCollider;

    private PlayerHealth playerHealth;
    private PlayerAggro playerAggro;
    private Collider playerCollider;

    private Collider enemyCollider;
    private Collider currentTargetCollider;

    private NavMeshAgent agent;

    private float repathTimer;

    public bool IsAtTarget { get; private set; }
    public bool IsTargetingPlayer { get; private set; }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();

        coreTower = FindAnyObjectByType<CoreTowerHealth>();

        if (coreTower != null)
        {
            coreTowerCollider =
                coreTower.GetComponent<Collider>();
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

        agent.obstacleAvoidanceType =
            ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        agent.avoidancePriority =
            Random.Range(40, 61);

        if (resumeGap < stopGap)
        {
            resumeGap = stopGap;
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

        if (enemyCollider == null)
        {
            Debug.LogWarning(
                "EnemyMovement: Enemy Collider was not found."
            );
        }
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            IsAtTarget = false;
            return;
        }

        Collider desiredTargetCollider;

        if (ShouldTargetPlayer())
        {
            IsTargetingPlayer = true;
            desiredTargetCollider = playerCollider;
        }
        else
        {
            IsTargetingPlayer = false;
            desiredTargetCollider = coreTowerCollider;
        }

        if (desiredTargetCollider == null)
        {
            StopAgent();
            currentTargetCollider = null;
            return;
        }

        if (currentTargetCollider != desiredTargetCollider)
        {
            currentTargetCollider = desiredTargetCollider;

            IsAtTarget = false;
            repathTimer = 0f;

            agent.isStopped = false;
        }

        UpdateTargetDistance();

        if (IsAtTarget)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            UpdateDestination();
            repathTimer = repathInterval;
        }
    }

    private bool ShouldTargetPlayer()
    {
        if (playerHealth == null ||
            playerAggro == null ||
            playerCollider == null)
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

    private void UpdateTargetDistance()
    {
        if (currentTargetCollider == null)
        {
            IsAtTarget = false;
            return;
        }

        float gap =
            GetGapToTarget(currentTargetCollider);

        if (IsAtTarget)
        {
            if (gap > resumeGap)
            {
                IsAtTarget = false;
            }

            return;
        }

        if (gap <= stopGap)
        {
            IsAtTarget = true;
        }
    }

    private float GetGapToTarget(
        Collider targetCollider
    )
    {
        Vector3 enemyCenter =
            transform.position;

        if (enemyCollider != null)
        {
            enemyCenter =
                enemyCollider.bounds.center;
        }

        Vector3 targetPoint =
            targetCollider.ClosestPoint(enemyCenter);

        Vector3 direction =
            targetPoint - enemyCenter;

        direction.y = 0f;

        float centerToTarget =
            direction.magnitude;

        float enemyRadius =
            GetEnemyHorizontalRadius();

        return Mathf.Max(
            0f,
            centerToTarget - enemyRadius
        );
    }

    private void UpdateDestination()
    {
        if (currentTargetCollider == null)
        {
            return;
        }

        Vector3 enemyCenter =
            transform.position;

        if (enemyCollider != null)
        {
            enemyCenter =
                enemyCollider.bounds.center;
        }

        Vector3 targetPoint =
            currentTargetCollider.ClosestPoint(
                enemyCenter
            );

        if (NavMesh.SamplePosition(
            targetPoint,
            out NavMeshHit hit,
            navMeshSampleDistance,
            agent.areaMask))
        {
            float enemyRadius =
                GetEnemyHorizontalRadius();

            agent.stoppingDistance =
                enemyRadius + stopGap;

            agent.SetDestination(hit.position);
        }
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

    private void StopAgent()
    {
        IsAtTarget = false;

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}