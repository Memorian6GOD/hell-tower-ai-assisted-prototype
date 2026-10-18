using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class EnemyBalanceValues
{
    [SerializeField, Min(1)] private int baseMaxHealth = 100;
    [SerializeField, Min(1)] private int baseDamage = 10;
    [SerializeField, Min(0.01f)] private float moveSpeed = 2f;
    [SerializeField, Min(0.01f)] private float attackInterval = 1f;
    [SerializeField, Min(0f)] private float attackWindup = 0.5f;

    public int BaseMaxHealth => Mathf.Max(1, baseMaxHealth);
    public int BaseDamage => Mathf.Max(1, baseDamage);
    public float MoveSpeed => Mathf.Max(0.01f, moveSpeed);
    public float AttackInterval => Mathf.Max(0.01f, attackInterval);
    public float AttackWindup => Mathf.Max(0f, attackWindup);

    public EnemyBalanceValues(
        int baseMaxHealth,
        int baseDamage,
        float moveSpeed,
        float attackInterval,
        float attackWindup
    )
    {
        this.baseMaxHealth = baseMaxHealth;
        this.baseDamage = baseDamage;
        this.moveSpeed = moveSpeed;
        this.attackInterval = attackInterval;
        this.attackWindup = attackWindup;
    }

    public void Validate()
    {
        baseMaxHealth = Mathf.Max(1, baseMaxHealth);
        baseDamage = Mathf.Max(1, baseDamage);
        moveSpeed = Mathf.Max(0.01f, moveSpeed);
        attackInterval = Mathf.Max(0.01f, attackInterval);
        attackWindup = Mathf.Max(0f, attackWindup);
    }
}

[Serializable]
public sealed class WaveBalanceValues
{
    [SerializeField, Min(0.01f)] private float healthMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float damageMultiplier = 1f;

    public float HealthMultiplier => Mathf.Max(0.01f, healthMultiplier);
    public float DamageMultiplier => Mathf.Max(0.01f, damageMultiplier);

    public WaveBalanceValues(float healthMultiplier, float damageMultiplier)
    {
        this.healthMultiplier = healthMultiplier;
        this.damageMultiplier = damageMultiplier;
    }

    public void Validate()
    {
        healthMultiplier = Mathf.Max(0.01f, healthMultiplier);
        damageMultiplier = Mathf.Max(0.01f, damageMultiplier);
    }
}

[Serializable]
public sealed class DoubleShotBalanceValues
{
    [SerializeField, Range(0.01f, 1f)] private float projectileDamageMultiplier = 0.85f;
    [SerializeField, Min(1f)] private float attackIntervalMultiplier = 1.10f;
    [SerializeField, Min(0f)] private float windupPenalty = 0.10f;
    [SerializeField, Min(0f)] private float projectileOffset = 0.08f;

    public int MaximumStacks => 1;

    public float ProjectileDamageMultiplier => Mathf.Clamp(projectileDamageMultiplier, 0.01f, 1f);
    public float AttackIntervalMultiplier => Mathf.Max(1f, attackIntervalMultiplier);
    public float WindupPenalty => Mathf.Max(0f, windupPenalty);
    public float ProjectileOffset => Mathf.Max(0f, projectileOffset);

    public DoubleShotBalanceValues(
        float projectileDamageMultiplier,
        float attackIntervalMultiplier,
        float windupPenalty,
        float projectileOffset
    )
    {
        this.projectileDamageMultiplier = projectileDamageMultiplier;
        this.attackIntervalMultiplier = attackIntervalMultiplier;
        this.windupPenalty = windupPenalty;
        this.projectileOffset = projectileOffset;
    }

    public void Validate()
    {
        projectileDamageMultiplier = Mathf.Clamp(projectileDamageMultiplier, 0.01f, 1f);
        attackIntervalMultiplier = Mathf.Max(1f, attackIntervalMultiplier);
        windupPenalty = Mathf.Max(0f, windupPenalty);
        projectileOffset = Mathf.Max(0f, projectileOffset);
    }
}

[Serializable]
public sealed class RicochetTierBalanceValues
{
    [SerializeField, Min(0)] private int extraTargets = 1;
    [SerializeField, Range(0.01f, 1f)] private float damageMultiplier = 0.33f;

    public int ExtraTargets => Mathf.Max(0, extraTargets);
    public float DamageMultiplier => Mathf.Clamp(damageMultiplier, 0.01f, 1f);

    public RicochetTierBalanceValues(int extraTargets, float damageMultiplier)
    {
        this.extraTargets = extraTargets;
        this.damageMultiplier = damageMultiplier;
    }

    public void Validate()
    {
        extraTargets = Mathf.Max(0, extraTargets);
        damageMultiplier = Mathf.Clamp(damageMultiplier, 0.01f, 1f);
    }
}

[Serializable]
public sealed class RicochetBalanceValues
{
    [SerializeField, Min(0.1f)] private float searchRadius = 4f;
    [SerializeField] private RicochetTierBalanceValues stack1 = new RicochetTierBalanceValues(1, 0.33f);
    [SerializeField] private RicochetTierBalanceValues stack2 = new RicochetTierBalanceValues(2, 0.33f);
    [SerializeField] private RicochetTierBalanceValues stack3 = new RicochetTierBalanceValues(2, 0.66f);

    public float SearchRadius => Mathf.Max(0.1f, searchRadius);

    public int MaximumStacks
    {
        get
        {
            int stackCount = 0;

            if (stack1 != null) stackCount++;
            if (stack2 != null) stackCount++;
            if (stack3 != null) stackCount++;

            return Mathf.Max(1, stackCount);
        }
    }

    public RicochetTierBalanceValues GetTier(int stackNumber)
    {
        switch (stackNumber)
        {
            case 1: return stack1;
            case 2: return stack2;
            case 3: return stack3;
            default: return null;
        }
    }

    public void Validate()
    {
        searchRadius = Mathf.Max(0.1f, searchRadius);

        if (stack1 == null) stack1 = new RicochetTierBalanceValues(1, 0.33f);
        if (stack2 == null) stack2 = new RicochetTierBalanceValues(2, 0.33f);
        if (stack3 == null) stack3 = new RicochetTierBalanceValues(2, 0.66f);

        stack1.Validate();
        stack2.Validate();
        stack3.Validate();
    }
}

[Serializable]
public sealed class PoisonBalanceValues
{
    [SerializeField, Range(0f, 1f)] private float primaryApplyChance = 0.50f;
    [SerializeField, Range(0f, 1f)] private float ricochetApplyChance = 0.33f;
    [SerializeField, Range(0.001f, 1f)] private float damageMultiplier = 0.05f;
    [SerializeField, Min(0.1f)] private float duration = 5f;
    [SerializeField, Min(0.1f)] private float tickInterval = 2.5f;

    public int MaximumStacks => 1;

    public float PrimaryApplyChance => Mathf.Clamp01(primaryApplyChance);
    public float RicochetApplyChance => Mathf.Clamp01(ricochetApplyChance);
    public float DamageMultiplier => Mathf.Clamp(damageMultiplier, 0.001f, 1f);
    public float Duration => Mathf.Max(0.1f, duration);
    public float TickInterval => Mathf.Max(0.1f, tickInterval);

