using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Technical Attack Settings")]
    [SerializeField]
    private float rotationSpeed = 720f;


    [Header("Combat Stats")]
    [SerializeField]
    private CombatStats combatStats;


    [Header("Projectile Settings")]
    [SerializeField]
    private Projectile projectilePrefab;

    [SerializeField]
    private Transform attackPoint;


    [Header("Debug Stun Testing")]
    [Tooltip(
        "When enabled, Stun always procs. " +
        "Use only for deterministic card testing."
    )]
    [SerializeField]
    private bool forceStunProcForDebug;


    [Header("Animation Settings")]
    [SerializeField]
    private PlayerAttackAnimation attackAnimation;


    [Header("Target Settings")]
    [SerializeField]
    private LayerMask enemyLayer;


    [Header("Diagnostics")]
    [SerializeField]
    private bool logShotDamage;


    [Header("Debug - Do Not Edit")]
    [SerializeField]
    private bool debugAttackDelayed;

    [SerializeField]
    private float debugAttackDelayTimer;

    [SerializeField]
    private float currentAttackInterval;

    [SerializeField]
    private float currentAttackWindup;

    [SerializeField]
    private bool debugDoubleShotEnabled;

    [SerializeField]
    private int debugRicochetStacks;

    [SerializeField]
    private int debugRicochetExtraTargets;

    [SerializeField]
    private float debugRicochetDamagePercent;

    [SerializeField]
    private bool debugPoisonEnabled;

    [SerializeField]
    private int debugStunStacks;

    [SerializeField]
    private float debugStunDuration;

    [SerializeField]
    private int debugSlowStacks;

    [SerializeField]
    private float debugSlowPercent;

    [SerializeField]
    private float debugSlowDuration;


    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    private float attackTimer;
    private float attackDelayTimer;

    private bool attackAnimationStarted;


    // =========================================================
    // DOUBLE SHOT RUNTIME STATE
    // =========================================================

    private bool doubleShotEnabled;

    private float doubleShotProjectileDamageMultiplier = 1f;
    private float doubleShotWindupPenalty;
    private float doubleShotAttackIntervalMultiplier = 1f;
    private float doubleShotProjectileOffset;


    // =========================================================
    // RICOCHET RUNTIME STATE
    // =========================================================

    private int ricochetStacks;

    private int currentRicochetExtraTargets;

    private float currentRicochetDamageMultiplier;

    private float ricochetSearchRadius;


    // =========================================================
    // POISON RUNTIME STATE
    // =========================================================

    private bool poisonEnabled;

    private float poisonPrimaryApplyChance;

    private float poisonRicochetApplyChance;

    private float poisonDamageMultiplier;

    private float poisonDuration;

    private float poisonTickInterval;


    // =========================================================
    // STUN RUNTIME STATE
    // =========================================================

    private int stunStacks;

    private float stunPrimaryApplyChance;

    private float stunRicochetApplyChance;

    private float currentStunDuration;


    // =========================================================
    // SLOW RUNTIME STATE
    // =========================================================

    private int slowStacks;

    private float currentSlowPercent;

    private float slowDuration;


    public bool IsAttackDelayed =>
        attackDelayTimer > 0f;

    public bool IsDoubleShotEnabled =>
        doubleShotEnabled;

    public int RicochetStacks =>
        ricochetStacks;

    public bool IsPoisonEnabled =>
        poisonEnabled;

    public int StunStacks =>
        stunStacks;

    public int SlowStacks =>
        slowStacks;


    private float CurrentStunDuration =>
        stunStacks > 0
            ? currentStunDuration
            : 0f;


    private float CurrentSlowPercent =>
        slowStacks > 0
            ? currentSlowPercent
            : 0f;


    private void Start()
    {
        playerMovement =
            GetComponent<PlayerMovement>();

        playerHealth =
            GetComponent<PlayerHealth>();

        if (combatStats == null)
        {
            Debug.LogError(
                "PlayerAttack: Assign CombatStats from GameProgression.",
                this
            );

            enabled = false;
            return;
        }

        if (combatStats.HeroAttackRadius <= 0f ||
            combatStats.HeroProjectileSpeed <= 0f)
        {
            Debug.LogError(
                "PlayerAttack: Hero attack balance values " +
                "must be positive. Check GameBalanceConfig.",
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

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            ResetAttackCycle();
            return;
        }

        if (playerMovement != null &&
            playerMovement.WantsToMove)
        {
            ResetAttackCycle();
            return;
        }

        Collider nearestEnemy =
            FindNearestEnemy();

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

        RotateTowardsEnemy(
            nearestEnemy
        );

        attackTimer -=
            Time.deltaTime;

        if (!attackAnimationStarted &&
            attackTimer <= currentAttackWindup)
        {
            StartAttackAnimation();
        }

        if (attackTimer > 0f)
        {
            return;
        }

        AttackEnemy(
            nearestEnemyHealth
        );

        RefreshAttackTiming();

        attackTimer =
            currentAttackInterval;

        attackAnimationStarted =
            false;
    }


    private void LateUpdate()
    {
        debugAttackDelayed =
            IsAttackDelayed;

        debugAttackDelayTimer =
            attackDelayTimer;

        debugDoubleShotEnabled =
            doubleShotEnabled;

        debugRicochetStacks =
            ricochetStacks;

        debugRicochetExtraTargets =
            currentRicochetExtraTargets;

        debugRicochetDamagePercent =
            currentRicochetDamageMultiplier *
            100f;

        debugPoisonEnabled =
            poisonEnabled;

        debugStunStacks =
            stunStacks;

        debugStunDuration =
            CurrentStunDuration;

        debugSlowStacks =
            slowStacks;

        debugSlowPercent =
            CurrentSlowPercent *
            100f;

        debugSlowDuration =
            slowStacks > 0
                ? slowDuration
                : 0f;
    }


    private void RefreshAttackTiming()
    {
        float interval =
            combatStats.HeroAttackInterval;

        if (doubleShotEnabled)
        {
            interval *=
                doubleShotAttackIntervalMultiplier;
        }

        currentAttackInterval =
            Mathf.Max(
                0.01f,
                interval
            );

        float baseWindup =
            combatStats.HeroBaseAttackWindup;

        if (doubleShotEnabled)
        {
            baseWindup +=
                doubleShotWindupPenalty;
        }

        float acceleratedWindup =
            baseWindup /
            combatStats.HeroAttackSpeedMultiplier;

        currentAttackWindup =
            Mathf.Clamp(
                acceleratedWindup,
                0.01f,
                currentAttackInterval
            );
    }


    // =========================================================
    // DOUBLE SHOT
    // =========================================================

    public bool TryEnableDoubleShot(
        float projectileDamageMultiplier,
        float attackIntervalMultiplier,
        float windupPenalty,
        float projectileOffset
    )
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        if (doubleShotEnabled)
        {
            Debug.LogWarning(
                "PlayerAttack: Double Shot is already enabled.",
                this
            );

            return false;
        }

        if (projectileDamageMultiplier <= 0f ||
            attackIntervalMultiplier < 1f ||
            windupPenalty < 0f ||
            projectileOffset < 0f)
        {
            Debug.LogError(
                "PlayerAttack: Invalid Double Shot balance values.",
                this
            );

            return false;
        }

        doubleShotProjectileDamageMultiplier =
            Mathf.Clamp(
                projectileDamageMultiplier,
                0.01f,
                1f
            );

        doubleShotAttackIntervalMultiplier =
            Mathf.Max(
                1f,
                attackIntervalMultiplier
            );

        doubleShotWindupPenalty =
            Mathf.Max(
                0f,
                windupPenalty
            );

        doubleShotProjectileOffset =
            Mathf.Max(
                0f,
                projectileOffset
            );

        doubleShotEnabled =
            true;

        ResetAttackCycle();

        Debug.Log(
            "PlayerAttack: Double Shot enabled. " +
            $"Projectile damage multiplier = " +
            $"{doubleShotProjectileDamageMultiplier:F2}. " +
            $"Attack interval multiplier = " +
            $"{doubleShotAttackIntervalMultiplier:F2}. " +
            $"Windup penalty = " +
            $"{doubleShotWindupPenalty:F2} s. " +
            $"Projectile offset = " +
            $"{doubleShotProjectileOffset:F2}.",
            this
        );

        return true;
    }


    // =========================================================
    // RICOCHET
    // =========================================================

    public bool TryUpgradeRicochet(
        int stackNumber,
        int maximumStacks,
        int extraTargets,
        float damageMultiplier,
        float searchRadius
    )
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        int expectedStack =
            ricochetStacks + 1;

        if (stackNumber != expectedStack)
        {
            Debug.LogError(
                "PlayerAttack: Ricochet stack order mismatch. " +
                $"Expected {expectedStack}, received {stackNumber}.",
                this
            );

            return false;
        }

        if (maximumStacks <= 0 ||
            stackNumber <= 0 ||
            stackNumber > maximumStacks)
        {
            Debug.LogError(
                "PlayerAttack: Invalid Ricochet stack values.",
                this
            );

            return false;
        }

        if (extraTargets < 0 ||
            damageMultiplier <= 0f ||
            searchRadius <= 0f)
        {
            Debug.LogError(
                "PlayerAttack: Invalid Ricochet balance values.",
                this
            );

            return false;
        }

        ricochetStacks =
            stackNumber;

        currentRicochetExtraTargets =
            Mathf.Max(
                0,
                extraTargets
            );

        currentRicochetDamageMultiplier =
            Mathf.Clamp01(
                damageMultiplier
            );

        ricochetSearchRadius =
            Mathf.Max(
                0.1f,
                searchRadius
            );

        Debug.Log(
            "PlayerAttack: Ricochet upgraded to " +
            $"{ricochetStacks}/{maximumStacks}. " +
            $"Extra targets = " +
            $"{currentRicochetExtraTargets}. " +
            $"Ricochet damage = " +
            $"{currentRicochetDamageMultiplier * 100f:F0}%. " +
            $"Search radius = " +
            $"{ricochetSearchRadius:F2}.",
            this
        );

        return true;
    }


    // =========================================================
    // POISON
    // =========================================================

    public bool TryEnablePoison(
        float primaryApplyChance,
        float ricochetApplyChance,
        float damageMultiplier,
        float duration,
        float tickInterval
    )
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        if (poisonEnabled)
        {
            Debug.LogWarning(
                "PlayerAttack: Poison is already enabled.",
                this
            );

            return false;
        }

        if (primaryApplyChance < 0f ||
            primaryApplyChance > 1f ||
            ricochetApplyChance < 0f ||
            ricochetApplyChance > 1f ||
            damageMultiplier <= 0f ||
            damageMultiplier > 1f ||
            duration <= 0f ||
            tickInterval <= 0f)
        {
            Debug.LogError(
                "PlayerAttack: Invalid Poison balance values.",
                this
            );

            return false;
        }

        poisonPrimaryApplyChance =
            Mathf.Clamp01(
                primaryApplyChance
            );

        poisonRicochetApplyChance =
            Mathf.Clamp01(
                ricochetApplyChance
            );

        poisonDamageMultiplier =
            Mathf.Clamp(
                damageMultiplier,
                0.001f,
                1f
            );

        poisonDuration =
            Mathf.Max(
                0.1f,
                duration
            );

        poisonTickInterval =
            Mathf.Max(
                0.1f,
                tickInterval
            );

        poisonEnabled =
            true;

        Debug.Log(
            "PlayerAttack: Poison enabled. " +
            $"Primary apply chance = " +
            $"{poisonPrimaryApplyChance * 100f:F0}%. " +
            $"Ricochet apply chance = " +
            $"{poisonRicochetApplyChance * 100f:F0}%. " +
            $"Tick damage = " +
            $"{poisonDamageMultiplier * 100f:F0}% of the " +
            "hit that created the poison. " +
            $"Duration = {poisonDuration:F1} s. " +
            $"Tick interval = {poisonTickInterval:F1} s.",
            this
        );

        return true;
    }


    // =========================================================
    // STUN
    // =========================================================

    public bool TryUpgradeStun(
        int stackNumber,
        int maximumStacks,
        float primaryApplyChance,
        float ricochetApplyChance,
        float duration
    )
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        int expectedStack =
            stunStacks + 1;

        if (stackNumber != expectedStack)
        {
            Debug.LogError(
                "PlayerAttack: Stun stack order mismatch. " +
                $"Expected {expectedStack}, received {stackNumber}.",
                this
            );

            return false;
        }

        if (maximumStacks <= 0 ||
            stackNumber <= 0 ||
            stackNumber > maximumStacks)
        {
            Debug.LogError(
                "PlayerAttack: Invalid Stun stack values.",
                this
            );

            return false;
        }

        if (primaryApplyChance < 0f ||
            primaryApplyChance > 1f ||
            ricochetApplyChance < 0f ||
            ricochetApplyChance > 1f ||
            duration <= 0f)
        {
            Debug.LogError(
                "PlayerAttack: Invalid Stun balance values.",
                this
            );

            return false;
        }

        stunStacks =
            stackNumber;

        stunPrimaryApplyChance =
            Mathf.Clamp01(
                primaryApplyChance
            );

        stunRicochetApplyChance =
            Mathf.Clamp01(
                ricochetApplyChance
            );

        currentStunDuration =
            Mathf.Max(
                0.1f,
                duration
            );

        Debug.Log(
            "PlayerAttack: Stun upgraded to " +
            $"{stunStacks}/{maximumStacks}. " +
            $"Primary apply chance = " +
            $"{stunPrimaryApplyChance * 100f:F0}%. " +
            $"Ricochet apply chance = " +
            $"{stunRicochetApplyChance * 100f:F0}%. " +
            $"Duration = {CurrentStunDuration:F2} s. " +
            $"Debug force proc = {forceStunProcForDebug}.",
            this
        );

        return true;
    }


    // =========================================================
    // SLOW
    // =========================================================

    public bool TryUpgradeSlow(
        int stackNumber,
        int maximumStacks,
        float movementReductionPercent,
        float duration
    )
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        int expectedStack =
            slowStacks + 1;

        if (stackNumber != expectedStack)
        {
            Debug.LogError(
                "PlayerAttack: Slow stack order mismatch. " +
                $"Expected {expectedStack}, received {stackNumber}.",
                this
            );

            return false;
        }

        if (maximumStacks <= 0 ||
            stackNumber <= 0 ||
            stackNumber > maximumStacks)
        {
            Debug.LogError(
                "PlayerAttack: Invalid Slow stack values.",
                this
            );

            return false;
        }

        if (movementReductionPercent <= 0f ||
            movementReductionPercent > 0.95f ||
            duration <= 0f)
        {
            Debug.LogError(
                "PlayerAttack: Invalid Slow balance values.",
                this
            );

            return false;
        }

        slowStacks =
            stackNumber;

        currentSlowPercent =
            Mathf.Clamp(
                movementReductionPercent,
                0.01f,
                0.95f
            );

        slowDuration =
            Mathf.Max(
                0.1f,
                duration
            );

        Debug.Log(
            "PlayerAttack: Slow upgraded to " +
            $"{slowStacks}/{maximumStacks}. " +
            $"Movement reduction = " +
            $"{CurrentSlowPercent * 100f:F0}%. " +
            $"Duration = {slowDuration:F2} s. " +
            "Every projectile hit applies or refreshes Slow.",
            this
        );

        return true;
    }


    // =========================================================
    // ATTACK DELAY
    // =========================================================

    public void StartAttackDelay(
        float duration
    )
    {
        float safeDuration =
            Mathf.Max(
                0f,
                duration
            );

        attackDelayTimer =
            Mathf.Max(
                attackDelayTimer,
                safeDuration
            );

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
            Mathf.Max(
                0f,
                attackDelayTimer -
                Time.deltaTime
            );

        return true;
    }


    // =========================================================
    // TARGETING
    // =========================================================

    private Collider FindNearestEnemy()
    {
        Collider[] enemiesInRange =
            Physics.OverlapSphere(
                transform.position,
                combatStats.HeroAttackRadius,
                enemyLayer
            );

        Collider nearestEnemy =
            null;

        float shortestDistance =
            float.MaxValue;

        foreach (
            Collider enemy
            in enemiesInRange
        )
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance <
                shortestDistance)
            {
                shortestDistance =
                    distance;

                nearestEnemy =
                    enemy;
            }
        }

        return nearestEnemy;
    }


    // =========================================================
    // ATTACK CYCLE
    // =========================================================

    private void ResetAttackCycle()
    {
        if (combatStats != null)
        {
            RefreshAttackTiming();

            attackTimer =
                currentAttackWindup;
        }

        attackAnimationStarted =
            false;

        if (attackAnimation != null)
        {
            attackAnimation
                .ResetAnimation();
        }
    }


    private void StartAttackAnimation()
    {
        attackAnimationStarted =
            true;

        if (attackAnimation != null)
        {
            attackAnimation.Play(
                currentAttackWindup
            );
        }
    }


    private void RotateTowardsEnemy(
        Collider enemy
    )
    {
        Vector3 direction =
            enemy.transform.position -
            transform.position;

        direction.y =
            0f;

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


    // =========================================================
    // FIRE
    // =========================================================

    private void AttackEnemy(
        EnemyHealth enemyHealth
    )
    {
        if (enemyHealth == null)
        {
            return;
        }

        if (projectilePrefab == null ||
            attackPoint == null)
        {
            return;
        }

        float calculatedDamage =
            combatStats.HeroDamage;

        if (calculatedDamage <= 0f)
        {
            Debug.LogError(
                "PlayerAttack: Hero damage must be positive. " +
                "Check CombatStats and its Balance Config.",
                this
            );

            enabled =
                false;

            return;
        }

        float critChancePercent =
            combatStats
                .HeroCritChancePercent;

        bool isCritical;

        if (critChancePercent >= 100f)
        {
            isCritical =
                true;
        }
        else if (critChancePercent <= 0f)
        {
            isCritical =
                false;
        }
        else
        {
            float probability =
                critChancePercent /
                100f;

            isCritical =
                Random.value <
                probability;
        }

        if (isCritical)
        {
            calculatedDamage *=
                combatStats
                    .HeroCritDamageMultiplier;
        }

        if (doubleShotEnabled)
        {
            calculatedDamage *=
                doubleShotProjectileDamageMultiplier;
        }

        int shotDamage =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    calculatedDamage
                )
            );

        int projectileCount =
            doubleShotEnabled
                ? 2
                : 1;

        for (
            int i = 0;
            i < projectileCount;
            i++
        )
        {
            Vector3 spawnPosition =
                attackPoint.position;

            if (doubleShotEnabled)
            {
                float side =
                    i == 0
                        ? -1f
                        : 1f;

                spawnPosition +=
                    attackPoint.right
                    *
                    doubleShotProjectileOffset
                    *
                    side;
            }

            Projectile projectile =
                Instantiate(
                    projectilePrefab,
                    spawnPosition,
                    attackPoint.rotation
                );

            projectile.Initialize(
                enemyHealth,
                shotDamage,
                combatStats.HeroProjectileSpeed,
                currentRicochetExtraTargets,
                currentRicochetDamageMultiplier,
                ricochetSearchRadius,
                enemyLayer,
                logShotDamage,
                poisonEnabled,
                poisonPrimaryApplyChance,
                poisonRicochetApplyChance,
                poisonDamageMultiplier,
                poisonDuration,
                poisonTickInterval,
                stunStacks > 0,
                stunPrimaryApplyChance,
                stunRicochetApplyChance,
                CurrentStunDuration,
                forceStunProcForDebug,
                slowStacks > 0,
                CurrentSlowPercent,
                slowDuration
            );
        }

        if (logShotDamage)
        {
            Debug.Log(
                $"PlayerAttack: Shot damage = {shotDamage}. " +
                $"Projectiles = {projectileCount}. " +
                $"Projectile speed = " +
                $"{combatStats.HeroProjectileSpeed:F2}. " +
                $"Potential primary damage = " +
                $"{shotDamage * projectileCount}. " +
                $"Ricochet stacks = {ricochetStacks}. " +
                $"Ricochet targets = " +
                $"{currentRicochetExtraTargets}. " +
                $"Ricochet damage = " +
                $"{currentRicochetDamageMultiplier * 100f:F0}%. " +
                $"Poison = {poisonEnabled}. " +
                $"Stun stacks = {stunStacks}. " +
                $"Stun duration = {CurrentStunDuration:F2}. " +
                $"Slow stacks = {slowStacks}. " +
                $"Slow = {CurrentSlowPercent * 100f:F0}%. " +
                $"Slow duration = " +
                $"{(slowStacks > 0 ? slowDuration : 0f):F2}. " +
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