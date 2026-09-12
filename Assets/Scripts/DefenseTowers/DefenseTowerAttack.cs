using UnityEngine;

public class DefenseTowerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 6f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Tower References")]
    [SerializeField] private Transform rotatingPart;

    [Header("Projectile Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform attackPoint;

    [Header("Target Search Settings")]
    [SerializeField] private float targetSearchInterval = 0.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private EnemyHealth currentTarget;
    [SerializeField] private float attackTimer;

    private float targetSearchTimer;

    public EnemyHealth CurrentTarget => currentTarget;

    private void Update()
    {
        UpdateTargetSearch();
        RotateTowardsCurrentTarget();
        UpdateAttack();
    }

    private void UpdateTargetSearch()
    {
        targetSearchTimer -= Time.deltaTime;

        if (targetSearchTimer > 0f)
        {
            return;
        }

        targetSearchTimer = targetSearchInterval;

        EnemyHealth previousTarget = currentTarget;

        currentTarget = FindClosestEnemy();

        if (currentTarget != previousTarget)
        {
            ReportTargetChange();
        }
    }

    private EnemyHealth FindClosestEnemy()
    {
        Collider[] detectedColliders =
            Physics.OverlapSphere(
                transform.position,
                attackRadius,
                enemyLayer,
                QueryTriggerInteraction.Collide
            );

        EnemyHealth closestEnemy = null;
        float closestSqrDistance =
            float.PositiveInfinity;

        foreach (Collider detectedCollider
                 in detectedColliders)
        {
            EnemyHealth enemyHealth =
                detectedCollider
                    .GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null)
            {
                continue;
            }

            if (enemyHealth.IsDead)
            {
                continue;
            }

            float sqrDistance =
                (
                    enemyHealth.transform.position -
                    transform.position
                ).sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestEnemy = enemyHealth;
            }
        }

        return closestEnemy;
    }

    private void RotateTowardsCurrentTarget()
    {
        if (currentTarget == null ||
            rotatingPart == null)
        {
            return;
        }

        Vector3 direction =
            currentTarget.transform.position -
            rotatingPart.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        rotatingPart.rotation =
            Quaternion.RotateTowards(
                rotatingPart.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    private void UpdateAttack()
    {
        if (currentTarget == null)
        {
            attackTimer = 0f;
            return;
        }

        if (currentTarget.IsDead)
        {
            currentTarget = null;
            attackTimer = 0f;
            return;
        }

        float sqrDistanceToTarget =
            (
                currentTarget.transform.position -
                transform.position
            ).sqrMagnitude;

        float attackRadiusSqr =
            attackRadius * attackRadius;

        if (sqrDistanceToTarget >
            attackRadiusSqr)
        {
            currentTarget = null;
            attackTimer = 0f;
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        ShootAtCurrentTarget();

        attackTimer = attackInterval;
    }

    private void ShootAtCurrentTarget()
    {
        if (currentTarget == null)
        {
            return;
        }

        if (projectilePrefab == null ||
            attackPoint == null)
        {
            return;
        }

        Projectile projectile =
            Instantiate(
                projectilePrefab,
                attackPoint.position,
                attackPoint.rotation
            );

        projectile.Initialize(
            currentTarget,
            attackDamage
        );

        Debug.Log(
            gameObject.name +
            " fired at " +
            currentTarget.gameObject.name
        );
    }

    private void ReportTargetChange()
    {
        if (currentTarget != null)
        {
            Debug.Log(
                gameObject.name +
                " selected target: " +
                currentTarget.gameObject.name
            );
        }
        else
        {
            Debug.Log(
                gameObject.name +
                " has no target."
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRadius
        );
    }
}