    public PoisonBalanceValues(
        float primaryApplyChance,
        float ricochetApplyChance,
        float damageMultiplier,
        float duration,
        float tickInterval
    )
    {
        this.primaryApplyChance = primaryApplyChance;
        this.ricochetApplyChance = ricochetApplyChance;
        this.damageMultiplier = damageMultiplier;
        this.duration = duration;
        this.tickInterval = tickInterval;
    }

    public void Validate()
    {
        primaryApplyChance = Mathf.Clamp01(primaryApplyChance);
        ricochetApplyChance = Mathf.Clamp01(ricochetApplyChance);
        damageMultiplier = Mathf.Clamp(damageMultiplier, 0.001f, 1f);
        duration = Mathf.Max(0.1f, duration);
        tickInterval = Mathf.Max(0.1f, tickInterval);
    }
}

[Serializable]
public sealed class StunTierBalanceValues
{
    [SerializeField, Min(0.1f)] private float duration = 1f;

    public float Duration => Mathf.Max(0.1f, duration);

    public StunTierBalanceValues(float duration)
    {
        this.duration = duration;
    }

    public void Validate()
    {
        duration = Mathf.Max(0.1f, duration);
    }
}

[Serializable]
public sealed class StunBalanceValues
{
    [SerializeField, Range(0f, 1f)] private float primaryApplyChance = 0.10f;
    [SerializeField, Range(0f, 1f)] private float ricochetApplyChance = 0.05f;
    [SerializeField] private StunTierBalanceValues stack1 = new StunTierBalanceValues(1f);
    [SerializeField] private StunTierBalanceValues stack2 = new StunTierBalanceValues(2f);

    public float PrimaryApplyChance => Mathf.Clamp01(primaryApplyChance);
    public float RicochetApplyChance => Mathf.Clamp01(ricochetApplyChance);

    public int MaximumStacks
    {
        get
        {
            int stackCount = 0;

            if (stack1 != null) stackCount++;
            if (stack2 != null) stackCount++;

            return Mathf.Max(1, stackCount);
        }
    }

    public StunTierBalanceValues GetTier(int stackNumber)
    {
        switch (stackNumber)
        {
            case 1: return stack1;
            case 2: return stack2;
            default: return null;
        }
    }

    public void Validate()
    {
        primaryApplyChance = Mathf.Clamp01(primaryApplyChance);
        ricochetApplyChance = Mathf.Clamp01(ricochetApplyChance);

        if (stack1 == null) stack1 = new StunTierBalanceValues(1f);
        if (stack2 == null) stack2 = new StunTierBalanceValues(2f);

        stack1.Validate();
        stack2.Validate();
    }
}

[Serializable]
public sealed class SlowTierBalanceValues
{
    [SerializeField, Range(0.01f, 0.95f)] private float movementReductionPercent = 0.15f;

    public float MovementReductionPercent =>
        Mathf.Clamp(movementReductionPercent, 0.01f, 0.95f);

    public SlowTierBalanceValues(float movementReductionPercent)
    {
        this.movementReductionPercent = movementReductionPercent;
    }

    public void Validate()
    {
        movementReductionPercent =
            Mathf.Clamp(movementReductionPercent, 0.01f, 0.95f);
    }
}

[Serializable]
public sealed class SlowBalanceValues
{
    [SerializeField, Min(0.1f)] private float duration = 2f;
    [SerializeField] private SlowTierBalanceValues stack1 = new SlowTierBalanceValues(0.15f);
    [SerializeField] private SlowTierBalanceValues stack2 = new SlowTierBalanceValues(0.30f);

    public float Duration => Mathf.Max(0.1f, duration);

    public int MaximumStacks
    {
        get
        {
            int stackCount = 0;

            if (stack1 != null) stackCount++;
            if (stack2 != null) stackCount++;

            return Mathf.Max(1, stackCount);
        }
    }

    public SlowTierBalanceValues GetTier(int stackNumber)
    {
        switch (stackNumber)
        {
            case 1: return stack1;
            case 2: return stack2;
            default: return null;
        }
    }

    public void Validate()
    {
        duration = Mathf.Max(0.1f, duration);

        if (stack1 == null) stack1 = new SlowTierBalanceValues(0.15f);
        if (stack2 == null) stack2 = new SlowTierBalanceValues(0.30f);

        stack1.Validate();
        stack2.Validate();
    }
}


[Serializable]
public sealed class BossCoreBalanceValues
{
    [SerializeField, Min(1)] private int baseMaxHealth = 550;
    [SerializeField, Min(0.01f)] private float moveSpeed = 2f;
    [SerializeField, Min(0f)] private float stoppingDistance = 2.2f;

    [SerializeField, Min(0f)] private float initialAttackDelay = 2f;

    [SerializeField]
    private bool requireMediumAttackBeforeLong = true;

    [SerializeField, Min(0f)] private float meleeAttackRange = 2.5f;
    [SerializeField, Min(0f)] private float mediumAttackRange = 7f;
    [SerializeField, Min(0f)] private float longAttackRange = 20f;

    [SerializeField, Min(0f)] private float meleeAttackCooldown = 3f;
    [SerializeField, Min(0f)] private float mediumAttackCooldown = 5f;
    [SerializeField, Min(0f)] private float longAttackCooldown = 7f;

    [SerializeField, Min(0f)] private float meleeAttackWindup = 0.75f;
    [SerializeField, Min(0f)] private float mediumAttackWindup = 1f;
    [SerializeField, Min(0f)] private float longAttackWindup = 1.2f;

    [SerializeField, Min(0f)] private float recoveryDuration = 0.8f;

    public int BaseMaxHealth => Mathf.Max(1, baseMaxHealth);
    public float MoveSpeed => Mathf.Max(0.01f, moveSpeed);
    public float StoppingDistance => Mathf.Max(0f, stoppingDistance);
    public float InitialAttackDelay => Mathf.Max(0f, initialAttackDelay);
    public bool RequireMediumAttackBeforeLong => requireMediumAttackBeforeLong;

    public float MeleeAttackRange => Mathf.Max(0f, meleeAttackRange);
    public float MediumAttackRange => Mathf.Max(MeleeAttackRange, mediumAttackRange);
    public float LongAttackRange => Mathf.Max(MediumAttackRange, longAttackRange);

    public float MeleeAttackCooldown => Mathf.Max(0f, meleeAttackCooldown);
    public float MediumAttackCooldown => Mathf.Max(0f, mediumAttackCooldown);
    public float LongAttackCooldown => Mathf.Max(0f, longAttackCooldown);

    public float MeleeAttackWindup => Mathf.Max(0f, meleeAttackWindup);
    public float MediumAttackWindup => Mathf.Max(0f, mediumAttackWindup);
    public float LongAttackWindup => Mathf.Max(0f, longAttackWindup);
    public float RecoveryDuration => Mathf.Max(0f, recoveryDuration);

