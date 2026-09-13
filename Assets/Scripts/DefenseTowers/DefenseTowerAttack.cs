using UnityEngine;

public class DefenseTowerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 6f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Tower References")]
    [SerializeField] private Transform rotatingPart;

    [Header("Projectile Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform attackPoint;

    [Header("Target Search Settings")]
    [SerializeField] private float targetSearchInterval = 0.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Diagnostics")]
    [SerializeField] private bool logShotDamage;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private EnemyHealth currentTarget;
    [SerializeField] private float attackTimer;
    [SerializeField] private CombatStats combatStats;
    [SerializeField] private float currentAttackInterval;

    private float targetSearchTimer;

    public EnemyHealth CurrentTarget => currentTarget;

    private void Start()
    {
        combatStats = FindAnyObjectByType<CombatStats>();

        if (combatStats == null)
        {
            Debug.LogError(
                "DefenseTowerAttack: CombatStats was not found. " +
                "Check GameProgression in the scene.",
                this
            );

            enabled = false;
            return;
        }

        currentAttackInterval = combatStats.TowerAttackInterval;

        // Новая башня готова к первому выстрелу.
        attackTimer = 0f;
    }

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
        Collider[] detectedColliders = Physics.OverlapSphere(
            transform.position,
            attackRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        EnemyHealth closestEnemy = null;
        float closestSqrDistance = float.PositiveInfinity;

        foreach (Collider detectedCollider in detectedColliders)
        {
            EnemyHealth enemyHealth =
                detectedCollider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            float sqrDistance =
                (enemyHealth.transform.position -
                 transform.position).sqrMagnitude;

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
        if (currentTarget == null || rotatingPart == null)
        {
            return;
        }

        Vector3 direction =
            currentTarget.transform.position - rotatingPart.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        rotatingPart.rotation = Quaternion.RotateTowards(
            rotatingPart.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void UpdateAttack()
    {
        // Перезарядка идёт даже тогда, когда цели нет.
        // После завершения таймер остаётся равным нулю.
        attackTimer = Mathf.Max(
            0f,
            attackTimer - Time.deltaTime
        );

        if (currentTarget == null)
        {
            return;
        }

        if (currentTarget.IsDead)
        {
            currentTarget = null;
            return;
        }

        float sqrDistanceToTarget =
            (currentTarget.transform.position -
             transform.position).sqrMagnitude;

        float attackRadiusSqr = attackRadius * attackRadius;

        if (sqrDistanceToTarget > attackRadiusSqr)
        {
            currentTarget = null;
            return;
        }

        // Смена цели не отменяет ожидание перезарядки.
        if (attackTimer > 0f)
        {
            return;
        }

        if (!TryShootAtCurrentTarget())
        {
            return;
        }

        // Запускаем перезарядку только после реального выстрела.
        attackTimer = currentAttackInterval;
    }

    private bool TryShootAtCurrentTarget()
    {
        if (currentTarget == null)
        {
            return false;
        }

        if (projectilePrefab == null || attackPoint == null)
        {
            return false;
        }

        if (combatStats == null)
        {
            Debug.LogError(
                "DefenseTowerAttack: CombatStats reference is missing.",
                this
            );

            enabled = false;
            return false;
        }

        float calculatedDamage = combatStats.TowerDamage;

        if (calculatedDamage <= 0f)
        {
            return false;
        }

        int shotDamage = Mathf.Max(
            1,
            Mathf.RoundToInt(calculatedDamage)
        );

        // Этот интервал будет выдержан ПОСЛЕ данного выстрела.
        currentAttackInterval = combatStats.TowerAttackInterval;

        Projectile projectile = Instantiate(
            projectilePrefab,
            attackPoint.position,
            attackPoint.rotation
        );

        projectile.Initialize(currentTarget, shotDamage);

        if (logShotDamage)
        {
            Debug.Log(
                $"DefenseTowerAttack: Shot damage = {shotDamage}. " +
                $"Time = {Time.time:F3}. " +
                $"Next interval = {currentAttackInterval:F3}. " +
                $"Tower: {gameObject.name}",
                this
            );
        }

        Debug.Log(
            gameObject.name +
            " fired at " +
            currentTarget.gameObject.name
        );

        return true;
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
            Debug.Log(gameObject.name + " has no target.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}