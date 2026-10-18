using UnityEngine;

[RequireComponent(typeof(DefenseTowerRunStats))]
public class DefenseTowerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
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
    private DefenseTowerRunStats individualStats;

    public EnemyHealth CurrentTarget => currentTarget;

    private void Start()
    {
        combatStats = FindAnyObjectByType<CombatStats>();
        individualStats = GetComponent<DefenseTowerRunStats>();
        if (combatStats == null || individualStats == null)
        {
            Debug.LogError("DefenseTowerAttack: CombatStats or DefenseTowerRunStats was not found.", this);
            enabled = false;
            return;
        }
        if (combatStats.TowerAttackRadius <= 0f || combatStats.TowerProjectileSpeed <= 0f)
        {
            Debug.LogError("DefenseTowerAttack: Tower attack radius and projectile speed must be positive. Check GameBalanceConfig.", this);
            enabled = false;
            return;
        }
        currentAttackInterval = individualStats.GetAttackInterval(combatStats);
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
        if (targetSearchTimer > 0f) return;
        targetSearchTimer = targetSearchInterval;
        EnemyHealth previousTarget = currentTarget;
        currentTarget = FindClosestEnemy();
        if (currentTarget != previousTarget) ReportTargetChange();
    }

    private EnemyHealth FindClosestEnemy()
    {
        Collider[] detectedColliders = Physics.OverlapSphere(
            transform.position, combatStats.TowerAttackRadius, enemyLayer,
            QueryTriggerInteraction.Collide);
        EnemyHealth closestEnemy = null;
        float closestSqrDistance = float.PositiveInfinity;
        foreach (Collider detectedCollider in detectedColliders)
        {
            EnemyHealth enemyHealth = detectedCollider.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null || enemyHealth.IsDead) continue;
            float sqrDistance = (enemyHealth.transform.position - transform.position).sqrMagnitude;
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
        if (currentTarget == null || rotatingPart == null) return;
        Vector3 direction = currentTarget.transform.position - rotatingPart.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rotatingPart.rotation = Quaternion.RotateTowards(
            rotatingPart.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void UpdateAttack()
    {
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
        if (currentTarget == null) return;
        if (currentTarget.IsDead)
        {
            currentTarget = null;
            return;
        }
        float sqrDistanceToTarget = (currentTarget.transform.position - transform.position).sqrMagnitude;
        float attackRadiusSqr = combatStats.TowerAttackRadius * combatStats.TowerAttackRadius;
        if (sqrDistanceToTarget > attackRadiusSqr)
        {
            currentTarget = null;
            return;
        }
        if (attackTimer > 0f || !TryShootAtCurrentTarget()) return;
        attackTimer = currentAttackInterval;
    }

    private bool TryShootAtCurrentTarget()
    {
        if (currentTarget == null || projectilePrefab == null || attackPoint == null)
            return false;
        if (combatStats == null || individualStats == null)
        {
            Debug.LogError("DefenseTowerAttack: CombatStats or individual tower stats are missing.", this);
            enabled = false;
            return false;
        }
        float calculatedDamage = individualStats.GetDamage(combatStats);
        if (calculatedDamage <= 0f) return false;
        int shotDamage = Mathf.Max(1, Mathf.RoundToInt(calculatedDamage));
        currentAttackInterval = individualStats.GetAttackInterval(combatStats);
        Projectile projectile = Instantiate(projectilePrefab, attackPoint.position, attackPoint.rotation);
        projectile.Initialize(currentTarget, shotDamage, combatStats.TowerProjectileSpeed);
        if (logShotDamage)
        {
            Debug.Log($"DefenseTowerAttack: Shot damage = {shotDamage}. Projectile speed = {combatStats.TowerProjectileSpeed:F2}. Time = {Time.time:F3}. Next interval = {currentAttackInterval:F3}. Tower: {gameObject.name}", this);
        }
        Debug.Log(gameObject.name + " fired at " + currentTarget.gameObject.name);
        return true;
    }

    private void ReportTargetChange()
    {
        if (currentTarget != null)
            Debug.Log(gameObject.name + " selected target: " + currentTarget.gameObject.name);
        else
            Debug.Log(gameObject.name + " has no target.");
    }

    private void OnDrawGizmosSelected()
    {
        if (combatStats == null || combatStats.TowerAttackRadius <= 0f) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, combatStats.TowerAttackRadius);
    }
}