    public void Validate()
    {
        baseMaxHealth = Mathf.Max(1, baseMaxHealth);
        moveSpeed = Mathf.Max(0.01f, moveSpeed);
        stoppingDistance = Mathf.Max(0f, stoppingDistance);
        initialAttackDelay = Mathf.Max(0f, initialAttackDelay);

        meleeAttackRange = Mathf.Max(0f, meleeAttackRange);
        mediumAttackRange = Mathf.Max(meleeAttackRange, mediumAttackRange);
        longAttackRange = Mathf.Max(mediumAttackRange, longAttackRange);

        meleeAttackCooldown = Mathf.Max(0f, meleeAttackCooldown);
        mediumAttackCooldown = Mathf.Max(0f, mediumAttackCooldown);
        longAttackCooldown = Mathf.Max(0f, longAttackCooldown);

        meleeAttackWindup = Mathf.Max(0f, meleeAttackWindup);
        mediumAttackWindup = Mathf.Max(0f, mediumAttackWindup);
        longAttackWindup = Mathf.Max(0f, longAttackWindup);
        recoveryDuration = Mathf.Max(0f, recoveryDuration);
    }
}


[Serializable]
public sealed class BossMeleeAttackBalanceValues
{
    [SerializeField, Min(0)] private int playerDamage = 35;
    [SerializeField, Min(0)] private int coreTowerDamage = 60;
    [SerializeField, Min(0f)] private float attackRadius = 2.5f;

    public int PlayerDamage => Mathf.Max(0, playerDamage);
    public int CoreTowerDamage => Mathf.Max(0, coreTowerDamage);
    public float AttackRadius => Mathf.Max(0f, attackRadius);

    public void Validate()
    {
        playerDamage = Mathf.Max(0, playerDamage);
        coreTowerDamage = Mathf.Max(0, coreTowerDamage);
        attackRadius = Mathf.Max(0f, attackRadius);
    }
}


[Serializable]
public sealed class BossMediumLineAttackBalanceValues
{
    [SerializeField, Min(0)] private int playerDamage = 45;
    [SerializeField, Min(0)] private int coreTowerDamage = 120;

    [SerializeField, Min(0f)] private float lineLength = 8f;
    [SerializeField, Min(0f)] private float lineWidth = 2.5f;
    [SerializeField, Min(0f)] private float attackHeight = 3f;

    public int PlayerDamage => Mathf.Max(0, playerDamage);
    public int CoreTowerDamage => Mathf.Max(0, coreTowerDamage);
    public float LineLength => Mathf.Max(0f, lineLength);
    public float LineWidth => Mathf.Max(0f, lineWidth);
    public float AttackHeight => Mathf.Max(0f, attackHeight);

    public void Validate()
    {
        playerDamage = Mathf.Max(0, playerDamage);
        coreTowerDamage = Mathf.Max(0, coreTowerDamage);

        lineLength = Mathf.Max(0f, lineLength);
        lineWidth = Mathf.Max(0f, lineWidth);
        attackHeight = Mathf.Max(0f, attackHeight);
    }
}


[Serializable]
public sealed class BossLongZonesAttackBalanceValues
{
    [SerializeField, Min(0)] private int playerDamage = 55;

    [SerializeField, Min(0f)] private float zoneRadius = 1.75f;
    [SerializeField, Min(0f)] private float randomPlacementRadius = 4f;
    [SerializeField, Min(0f)] private float minimumZoneSpacing = 2.5f;

    public int PlayerDamage => Mathf.Max(0, playerDamage);
    public float ZoneRadius => Mathf.Max(0f, zoneRadius);
    public float RandomPlacementRadius => Mathf.Max(0f, randomPlacementRadius);
    public float MinimumZoneSpacing => Mathf.Max(0f, minimumZoneSpacing);

    public void Validate()
    {
        playerDamage = Mathf.Max(0, playerDamage);

        zoneRadius = Mathf.Max(0f, zoneRadius);
        randomPlacementRadius = Mathf.Max(0f, randomPlacementRadius);
        minimumZoneSpacing = Mathf.Max(0f, minimumZoneSpacing);
    }
}


[Serializable]
public sealed class BossAttackPayloadBalanceValues
{
    [SerializeField]
    private BossMeleeAttackBalanceValues melee =
        new BossMeleeAttackBalanceValues();

    [SerializeField]
    private BossMediumLineAttackBalanceValues mediumLine =
        new BossMediumLineAttackBalanceValues();

    [SerializeField]
    private BossLongZonesAttackBalanceValues longZones =
        new BossLongZonesAttackBalanceValues();

    public BossMeleeAttackBalanceValues Melee => melee;
    public BossMediumLineAttackBalanceValues MediumLine => mediumLine;
    public BossLongZonesAttackBalanceValues LongZones => longZones;

    public void Validate()
    {
        if (melee == null)
        {
            melee = new BossMeleeAttackBalanceValues();
        }

        if (mediumLine == null)
        {
            mediumLine = new BossMediumLineAttackBalanceValues();
        }

        if (longZones == null)
        {
            longZones = new BossLongZonesAttackBalanceValues();
        }

        melee.Validate();
        mediumLine.Validate();
        longZones.Validate();
    }
}


[Serializable]
public sealed class BossContinueBalanceValues
{
    [SerializeField, Range(1, 100)]
    private int continueHealthPercent = 100;

    [SerializeField, Min(0f)]
    private float playerAttackDelay = 3f;

    [SerializeField, Min(0f)]
    private float bossMovementDelay = 2f;

    [SerializeField, Min(0f)]
    private float bossAttackDelay = 3f;

    public int ContinueHealthPercent =>
        Mathf.Clamp(continueHealthPercent, 1, 100);

    public float PlayerAttackDelay =>
        Mathf.Max(0f, playerAttackDelay);

    public float BossMovementDelay =>
        Mathf.Max(0f, bossMovementDelay);

    public float BossAttackDelay =>
        Mathf.Max(0f, bossAttackDelay);

    public void Validate()
    {
        continueHealthPercent =
            Mathf.Clamp(continueHealthPercent, 1, 100);

        playerAttackDelay =
            Mathf.Max(0f, playerAttackDelay);

        bossMovementDelay =
            Mathf.Max(0f, bossMovementDelay);

        bossAttackDelay =
            Mathf.Max(0f, bossAttackDelay);
    }
}


public enum BetweenWaveRewardType
{
    None,
    HeroUpgrade,
    DefenseTowerUpgrade,
    HealthRestore
}


public enum WaveEnemyType
{
    Melee,
    Ranged,
    Strong,
    Siege,
    Boss
}

[Serializable]
public sealed class WaveSpawnGroupBalanceValues
{
    [SerializeField]
    private WaveEnemyType enemyType = WaveEnemyType.Melee;

    [SerializeField, Min(1)]
    private int enemyCount = 1;

    [SerializeField, Min(0f)]
    private float spawnInterval = 1.5f;

    public WaveEnemyType EnemyType =>
        enemyType;

    public int EnemyCount =>
        Mathf.Max(1, enemyCount);

    public float SpawnInterval =>
        Mathf.Max(0f, spawnInterval);

    public WaveSpawnGroupBalanceValues(
        WaveEnemyType enemyType,
        int enemyCount,
        float spawnInterval
    )
    {
        this.enemyType = enemyType;
        this.enemyCount = enemyCount;
        this.spawnInterval = spawnInterval;
    }

