using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 4f;

    [SerializeField, Min(0.01f)]
    private float attackWindup = 0.2f;

    [SerializeField] private float rotationSpeed = 720f;

    [Header("Combat Stats")]
    [SerializeField] private CombatStats combatStats;

    [Header("Projectile Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform attackPoint;

    [Header("Animation Settings")]
    [SerializeField] private PlayerAttackAnimation attackAnimation;

    [Header("Target Settings")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Diagnostics")]
    [SerializeField] private bool logShotDamage;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool debugAttackDelayed;
    [SerializeField] private float debugAttackDelayTimer;
    [SerializeField] private float currentAttackInterval;
    [SerializeField] private float currentAttackWindup;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    private float attackTimer;
    private float attackDelayTimer;

    private bool attackAnimationStarted;

    public bool IsAttackDelayed => attackDelayTimer > 0f;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();

        if (combatStats == null)
        {
            Debug.LogError(
                "PlayerAttack: Assign CombatStats from GameProgression.",
                this
            );

            enabled = false;
            return;
        }

        ResetAttackCycle();
    }

    private void Update()
    {
        if (UpdateAttackDelay())
        {
            ResetAttackCycle();
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            ResetAttackCycle();
            return;
        }

        if (playerMovement != null && playerMovement.IsMoving)
        {
            ResetAttackCycle();
            return;
        }

        Collider nearestEnemy = FindNearestEnemy();

        if (nearestEnemy == null)
        {
            ResetAttackCycle();
            return;
        }

        EnemyHealth nearestEnemyHealth =
            nearestEnemy.GetComponent<EnemyHealth>();

        if (nearestEnemyHealth == null)
        {
            ResetAttackCycle();
            return;
        }

        RotateTowardsEnemy(nearestEnemy);

        attackTimer -= Time.deltaTime;

        if (!attackAnimationStarted &&
            attackTimer <= currentAttackWindup)
        {
            StartAttackAnimation();
        }

        if (attackTimer > 0f)
        {
            return;
        }

        AttackEnemy(nearestEnemyHealth);

        RefreshAttackTiming();

        attackTimer = currentAttackInterval;
        attackAnimationStarted = false;
    }

    private void LateUpdate()
    {
        debugAttackDelayed = IsAttackDelayed;
        debugAttackDelayTimer = attackDelayTimer;
    }

    private void RefreshAttackTiming()
    {
        currentAttackInterval = combatStats.HeroAttackInterval;

        float acceleratedWindup =
            attackWindup / combatStats.HeroAttackSpeedMultiplier;

        currentAttackWindup = Mathf.Clamp(
            acceleratedWindup,
            0.01f,
            currentAttackInterval
        );
    }

    public void StartAttackDelay(float duration)
    {
        float safeDuration = Mathf.Max(0f, duration);

        attackDelayTimer =
            Mathf.Max(attackDelayTimer, safeDuration);

        ResetAttackCycle();

        Debug.Log(
            "PlayerAttack: Attacks blocked for " +
            safeDuration.ToString("F1") +
            " seconds."
        );
    }

    private bool UpdateAttackDelay()
    {
        if (attackDelayTimer <= 0f)
        {
            return false;
        }

        attackDelayTimer =
            Mathf.Max(0f, attackDelayTimer - Time.deltaTime);

        return true;
    }

    private Collider FindNearestEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(
            transform.position,
            attackRadius,
            enemyLayer
        );

        Collider nearestEnemy = null;
        float shortestDistance = float.MaxValue;

        foreach (Collider enemy in enemiesInRange)
        {
            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private void ResetAttackCycle()
    {
        if (combatStats != null)
        {
            RefreshAttackTiming();
            attackTimer = currentAttackWindup;
        }

        attackAnimationStarted = false;

        if (attackAnimation != null)
        {
            attackAnimation.ResetAnimation();
        }
    }

    private void StartAttackAnimation()
    {
        attackAnimationStarted = true;

        if (attackAnimation != null)
        {
            attackAnimation.Play(currentAttackWindup);
        }
    }

    private void RotateTowardsEnemy(Collider enemy)
    {
        Vector3 direction =
            enemy.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void AttackEnemy(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null)
        {
            return;
        }

        if (projectilePrefab == null || attackPoint == null)
        {
            return;
        }

        // »тоговый обычный урон с посто€нными и временными бонусами.
        float calculatedDamage = combatStats.HeroDamage;

        if (calculatedDamage <= 0f)
        {
            Debug.LogError(
                "PlayerAttack: Hero damage must be positive. " +
                "Check CombatStats and its Balance Config.",
                this
            );

            enabled = false;
            return;
        }

        float critChancePercent = combatStats.HeroCritChancePercent;

        bool isCritical;

        if (critChancePercent >= 100f)
        {
            // ѕри 100% каждый выстрел гарантированно критический.
            isCritical = true;
        }
        else if (critChancePercent <= 0f)
        {
            isCritical = false;
        }
        else
        {
            // ѕереводим проценты в веро€тность:
            // например, 3% превращаютс€ в 0.03.
            float probability = critChancePercent / 100f;

            isCritical = Random.value < probability;
        }

        if (isCritical)
        {
            calculatedDamage *= combatStats.HeroCritDamageMultiplier;
        }

        // ќкругл€ем один раз Ч после применени€ крита.
        int shotDamage = Mathf.Max(
            1,
            Mathf.RoundToInt(calculatedDamage)
        );

        Projectile projectile = Instantiate(
            projectilePrefab,
            attackPoint.position,
            attackPoint.rotation
        );

        // —нар€д запоминает уже готовый урон.
        // ѕовторной проверки крита при попадании нет.
        projectile.Initialize(enemyHealth, shotDamage);

        if (logShotDamage)
        {
            Debug.Log(
                $"PlayerAttack: Shot damage = {shotDamage}. " +
                $"Critical = {isCritical}. " +
                $"Crit chance = {critChancePercent:F2}%. " +
                $"Time = {Time.time:F3}. " +
                $"Interval = {currentAttackInterval:F3}. " +
                $"Windup = {currentAttackWindup:F3}.",
                this
            );
        }
    }
}