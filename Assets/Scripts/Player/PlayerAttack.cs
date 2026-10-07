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

    [Header("Double Shot Run Upgrade")]
    [SerializeField, Range(0.01f, 1f)]
    private float doubleShotProjectileDamageMultiplier = 0.85f;

    [SerializeField, Min(0f)]
    private float doubleShotWindupPenalty = 0.1f;

    [SerializeField, Min(1f)]
    private float doubleShotAttackIntervalMultiplier = 1.10f;

    [SerializeField, Min(0f)]
    private float doubleShotProjectileOffset = 0.08f;

    [Header("Ricochet Run Upgrade")]
    [SerializeField, Min(0.1f)]
    private float ricochetSearchRadius = 4f;

    [Header("Poison Run Upgrade")]
    [SerializeField, Range(0f, 1f)]
    private float poisonPrimaryApplyChance = 0.50f;

    [SerializeField, Range(0f, 1f)]
    private float poisonRicochetApplyChance = 0.33f;

    [SerializeField, Range(0.001f, 1f)]
    private float poisonDamageMultiplier = 0.05f;

    [SerializeField, Min(0.1f)]
    private float poisonDuration = 5f;

    [SerializeField, Min(0.1f)]
    private float poisonTickInterval = 2.5f;

    [Header("Stun Run Upgrade")]
    [SerializeField, Range(0f, 1f)]
    private float stunPrimaryApplyChance = 0.10f;

    [SerializeField, Range(0f, 1f)]
    private float stunRicochetApplyChance = 0.05f;

    [SerializeField, Min(0.1f)]
    private float stunFirstDuration = 1f;

    [SerializeField, Min(0.1f)]
    private float stunSecondDuration = 2f;

    [Header("Debug Stun Testing")]
    [Tooltip(
        "When enabled, Stun always procs. " +
        "Use only for deterministic card testing."
    )]
    [SerializeField]
    private bool forceStunProcForDebug;

    [Header("Slow Run Upgrade")]
    [SerializeField, Range(0f, 0.95f)]
    private float slowFirstPercent = 0.15f;

    [SerializeField, Range(0f, 0.95f)]
    private float slowSecondPercent = 0.30f;

    [SerializeField, Min(0.1f)]
    private float slowDuration = 2f;

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
    [SerializeField] private bool debugDoubleShotEnabled;
    [SerializeField] private int debugRicochetStacks;
    [SerializeField] private int debugRicochetExtraTargets;
    [SerializeField] private float debugRicochetDamagePercent;
    [SerializeField] private bool debugPoisonEnabled;
    [SerializeField] private int debugStunStacks;
    [SerializeField] private float debugStunDuration;
    [SerializeField] private int debugSlowStacks;
    [SerializeField] private float debugSlowPercent;
    [SerializeField] private float debugSlowDuration;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    private float attackTimer;
    private float attackDelayTimer;

    private bool attackAnimationStarted;
    private bool doubleShotEnabled;
    private int ricochetStacks;
    private bool poisonEnabled;
    private int stunStacks;
    private int slowStacks;

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

    private float CurrentSlowPercent
    {
        get
        {
            if (slowStacks <= 0)
            {
                return 0f;
            }

            if (slowStacks == 1)
            {
                return slowFirstPercent;
            }

            return slowSecondPercent;
        }
    }

    private float CurrentStunDuration
    {
        get
        {
            if (stunStacks <= 0)
            {
                return 0f;
            }

            if (stunStacks == 1)
            {
                return stunFirstDuration;
            }

            return stunSecondDuration;
        }
    }

    private int CurrentRicochetExtraTargets
    {
        get
        {
            if (ricochetStacks <= 0)
            {
                return 0;
            }

            if (ricochetStacks == 1)
            {
                return 1;
            }

            return 2;
        }
    }

    private float CurrentRicochetDamageMultiplier
    {
        get
        {
            if (ricochetStacks <= 0)
            {
                return 0f;
            }

            if (ricochetStacks >= 3)
            {
                return 0.66f;
            }

            return 0.33f;
        }
    }

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
            CurrentRicochetExtraTargets;

        debugRicochetDamagePercent =
            CurrentRicochetDamageMultiplier *
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
            CurrentSlowPercent * 100f;

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
            attackWindup;

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

    public bool TryEnableDoubleShot()
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

        doubleShotEnabled = true;

        ResetAttackCycle();

        Debug.Log(
            "PlayerAttack: Double Shot enabled. " +
            $"Projectile damage multiplier = " +
            $"{doubleShotProjectileDamageMultiplier:F2}. " +
            $"Attack interval multiplier = " +
            $"{doubleShotAttackIntervalMultiplier:F2}. " +
            $"Windup penalty = " +
            $"{doubleShotWindupPenalty:F2} s.",
            this
        );

        return true;
    }

    public bool TryUpgradeRicochet()
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        if (ricochetStacks >= 3)
        {
            Debug.LogWarning(
                "PlayerAttack: Ricochet already reached 3/3.",
                this
            );

            return false;
        }

        ricochetStacks++;

        Debug.Log(
            "PlayerAttack: Ricochet upgraded to " +
            $"{ricochetStacks}/3. " +
            $"Extra targets = " +
            $"{CurrentRicochetExtraTargets}. " +
            $"Ricochet damage = " +
            $"{CurrentRicochetDamageMultiplier * 100f:F0}%. " +
            $"Search radius = {ricochetSearchRadius:F2}.",
            this
        );

        return true;
    }

    public bool TryEnablePoison()
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

        poisonEnabled = true;

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

    public bool TryUpgradeStun()
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        if (stunStacks >= 2)
        {
            Debug.LogWarning(
                "PlayerAttack: Stun already reached 2/2.",
                this
            );

            return false;
        }

        stunStacks++;

        Debug.Log(
            "PlayerAttack: Stun upgraded to " +
            $"{stunStacks}/2. " +
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

    public bool TryUpgradeSlow()
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        if (slowStacks >= 2)
        {
            Debug.LogWarning(
                "PlayerAttack: Slow already reached 2/2.",
                this
            );

            return false;
        }

        slowStacks++;

        Debug.Log(
            "PlayerAttack: Slow upgraded to " +
            $"{slowStacks}/2. " +
            $"Movement reduction = " +
            $"{CurrentSlowPercent * 100f:F0}%. " +
            $"Duration = {slowDuration:F2} s. " +
            "Every projectile hit applies or refreshes Slow.",
            this
        );

        return true;
    }

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

    private Collider FindNearestEnemy()
    {
        Collider[] enemiesInRange =
            Physics.OverlapSphere(
                transform.position,
                attackRadius,
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
            attackAnimation.ResetAnimation();
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

            enabled = false;
            return;
        }

        float critChancePercent =
            combatStats.HeroCritChancePercent;

        bool isCritical;

        if (critChancePercent >= 100f)
        {
            isCritical = true;
        }
        else if (critChancePercent <= 0f)
        {
            isCritical = false;
        }
        else
        {
            float probability =
                critChancePercent / 100f;

            isCritical =
                Random.value <
                probability;
        }

        if (isCritical)
        {
            calculatedDamage *=
                combatStats.HeroCritDamageMultiplier;
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
            doubleShotEnabled ? 2 : 1;

        for (int i = 0;
             i < projectileCount;
             i++)
        {
            Vector3 spawnPosition =
                attackPoint.position;

            if (doubleShotEnabled)
            {
                float side =
                    i == 0 ? -1f : 1f;

                spawnPosition +=
                    attackPoint.right *
                    doubleShotProjectileOffset *
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
                CurrentRicochetExtraTargets,
                CurrentRicochetDamageMultiplier,
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
                $"Potential primary damage = " +
                $"{shotDamage * projectileCount}. " +
                $"Ricochet stacks = {ricochetStacks}. " +
                $"Ricochet targets = " +
                $"{CurrentRicochetExtraTargets}. " +
                $"Ricochet damage = " +
                $"{CurrentRicochetDamageMultiplier * 100f:F0}%. " +
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