    public void Validate()
    {
        enemyCount = Mathf.Max(1, enemyCount);
        spawnInterval = Mathf.Max(0f, spawnInterval);
    }
}

[Serializable]
public sealed class WaveCompositionValues
{
    [SerializeField]
    private string waveName = "Wave";

    [SerializeField]
    private List<WaveSpawnGroupBalanceValues> enemyGroups =
        new List<WaveSpawnGroupBalanceValues>();

    public string WaveName =>
        string.IsNullOrWhiteSpace(waveName)
            ? "Wave"
            : waveName;

    public IReadOnlyList<WaveSpawnGroupBalanceValues> EnemyGroups =>
        enemyGroups;

    public WaveCompositionValues(
        string waveName,
        params WaveSpawnGroupBalanceValues[] enemyGroups
    )
    {
        this.waveName = waveName;
        this.enemyGroups = enemyGroups != null
            ? new List<WaveSpawnGroupBalanceValues>(enemyGroups)
            : new List<WaveSpawnGroupBalanceValues>();
    }

    public void Validate(int waveNumber)
    {
        if (string.IsNullOrWhiteSpace(waveName) ||
            waveName == "Wave")
        {
            waveName = $"Wave {waveNumber}";
        }

        if (enemyGroups == null)
        {
            enemyGroups =
                new List<WaveSpawnGroupBalanceValues>();
        }

        for (int i = 0; i < enemyGroups.Count; i++)
        {
            enemyGroups[i]?.Validate();
        }
    }
}

[CreateAssetMenu(
    fileName = "GameBalanceConfig",
    menuName = "Hell Tower/Progression/Game Balance Config"
)]
public class GameBalanceConfig : ScriptableObject
{
    private const int DefaultWaveCount = 15;

    [Header("Hero - Base Stats")]
    [SerializeField, Min(1)] private int heroBaseMaxHealth = 100;
    [SerializeField, Min(1)] private int heroBaseDamage = 20;
    [SerializeField, Min(0.01f)] private float heroBaseMoveSpeed = 5f;
    [SerializeField, Min(0.01f)] private float heroBaseAttackInterval = 1f;
    [SerializeField, Min(0.1f)] private float heroAttackRadius = 5.5f;
    [SerializeField, Min(0.01f)] private float heroBaseAttackWindup = 0.2f;
    [SerializeField, Min(0.01f)] private float heroProjectileSpeed = 12f;
    [SerializeField, Range(0f, 100f)] private float heroBaseCritChancePercent = 3f;
    [SerializeField, Min(1f)] private float heroCritDamageMultiplier = 2f;

    [Header("Permanent Progression - General")]
    [SerializeField, Min(1)] private int purchasesPerRequiredBranch = 10;

    [Header("Permanent Progression - Hero Damage")]
    [SerializeField, Min(0.01f)] private float heroDamagePercentPerPurchase = 10f;
    [SerializeField, Min(1)] private int heroDamageStartingPrice = 100;
    [SerializeField, Min(0)] private int heroDamagePriceIncreasePerPurchase = 75;

    [Header("Permanent Progression - Critical Chance")]
    [SerializeField, Min(1)] private int heroCriticalChanceUnlockLevel = 3;
    [SerializeField, Min(0.01f)] private float heroCriticalChancePointsPerPurchase = 0.5f;
    [SerializeField, Min(1)] private int heroCriticalChanceStartingPrice = 100;
    [SerializeField, Min(0)] private int heroCriticalChancePriceIncreasePerPurchase = 75;

    [Header("Permanent Progression - Attack Speed")]
    [SerializeField, Min(1)] private int heroAttackSpeedUnlockLevel = 5;
    [SerializeField, Min(0.01f)] private float heroAttackSpeedPercentPerPurchase = 2f;
    [SerializeField, Min(1)] private int heroAttackSpeedStartingPrice = 100;
    [SerializeField, Min(0)] private int heroAttackSpeedPriceIncreasePerPurchase = 75;

    [Header("Permanent Progression - Shared Maximum Health")]
    [SerializeField, Min(1)] private int sharedMaxHealthPurchasesPerHeroLevel = 5;
    [SerializeField, Min(0.01f)] private float sharedMaxHealthPercentPerPurchase = 5f;
    [SerializeField, Min(1)] private int sharedMaxHealthStartingPrice = 100;
    [SerializeField, Min(0)] private int sharedMaxHealthPriceIncreasePerPurchase = 75;

    [Header("Core Tower - Base Stats")]
    [SerializeField, Min(1)] private int coreTowerBaseMaxHealth = 350;

    [Header("Defense Tower - Base Stats")]
    [SerializeField, Min(1)] private int defenseTowerBaseMaxHealth = 300;
    [SerializeField, Range(0f, 1f)] private float defenseTowerDamageCoefficient = 0.5f;
    [SerializeField, Min(0.01f)] private float defenseTowerBaseAttackInterval = 1f;
    [SerializeField, Min(0.1f)] private float defenseTowerAttackRadius = 6f;
    [SerializeField, Min(0.01f)] private float defenseTowerProjectileSpeed = 12f;

    [Header("Enemy - Player Aggro")]
    [SerializeField, Min(0f)] private float enemyAggroRadius = 4f;
    [SerializeField, Min(0f)] private float enemyDisengageRadius = 5f;
    [SerializeField, Min(0f)] private float enemyAggroConfirmationTime = 0.3f;

    [Header("Enemy - Melee")]
    [SerializeField]
    private EnemyBalanceValues meleeEnemy =
        new EnemyBalanceValues(35, 15, 2f, 1f, 0.5f);

    [Header("Enemy - Ranged")]
    [SerializeField]
    private EnemyBalanceValues rangedEnemy =
        new EnemyBalanceValues(50, 10, 2f, 1.5f, 0.5f);

    [SerializeField, Min(0.01f)]
    private float rangedEnemyProjectileSpeed = 8f;

    [Header("Enemy - Strong")]
    [SerializeField]
    private EnemyBalanceValues strongEnemy =
        new EnemyBalanceValues(100, 25, 1.5f, 1.4f, 0.8f);

    [Header("Enemy - Siege")]
    [SerializeField]
    private EnemyBalanceValues siegeEnemy =
        new EnemyBalanceValues(100, 50, 1.5f, 1.5f, 0.6f);

    [SerializeField, Min(0.01f)]
    private float siegeDefenseTowerDamageMultiplier = 1f;

    [SerializeField, Min(0.01f)]
    private float siegeCoreTowerDamageMultiplier = 1f;

    [Header("Boss - First Boss Core & Controller")]
    [SerializeField]
    private BossCoreBalanceValues firstBossCore =
        new BossCoreBalanceValues();

    [Header("Boss - First Boss Attack Payloads")]
    [SerializeField]
    private BossAttackPayloadBalanceValues firstBossAttacks =
        new BossAttackPayloadBalanceValues();

    [Header("Boss - First Boss Continue / Revive")]
    [SerializeField]
    private BossContinueBalanceValues firstBossContinue =
        new BossContinueBalanceValues();

    [Header("Run Flow - Pre-Wave Preparation")]
    [SerializeField, Min(0f)] private float preWavePreparationDuration = 20f;
    [SerializeField, Min(0f)] private float firstWaveStartDelay = 2f;
    [SerializeField, Min(1)] private int baseAvailableDefenseTowerSlots = 1;

    [Header("Run Flow - Between-Wave Rewards")]
    [FormerlySerializedAs("betweenWaveHealthRestorePercent")]
    [SerializeField, Range(0f, 100f)] private float betweenWaveHeroHealthRestorePercent = 20f;
    [SerializeField, Range(0f, 100f)] private float betweenWaveCoreTowerHealthRestorePercent = 15f;
    [SerializeField, Range(0f, 100f)] private float betweenWaveDefenseTowerHealthRestorePercent = 15f;
    [SerializeField, Min(0f)] private float betweenWaveNextWaveDelay = 0.5f;
    [SerializeField, Min(0f)] private float betweenWaveHealthRewardDelay = 2f;

    [SerializeField]
    private List<BetweenWaveRewardType> betweenWaveRewardSchedule =
        new List<BetweenWaveRewardType>();

    [Header("Waves - Composition")]
    [SerializeField]
    private List<WaveCompositionValues> waveComposition =
        new List<WaveCompositionValues>();

    [Header("Waves - Enemy Scaling")]
    [SerializeField]
    private List<WaveBalanceValues> waveBalance =
        new List<WaveBalanceValues>();

    [Header("Run Upgrades - Selection Settings")]
    [SerializeField, Min(1)] private int runUpgradeUnlockHeroLevel = 2;
    [SerializeField, Range(2, 3)] private int runUpgradeOfferedOptionCount = 2;

    [Header("Run Upgrades - Definitions")]
    [SerializeField]
    private List<RunUpgradeDefinition> runUpgradeDefinitions =
        new List<RunUpgradeDefinition>();

    [Header("Run Upgrades - Double Shot")]
    [SerializeField]
    private DoubleShotBalanceValues heroDoubleShot =
        new DoubleShotBalanceValues(0.85f, 1.10f, 0.10f, 0.08f);

    [Header("Run Upgrades - Ricochet")]
    [SerializeField]
    private RicochetBalanceValues heroRicochet =
        new RicochetBalanceValues();

    [Header("Run Upgrades - Poison")]
    [SerializeField]
    private PoisonBalanceValues heroPoison =
        new PoisonBalanceValues(0.50f, 0.33f, 0.05f, 5f, 2.5f);

    [Header("Run Upgrades - Stun")]
    [SerializeField]
    private StunBalanceValues heroStun =
        new StunBalanceValues();

    [Header("Run Upgrades - Slow")]
    [SerializeField]
    private SlowBalanceValues heroSlow =
        new SlowBalanceValues();

    public int HeroBaseMaxHealth => Mathf.Max(1, heroBaseMaxHealth);
    public int HeroBaseDamage => Mathf.Max(1, heroBaseDamage);
    public float HeroBaseMoveSpeed => Mathf.Max(0.01f, heroBaseMoveSpeed);
    public float HeroBaseAttackInterval => Mathf.Max(0.01f, heroBaseAttackInterval);
    public float HeroAttackRadius => Mathf.Max(0.1f, heroAttackRadius);
    public float HeroBaseAttackWindup => Mathf.Max(0.01f, heroBaseAttackWindup);
    public float HeroProjectileSpeed => Mathf.Max(0.01f, heroProjectileSpeed);
    public float HeroBaseCritChancePercent => Mathf.Clamp(heroBaseCritChancePercent, 0f, 100f);
    public float HeroCritDamageMultiplier => Mathf.Max(1f, heroCritDamageMultiplier);

    public int CoreTowerBaseMaxHealth => Mathf.Max(1, coreTowerBaseMaxHealth);
    public int DefenseTowerBaseMaxHealth => Mathf.Max(1, defenseTowerBaseMaxHealth);
    public float DefenseTowerDamageCoefficient => Mathf.Clamp01(defenseTowerDamageCoefficient);
    public float DefenseTowerBaseAttackInterval => Mathf.Max(0.01f, defenseTowerBaseAttackInterval);
    public float DefenseTowerAttackRadius => Mathf.Max(0.1f, defenseTowerAttackRadius);
    public float DefenseTowerProjectileSpeed => Mathf.Max(0.01f, defenseTowerProjectileSpeed);

    public float EnemyAggroRadius => Mathf.Max(0f, enemyAggroRadius);
    public float EnemyDisengageRadius => Mathf.Max(EnemyAggroRadius, enemyDisengageRadius);
    public float EnemyAggroConfirmationTime => Mathf.Max(0f, enemyAggroConfirmationTime);

    public EnemyBalanceValues MeleeEnemy => meleeEnemy;
    public EnemyBalanceValues RangedEnemy => rangedEnemy;
    public float RangedEnemyProjectileSpeed => Mathf.Max(0.01f, rangedEnemyProjectileSpeed);
    public EnemyBalanceValues StrongEnemy => strongEnemy;
    public EnemyBalanceValues SiegeEnemy => siegeEnemy;
    public float SiegeDefenseTowerDamageMultiplier =>
        siegeDefenseTowerDamageMultiplier > 0f
            ? siegeDefenseTowerDamageMultiplier
            : 1f;

    public float SiegeCoreTowerDamageMultiplier =>
        siegeCoreTowerDamageMultiplier > 0f
            ? siegeCoreTowerDamageMultiplier
            : 1f;

    public BossCoreBalanceValues FirstBossCore => firstBossCore;
    public BossAttackPayloadBalanceValues FirstBossAttacks => firstBossAttacks;
    public BossContinueBalanceValues FirstBossContinue => firstBossContinue;

    public float PreWavePreparationDuration => Mathf.Max(0f, preWavePreparationDuration);
    public float FirstWaveStartDelay => Mathf.Max(0f, firstWaveStartDelay);
    public int BaseAvailableDefenseTowerSlots => Mathf.Max(1, baseAvailableDefenseTowerSlots);

    public float BetweenWaveHeroHealthRestorePercent =>
        Mathf.Clamp(betweenWaveHeroHealthRestorePercent, 0f, 100f);

    public float BetweenWaveCoreTowerHealthRestorePercent =>
        Mathf.Clamp(betweenWaveCoreTowerHealthRestorePercent, 0f, 100f);

    public float BetweenWaveDefenseTowerHealthRestorePercent =>
        Mathf.Clamp(betweenWaveDefenseTowerHealthRestorePercent, 0f, 100f);

    public float BetweenWaveNextWaveDelay =>
        Mathf.Max(0f, betweenWaveNextWaveDelay);

    public float BetweenWaveHealthRewardDelay =>
        Mathf.Max(0f, betweenWaveHealthRewardDelay);

    public BetweenWaveRewardType GetBetweenWaveRewardType(
        int completedWave
    )
    {
        if (betweenWaveRewardSchedule == null)
        {
            return BetweenWaveRewardType.None;
        }

        int index = completedWave - 1;

        if (index < 0 ||
            index >= betweenWaveRewardSchedule.Count)
        {
            return BetweenWaveRewardType.None;
        }

        return betweenWaveRewardSchedule[index];
    }

    public int WaveCompositionCount =>
        waveComposition == null ? 0 : waveComposition.Count;

    public WaveCompositionValues GetWaveComposition(
        int waveNumber
    )
    {
        if (waveComposition == null)
        {
            return null;
        }

        int index = waveNumber - 1;

        if (index < 0 ||
            index >= waveComposition.Count)
        {
            return null;
        }

        return waveComposition[index];
    }

    public int WaveBalanceCount => waveBalance == null ? 0 : waveBalance.Count;

    public WaveBalanceValues GetWaveBalance(int waveNumber)
    {
        if (waveBalance == null)
        {
            return null;
        }

        int index = waveNumber - 1;

        if (index < 0 || index >= waveBalance.Count)
        {
            return null;
        }

        return waveBalance[index];
    }

    public int RunUpgradeUnlockHeroLevel => Mathf.Max(1, runUpgradeUnlockHeroLevel);
    public int RunUpgradeOfferedOptionCount => Mathf.Clamp(runUpgradeOfferedOptionCount, 2, 3);
    public IReadOnlyList<RunUpgradeDefinition> RunUpgradeDefinitions => runUpgradeDefinitions;
    public DoubleShotBalanceValues HeroDoubleShot => heroDoubleShot;
    public RicochetBalanceValues HeroRicochet => heroRicochet;
    public PoisonBalanceValues HeroPoison => heroPoison;
    public StunBalanceValues HeroStun => heroStun;
    public SlowBalanceValues HeroSlow => heroSlow;

    public int GetRunUpgradeMaximumStacks(
        RunUpgradeDefinition definition
    )
    {
        if (definition == null)
        {
            return 0;
        }

        switch (definition.UpgradeType)
        {
            case RunUpgradeType.HeroDoubleShot:
                return HeroDoubleShot != null
                    ? HeroDoubleShot.MaximumStacks
                    : 1;

            case RunUpgradeType.HeroRicochet:
                return HeroRicochet != null
                    ? HeroRicochet.MaximumStacks
                    : 1;

            case RunUpgradeType.HeroPoison:
                return HeroPoison != null
                    ? HeroPoison.MaximumStacks
                    : 1;

            case RunUpgradeType.HeroStun:
                return HeroStun != null
                    ? HeroStun.MaximumStacks
                    : 1;

            case RunUpgradeType.HeroSlow:
                return HeroSlow != null
                    ? HeroSlow.MaximumStacks
                    : 1;

            default:
                return definition.MaximumStacks;
        }
    }

    public int PurchasesPerRequiredBranch => Mathf.Max(1, purchasesPerRequiredBranch);
    public float HeroDamagePercentPerPurchase => Mathf.Max(0.01f, heroDamagePercentPerPurchase);
    public int HeroCriticalChanceUnlockLevel => Mathf.Max(1, heroCriticalChanceUnlockLevel);
    public float HeroCriticalChancePointsPerPurchase => Mathf.Max(0.01f, heroCriticalChancePointsPerPurchase);
    public int HeroAttackSpeedUnlockLevel => Mathf.Max(1, heroAttackSpeedUnlockLevel);
    public float HeroAttackSpeedPercentPerPurchase => Mathf.Max(0.01f, heroAttackSpeedPercentPerPurchase);
    public int SharedMaxHealthPurchasesPerHeroLevel => Mathf.Max(1, sharedMaxHealthPurchasesPerHeroLevel);
    public float SharedMaxHealthPercentPerPurchase => Mathf.Max(0.01f, sharedMaxHealthPercentPerPurchase);

    public int GetHeroDamagePurchasePrice(int currentPurchaseCount)
    {
        return CalculatePurchasePrice(
            heroDamageStartingPrice,
            heroDamagePriceIncreasePerPurchase,
            currentPurchaseCount
        );
    }

    public int GetHeroCriticalChancePurchasePrice(int currentPurchaseCount)
    {
        return CalculatePurchasePrice(
            heroCriticalChanceStartingPrice,
            heroCriticalChancePriceIncreasePerPurchase,
            currentPurchaseCount
        );
    }

    public int GetHeroAttackSpeedPurchasePrice(int currentPurchaseCount)
    {
        return CalculatePurchasePrice(
            heroAttackSpeedStartingPrice,
            heroAttackSpeedPriceIncreasePerPurchase,
            currentPurchaseCount
        );
    }

    public int GetSharedMaxHealthPurchasePrice(int currentPurchaseCount)
    {
        return CalculatePurchasePrice(
            sharedMaxHealthStartingPrice,
            sharedMaxHealthPriceIncreasePerPurchase,
            currentPurchaseCount
        );
    }

    public int GetRequiredHeroDamagePurchases(int currentHeroLevel)
    {
        int safeHeroLevel = Mathf.Max(1, currentHeroLevel);

        return MultiplyAndClampToIntMax(
            safeHeroLevel,
            PurchasesPerRequiredBranch
        );
    }

    public int GetRequiredHeroCriticalChancePurchases(int currentHeroLevel)
    {
        int safeHeroLevel = Mathf.Max(1, currentHeroLevel);

        if (safeHeroLevel < HeroCriticalChanceUnlockLevel)
        {
            return 0;
        }

        int unlockedLevelCount =
            safeHeroLevel -
            HeroCriticalChanceUnlockLevel +
            1;

        return MultiplyAndClampToIntMax(
            unlockedLevelCount,
            PurchasesPerRequiredBranch
        );
    }

    public int GetRequiredHeroAttackSpeedPurchases(int currentHeroLevel)
    {
        int safeHeroLevel = Mathf.Max(1, currentHeroLevel);

        if (safeHeroLevel < HeroAttackSpeedUnlockLevel)
        {
            return 0;
        }

        int unlockedLevelCount =
            safeHeroLevel -
            HeroAttackSpeedUnlockLevel +
            1;

        return MultiplyAndClampToIntMax(
            unlockedLevelCount,
            PurchasesPerRequiredBranch
        );
    }

    public int GetSharedMaxHealthPurchaseLimit(int currentHeroLevel)
    {
        int safeHeroLevel = Mathf.Max(1, currentHeroLevel);

        return MultiplyAndClampToIntMax(
            safeHeroLevel,
            SharedMaxHealthPurchasesPerHeroLevel
        );
    }

    public float GetTotalHeroDamageBonus(int purchaseCount)
    {
        int safePurchaseCount = Mathf.Max(0, purchaseCount);

        double bonusPercent =
            safePurchaseCount *
            (double)HeroDamagePercentPerPurchase;

        double bonusDamage =
            HeroBaseDamage *
            bonusPercent /
            100d;

        return (float)Math.Min(float.MaxValue, bonusDamage);
    }

    public float GetTotalHeroCriticalChanceBonusPoints(int purchaseCount)
    {
        int safePurchaseCount = Mathf.Max(0, purchaseCount);

        double bonus =
            safePurchaseCount *
            (double)HeroCriticalChancePointsPerPurchase;

        return (float)Math.Min(float.MaxValue, bonus);
    }

    public float GetTotalHeroAttackSpeedBonusPercent(int purchaseCount)
    {
        int safePurchaseCount = Mathf.Max(0, purchaseCount);

        double bonus =
            safePurchaseCount *
            (double)HeroAttackSpeedPercentPerPurchase;

        return (float)Math.Min(float.MaxValue, bonus);
    }

    public float GetTotalSharedMaxHealthBonusPercent(int purchaseCount)
    {
        int safePurchaseCount = Mathf.Max(0, purchaseCount);

        double bonus =
            safePurchaseCount *
            (double)SharedMaxHealthPercentPerPurchase;

        return (float)Math.Min(float.MaxValue, bonus);
    }

    private void OnEnable()
    {
        EnsureFirstBossCoreBalance();
        EnsureFirstBossAttackPayloadBalance();
        EnsureFirstBossContinueBalance();
        EnsureBetweenWaveRewardSchedule();
        EnsureWaveCompositionList();
        EnsureWaveBalanceList();
        EnsureDefaultRunUpgradeDefinitions();
        EnsureDoubleShotBalance();
        EnsureRicochetBalance();
        EnsurePoisonBalance();
        EnsureStunBalance();
        EnsureSlowBalance();
    }

    private void OnValidate()
    {
        heroBaseMoveSpeed = Mathf.Max(0.01f, heroBaseMoveSpeed);
        defenseTowerAttackRadius = Mathf.Max(0.1f, defenseTowerAttackRadius);
        rangedEnemyProjectileSpeed = Mathf.Max(0.01f, rangedEnemyProjectileSpeed);

        enemyAggroRadius = Mathf.Max(0f, enemyAggroRadius);
        enemyDisengageRadius = Mathf.Max(enemyAggroRadius, enemyDisengageRadius);
        enemyAggroConfirmationTime = Mathf.Max(0f, enemyAggroConfirmationTime);

        heroCriticalChanceUnlockLevel = Mathf.Max(1, heroCriticalChanceUnlockLevel);
        heroAttackSpeedUnlockLevel = Mathf.Max(1, heroAttackSpeedUnlockLevel);

        preWavePreparationDuration = Mathf.Max(0f, preWavePreparationDuration);
        firstWaveStartDelay = Mathf.Max(0f, firstWaveStartDelay);
        baseAvailableDefenseTowerSlots = Mathf.Max(1, baseAvailableDefenseTowerSlots);

        betweenWaveHeroHealthRestorePercent = Mathf.Clamp(
            betweenWaveHeroHealthRestorePercent, 0f, 100f
        );
        betweenWaveCoreTowerHealthRestorePercent = Mathf.Clamp(
            betweenWaveCoreTowerHealthRestorePercent, 0f, 100f
        );
        betweenWaveDefenseTowerHealthRestorePercent = Mathf.Clamp(
            betweenWaveDefenseTowerHealthRestorePercent, 0f, 100f
        );
        betweenWaveNextWaveDelay = Mathf.Max(0f, betweenWaveNextWaveDelay);
        betweenWaveHealthRewardDelay = Mathf.Max(0f, betweenWaveHealthRewardDelay);

        runUpgradeUnlockHeroLevel = Mathf.Max(1, runUpgradeUnlockHeroLevel);
        runUpgradeOfferedOptionCount = Mathf.Clamp(runUpgradeOfferedOptionCount, 2, 3);

        meleeEnemy?.Validate();
        rangedEnemy?.Validate();
        strongEnemy?.Validate();
        siegeEnemy?.Validate();

        if (siegeDefenseTowerDamageMultiplier <= 0f)
        {
            siegeDefenseTowerDamageMultiplier = 1f;
        }

        if (siegeCoreTowerDamageMultiplier <= 0f)
        {
            siegeCoreTowerDamageMultiplier = 1f;
        }

        EnsureFirstBossCoreBalance();
        EnsureFirstBossAttackPayloadBalance();
        EnsureFirstBossContinueBalance();
        EnsureBetweenWaveRewardSchedule();
        EnsureWaveCompositionList();
        EnsureWaveBalanceList();
        EnsureDefaultRunUpgradeDefinitions();
        EnsureDoubleShotBalance();
        EnsureRicochetBalance();
        EnsurePoisonBalance();
        EnsureStunBalance();
        EnsureSlowBalance();

        if (runUpgradeDefinitions != null)
        {
            foreach (RunUpgradeDefinition definition in runUpgradeDefinitions)
            {
                definition?.Validate();
            }
        }
    }

    private void EnsureFirstBossCoreBalance()
    {
        if (firstBossCore == null)
        {
            firstBossCore = new BossCoreBalanceValues();
        }

        firstBossCore.Validate();
    }

    private void EnsureFirstBossAttackPayloadBalance()
    {
        if (firstBossAttacks == null)
        {
            firstBossAttacks =
                new BossAttackPayloadBalanceValues();
        }

        firstBossAttacks.Validate();
    }

    private void EnsureFirstBossContinueBalance()
    {
        if (firstBossContinue == null)
        {
            firstBossContinue =
                new BossContinueBalanceValues();
        }

        firstBossContinue.Validate();
    }

    private void EnsureBetweenWaveRewardSchedule()
    {
        if (betweenWaveRewardSchedule == null)
        {
            betweenWaveRewardSchedule =
                new List<BetweenWaveRewardType>();
        }

        while (betweenWaveRewardSchedule.Count < DefaultWaveCount)
        {
            int waveNumber =
                betweenWaveRewardSchedule.Count + 1;

            betweenWaveRewardSchedule.Add(
                CreateDefaultBetweenWaveRewardType(
                    waveNumber
                )
            );
        }
    }

    private static BetweenWaveRewardType
        CreateDefaultBetweenWaveRewardType(
            int completedWave
        )
    {
        switch (completedWave)
        {
            case 1:
            case 3:
            case 6:
            case 8:
            case 11:
            case 13:
                return BetweenWaveRewardType.HeroUpgrade;

            case 2:
            case 4:
            case 7:
            case 9:
            case 12:
            case 14:
                return BetweenWaveRewardType.DefenseTowerUpgrade;

            case 5:
            case 10:
                return BetweenWaveRewardType.HealthRestore;

            default:
                return BetweenWaveRewardType.None;
        }
    }

    private void EnsureWaveCompositionList()
    {
        if (waveComposition == null)
        {
            waveComposition =
                new List<WaveCompositionValues>();
        }

        while (waveComposition.Count < DefaultWaveCount)
        {
            int waveNumber =
                waveComposition.Count + 1;

            waveComposition.Add(
                CreateDefaultWaveComposition(
                    waveNumber
                )
            );
        }

        for (int i = 0; i < waveComposition.Count; i++)
        {
            if (waveComposition[i] == null)
            {
                waveComposition[i] =
                    CreateDefaultWaveComposition(
                        i + 1
                    );
            }

            waveComposition[i].Validate(
                i + 1
            );
        }
    }

    private static WaveCompositionValues
        CreateDefaultWaveComposition(
            int waveNumber
        )
    {
        string waveName =
            $"Wave {waveNumber}";

        switch (waveNumber)
        {
            case 1:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        5,
                        1.3f
                    )
                );

            case 2:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        7,
                        1.3f
                    )
                );

            case 3:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        5,
                        1.3f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        3,
                        2f
                    )
                );

            case 4:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        6,
                        1.3f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        5,
                        2f
                    )
                );

            case 5:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        5,
                        1.5f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        4,
                        2f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Strong,
                        1,
                        5f
                    )
                );

            case 6:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        6,
                        1.1f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        5,
                        1.5f
                    )
                );

            case 7:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        7,
                        1f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        5,
                        1.4f
                    )
                );

            case 8:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        6,
                        1f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        5,
                        1.4f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Strong,
                        2,
                        2.2f
                    )
                );

            case 9:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        6,
                        0.9f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        6,
                        1.2f
                    )
                );

            case 10:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        10,
                        0.9f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        8,
                        1.2f
                    )
                );

            case 11:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        7,
                        0.9f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        6,
                        1.2f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Strong,
                        3,
                        1.4f
                    )
                );

            case 12:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        10,
                        0.8f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        7,
                        1.1f
                    )
                );

            case 13:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        11,
                        0.8f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        7,
                        1f
                    )
                );

            case 14:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        9,
                        0.8f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Ranged,
                        6,
                        1f
                    ),
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Strong,
                        4,
                        1.3f
                    )
                );

            case 15:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Boss,
                        1,
                        2f
                    )
                );

            default:
                return new WaveCompositionValues(
                    waveName,
                    new WaveSpawnGroupBalanceValues(
                        WaveEnemyType.Melee,
                        1,
                        1.5f
                    )
                );
        }
    }

    private void EnsureWaveBalanceList()
    {
        if (waveBalance == null)
        {
            waveBalance = new List<WaveBalanceValues>();
        }

        while (waveBalance.Count < DefaultWaveCount)
        {
            int waveNumber = waveBalance.Count + 1;
            waveBalance.Add(CreateDefaultWaveBalance(waveNumber));
        }

        for (int i = 0; i < waveBalance.Count; i++)
        {
            if (waveBalance[i] == null)
            {
                waveBalance[i] = CreateDefaultWaveBalance(i + 1);
            }

            waveBalance[i].Validate();
        }
    }

    private static WaveBalanceValues CreateDefaultWaveBalance(int waveNumber)
    {
        if (waveNumber >= 1 && waveNumber <= 5)
        {
            return new WaveBalanceValues(1f, 1f);
        }

        if (waveNumber >= 6 && waveNumber <= 10)
        {
            return new WaveBalanceValues(1.1f, 1.05f);
        }

        if (waveNumber >= 11 && waveNumber <= 14)
        {
            return new WaveBalanceValues(1.2f, 1.1f);
        }

        return new WaveBalanceValues(1f, 1f);
    }

    private void EnsureDefaultRunUpgradeDefinitions()
    {
        if (runUpgradeDefinitions == null)
        {
            runUpgradeDefinitions = new List<RunUpgradeDefinition>();
        }

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroDamage,
            "DAMAGE",
            "+10% hero damage",
            10f,
            3
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroAttackSpeed,
            "ATTACK SPEED",
            "+10% hero attack speed",
            10f,
            3
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.TowerDamage,
            "TOWER DAMAGE",
            "Increases defense tower damage by 10%.",
            10f,
            3
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.TowerAttackSpeed,
            "TOWER ATTACK SPEED",
            "Increases defense tower attack speed by 10%.",
            10f,
            3
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroDamageStrong,
            "POWERFUL DAMAGE",
            "+20% hero damage",
            20f,
            2
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroAttackSpeedStrong,
            "RAPID ATTACK",
            "+20% hero attack speed",
            20f,
            2
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.TowerDamageStrong,
            "POWERFUL TOWER",
            "Increases defense tower damage by 20%.",
            20f,
            2
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.TowerAttackSpeedStrong,
            "RAPID TOWER",
            "Increases defense tower attack speed by 20%.",
            20f,
            2
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroCrit,
            "CRITICAL CHANCE",
            "Increases hero critical chance.",
            20f,
            3
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroCritStrong,
            "DEADLY PRECISION",
            "Greatly increases hero critical chance.",
            40f,
            2
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroHealth,
            "VITALITY",
            "Increases hero maximum health.",
            7f,
            3
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroHealthStrong,
            "GREATER VITALITY",
            "Greatly increases hero maximum health.",
            15f,
            2
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroDoubleShot,
            "DOUBLE SHOT",
            "Fires two projectiles at the same target. " +
            "Each projectile deals reduced damage and " +
            "the attack takes longer to prepare.",
            0f,
            1
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroRicochet,
            "RICOCHET",
            "Projectiles bounce to additional enemies. " +
            "Further stacks improve the number of targets " +
            "and ricochet damage.",
            0f,
            1
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroPoison,
            "POISON",
            "Projectile hits can poison enemies and deal " +
            "damage over time. Ricochet hits have a lower " +
            "chance to apply poison.",
            0f,
            1
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroStun,
            "STUN",
            "Projectile hits can briefly stop enemies. " +
            "A second stack increases the stun duration.",
            0f,
            1
        ));

        EnsureRunUpgradeDefinition(new RunUpgradeDefinition(
            RunUpgradeType.HeroSlow,
            "SLOW",
            "Projectile hits slow enemy movement. " +
            "A second stack increases the movement reduction.",
            0f,
            1
        ));
    }

    private void EnsureRunUpgradeDefinition(RunUpgradeDefinition definitionToAdd)
    {
        foreach (RunUpgradeDefinition definition in runUpgradeDefinitions)
        {
            if (definition != null &&
                definition.UpgradeType == definitionToAdd.UpgradeType)
            {
                return;
            }
        }

        runUpgradeDefinitions.Add(definitionToAdd);
    }

    private void EnsureDoubleShotBalance()
    {
        if (heroDoubleShot == null)
        {
            heroDoubleShot =
                new DoubleShotBalanceValues(0.85f, 1.10f, 0.10f, 0.08f);
        }

        heroDoubleShot.Validate();
    }

    private void EnsureRicochetBalance()
    {
        if (heroRicochet == null)
        {
            heroRicochet = new RicochetBalanceValues();
        }

        heroRicochet.Validate();
    }

    private void EnsurePoisonBalance()
    {
        if (heroPoison == null)
        {
            heroPoison =
                new PoisonBalanceValues(0.50f, 0.33f, 0.05f, 5f, 2.5f);
        }

        heroPoison.Validate();
    }

    private void EnsureStunBalance()
    {
        if (heroStun == null)
        {
            heroStun = new StunBalanceValues();
        }

        heroStun.Validate();
    }

    private void EnsureSlowBalance()
    {
        if (heroSlow == null)
        {
            heroSlow = new SlowBalanceValues();
        }

        heroSlow.Validate();
    }

    private static int CalculatePurchasePrice(
        int startingPrice,
        int priceIncreasePerPurchase,
        int currentPurchaseCount
    )
    {
        int safePurchaseCount = Mathf.Max(0, currentPurchaseCount);

        long price =
            (long)Mathf.Max(1, startingPrice) +
            (long)Mathf.Max(0, priceIncreasePerPurchase) *
            safePurchaseCount;

        return (int)Math.Min(int.MaxValue, price);
    }

    private static int MultiplyAndClampToIntMax(int firstValue, int secondValue)
    {
        long result =
            (long)firstValue *
            secondValue;

        return (int)Math.Min(int.MaxValue, result);
    }
}
