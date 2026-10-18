using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum RunUpgradeType
{
    // IMPORTANT:
    // Do not change the order of existing values.
    // Unity serializes enum values as integers.
    HeroDamage,
    HeroAttackSpeed,
    TowerDamage,
    TowerAttackSpeed,

    HeroDamageStrong,
    HeroAttackSpeedStrong,
    TowerDamageStrong,
    TowerAttackSpeedStrong,

    HeroCrit,
    HeroCritStrong,

    HeroHealth,
    HeroHealthStrong,

    HeroDoubleShot,
    HeroRicochet,
    HeroPoison,
    HeroStun,
    HeroSlow,

    // IMPORTANT:
    // New enum values must only be appended at the end.
    TowerHealth,
    TowerHealthStrong
}


public enum RunUpgradeCategory
{
    Hero,
    DefenseTower
}


[Serializable]
public sealed class RunUpgradeDefinition
{
    [SerializeField]
    private RunUpgradeType upgradeType;

    [SerializeField]
    private string displayName = "UPGRADE";

    [FormerlySerializedAs("description")]
    [SerializeField, TextArea(2, 4)]
    private string customDescription =
        "Upgrade description.";

    [SerializeField, Min(0f)]
    private float bonusPercent = 10f;

    [SerializeField, Range(1, 3)]
    private int maximumStacks = 3;


    public RunUpgradeType UpgradeType =>
        upgradeType;

    public string DisplayName =>
        displayName;

    public bool UsesDefinitionBonusPercent
    {
        get
        {
            switch (upgradeType)
            {
                case RunUpgradeType.HeroDamage:
                case RunUpgradeType.HeroAttackSpeed:
                case RunUpgradeType.TowerDamage:
                case RunUpgradeType.TowerAttackSpeed:

                case RunUpgradeType.HeroDamageStrong:
                case RunUpgradeType.HeroAttackSpeedStrong:
                case RunUpgradeType.TowerDamageStrong:
                case RunUpgradeType.TowerAttackSpeedStrong:

                case RunUpgradeType.HeroCrit:
                case RunUpgradeType.HeroCritStrong:

                case RunUpgradeType.HeroHealth:
                case RunUpgradeType.HeroHealthStrong:

                case RunUpgradeType.TowerHealth:
                case RunUpgradeType.TowerHealthStrong:

                    return true;

                default:
                    return false;
            }
        }
    }

    public bool UsesDefinitionMaximumStacks =>
        UsesDefinitionBonusPercent;

    public float BonusPercent
    {
        get
        {
            if (!UsesDefinitionBonusPercent)
            {
                return 0f;
            }

            return Mathf.Max(
                0.01f,
                bonusPercent
            );
        }
    }

    public int MaximumStacks
    {
        get
        {
            if (!UsesDefinitionMaximumStacks)
            {
                return 1;
            }

            return Mathf.Clamp(
                maximumStacks,
                1,
                3
            );
        }
    }


    public bool UsesAutomaticDescription =>
        UsesDefinitionBonusPercent;


    public string Description
    {
        get
        {
            if (UsesAutomaticDescription)
            {
                return BuildAutomaticDescription();
            }

            if (string.IsNullOrWhiteSpace(
                    customDescription
                ))
            {
                return "Upgrade description.";
            }

            return customDescription;
        }
    }


    public RunUpgradeCategory Category
    {
        get
        {
            switch (upgradeType)
            {
                case RunUpgradeType.TowerDamage:
                case RunUpgradeType.TowerAttackSpeed:
                case RunUpgradeType.TowerDamageStrong:
                case RunUpgradeType.TowerAttackSpeedStrong:
                case RunUpgradeType.TowerHealth:
                case RunUpgradeType.TowerHealthStrong:

                    return RunUpgradeCategory.DefenseTower;

                default:

                    return RunUpgradeCategory.Hero;
            }
        }
    }


    public RunUpgradeDefinition()
    {
    }


    public RunUpgradeDefinition(
        RunUpgradeType upgradeType,
        string displayName,
        string description,
        float bonusPercent,
        int maximumStacks
    )
    {
        this.upgradeType =
            upgradeType;

        this.displayName =
            displayName;

        customDescription =
            description;

        this.bonusPercent =
            bonusPercent;

        this.maximumStacks =
            Mathf.Clamp(
                maximumStacks,
                1,
                3
            );
    }


    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(
                displayName
            ))
        {
            displayName =
                upgradeType.ToString();
        }

        if (UsesDefinitionBonusPercent)
        {
            bonusPercent =
                Mathf.Max(
                    0.01f,
                    bonusPercent
                );
        }
        else
        {
            bonusPercent = 0f;
        }

        if (UsesDefinitionMaximumStacks)
        {
            maximumStacks =
                Mathf.Clamp(
                    maximumStacks,
                    1,
                    3
                );
        }
        else
        {
            maximumStacks = 1;
        }

        if (UsesAutomaticDescription)
        {
            customDescription =
                string.Empty;

            return;
        }

        if (string.IsNullOrWhiteSpace(
                customDescription
            ))
        {
            customDescription =
                "Upgrade description.";
        }
    }


    private string BuildAutomaticDescription()
    {
        string formattedPercent =
            BonusPercent.ToString(
                "0.##"
            );

        switch (upgradeType)
        {
            case RunUpgradeType.HeroDamage:
            case RunUpgradeType.HeroDamageStrong:

                return
                    "Increases hero damage by " +
                    formattedPercent +
                    "%.";


            case RunUpgradeType.HeroAttackSpeed:
            case RunUpgradeType.HeroAttackSpeedStrong:

                return
                    "Increases hero attack speed by " +
                    formattedPercent +
                    "%.";


            case RunUpgradeType.TowerDamage:
            case RunUpgradeType.TowerDamageStrong:

                return
                    "Increases defense tower damage by " +
                    formattedPercent +
                    "%.";


            case RunUpgradeType.TowerAttackSpeed:
            case RunUpgradeType.TowerAttackSpeedStrong:

                return
                    "Increases defense tower attack speed by " +
                    formattedPercent +
                    "%.";


            case RunUpgradeType.HeroCrit:
            case RunUpgradeType.HeroCritStrong:

                return
                    "Increases hero critical chance by " +
                    formattedPercent +
                    "%.";


            case RunUpgradeType.HeroHealth:
            case RunUpgradeType.HeroHealthStrong:

                return
                    "Increases hero maximum health by " +
                    formattedPercent +
                    "%.";


            case RunUpgradeType.TowerHealth:
            case RunUpgradeType.TowerHealthStrong:

                return
                    "Increases defense tower maximum health by " +
                    formattedPercent +
                    "%.";


            default:

                return customDescription;
        }
    }
}


[DisallowMultipleComponent]
public class RunUpgradeManager : MonoBehaviour
{
    [Header("System References")]

    [SerializeField]
    private CombatStats combatStats;

    [SerializeField]
    private PlayerProgression playerProgression;

    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private PlayerAttack playerAttack;


    [Header("Master Balance Source")]

    [SerializeField]
    private GameBalanceConfig balanceConfig;


    [Header("Debug Card Testing")]

    [Tooltip(
        "When enabled, only the selected upgrade types are " +
        "allowed to appear for the chosen category."
    )]
    [SerializeField]
    private bool useDebugTestPool;

    [SerializeField]
    private RunUpgradeCategory debugTestCategory =
        RunUpgradeCategory.Hero;

    [SerializeField]
    private List<RunUpgradeType>
        debugTestUpgradeTypes =
            new List<RunUpgradeType>();


    [Header("Debug - Do Not Edit")]

    [SerializeField]
    private bool selectionInProgress;

    [SerializeField]
    private RunUpgradeCategory debugCurrentCategory;

    [SerializeField]
    private int debugHeroDamageStacks;

    [SerializeField]
    private int debugHeroAttackSpeedStacks;

    [SerializeField]
    private int debugTowerDamageStacks;

    [SerializeField]
    private int debugTowerAttackSpeedStacks;

    [SerializeField]
    private int debugHeroDamageStrongStacks;

    [SerializeField]
    private int debugHeroAttackSpeedStrongStacks;

    [SerializeField]
    private int debugTowerDamageStrongStacks;

    [SerializeField]
    private int debugTowerAttackSpeedStrongStacks;

    [SerializeField]
    private int debugHeroCritStacks;

    [SerializeField]
    private int debugHeroCritStrongStacks;

    [SerializeField]
    private int debugHeroHealthStacks;

    [SerializeField]
    private int debugHeroHealthStrongStacks;

    [SerializeField]
    private int debugTowerHealthStacks;

    [SerializeField]
    private int debugTowerHealthStrongStacks;

    [SerializeField]
    private int debugHeroDoubleShotStacks;

    [SerializeField]
    private int debugHeroRicochetStacks;

    [SerializeField]
    private int debugHeroPoisonStacks;

    [SerializeField]
    private int debugHeroStunStacks;

    [SerializeField]
    private int debugHeroSlowStacks;

    [SerializeField]
    private int debugAvailableUpgradeCount;


    private readonly Dictionary<
        RunUpgradeType,
        int
    >
        stackCounts =
            new Dictionary<
                RunUpgradeType,
                int
            >();


    private readonly List<
        RunUpgradeDefinition
    >
        currentOptions =
            new List<
                RunUpgradeDefinition
            >();


    public bool IsSelectionUnlocked
    {
        get
        {
            if (playerProgression == null ||
                balanceConfig == null)
            {
                return false;
            }

            return
                playerProgression.HeroLevel >=
                balanceConfig.RunUpgradeUnlockHeroLevel;
        }
    }


    public bool IsSelectionInProgress =>
        selectionInProgress;


    public IReadOnlyList<RunUpgradeDefinition>
        CurrentOptions =>
            currentOptions;


    private void Awake()
    {
        ResolveReferences();
        RefreshDebugInfo();
    }


    private void Reset()
    {
        ResolveReferences();
    }


    private void OnValidate()
    {
        if (debugTestUpgradeTypes == null)
        {
            debugTestUpgradeTypes =
                new List<RunUpgradeType>();
        }
    }


    public int GetStackCount(
        RunUpgradeType upgradeType
    )
    {
        if (stackCounts.TryGetValue(
                upgradeType,
                out int stackCount
            ))
        {
            return stackCount;
        }

        return 0;
    }


    public bool TryBeginInitialHeroSelection()
    {
        if (!IsSelectionUnlocked)
        {
            return false;
        }

        return TryBeginSelectionInternal(
            RunUpgradeCategory.Hero
        );
    }


    public bool TryBeginSelection(
        RunUpgradeCategory category
    )
    {
        return TryBeginSelectionInternal(
            category
        );
    }


    private bool TryBeginSelectionInternal(
        RunUpgradeCategory category
    )
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "RunUpgradeManager: Selection works only " +
                "in Play Mode.",
                this
            );

            return false;
        }

        if (selectionInProgress)
        {
            Debug.LogWarning(
                "RunUpgradeManager: A selection is already " +
                "in progress.",
                this
            );

            return false;
        }

        if (!ValidateReferences())
        {
            return false;
        }

        BuildCurrentOptions(
            category
        );

        if (currentOptions.Count == 0)
        {
            Debug.Log(
                "RunUpgradeManager: No run upgrades are " +
                "available for this category. The selection " +
                "will be skipped.",
                this
            );

            return false;
        }

        selectionInProgress =
            true;

        debugCurrentCategory =
            category;

        RefreshDebugInfo();

        Debug.Log(
            "RunUpgradeManager: Created " +
            currentOptions.Count +
            " upgrade option(s).",
            this
        );

        return true;
    }


    public bool TrySelectOption(
        int optionIndex,
        out RunUpgradeDefinition selectedUpgrade
    )
    {
        selectedUpgrade =
            null;

        if (!selectionInProgress)
        {
            return false;
        }

        if (optionIndex < 0 ||
            optionIndex >= currentOptions.Count)
        {
            Debug.LogError(
                "RunUpgradeManager: Selected option index " +
                $"{optionIndex} is outside the current list.",
                this
            );

            return false;
        }

        RunUpgradeDefinition definition =
            currentOptions[optionIndex];

        int currentStackCount =
            GetStackCount(
                definition.UpgradeType
            );

        int maximumStacks =
            GetMaximumStacks(
                definition
            );

        if (maximumStacks <= 0)
        {
            Debug.LogError(
                "RunUpgradeManager: Maximum stack count " +
                $"for {definition.UpgradeType} could not be resolved.",
                this
            );

            return false;
        }

        if (currentStackCount >=
            maximumStacks)
        {
            Debug.LogError(
                "RunUpgradeManager: The selected upgrade " +
                "already reached its stack limit.",
                this
            );

            return false;
        }

        if (!TryApplyUpgrade(
                definition
            ))
        {
            return false;
        }

        int newStackCount =
            currentStackCount + 1;

        stackCounts[
            definition.UpgradeType
        ] =
            newStackCount;

        selectedUpgrade =
            definition;

        selectionInProgress =
            false;

        currentOptions.Clear();

        RefreshDebugInfo();

        Debug.Log(
            "RunUpgradeManager: Selected " +
            $"'{definition.DisplayName}'. Stack " +
            $"{newStackCount}/" +
            $"{maximumStacks}.",
            this
        );

        return true;
    }


    public void CancelSelection()
    {
        selectionInProgress =
            false;

        currentOptions.Clear();

        RefreshDebugInfo();
    }


    private int GetMaximumStacks(
        RunUpgradeDefinition definition
    )
    {
        if (definition == null ||
            balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetRunUpgradeMaximumStacks(
                definition
            );
    }


    private bool TryApplyUpgrade(
        RunUpgradeDefinition definition
    )
    {
        switch (definition.UpgradeType)
        {
            case RunUpgradeType.HeroDamage:
            case RunUpgradeType.HeroDamageStrong:

                return combatStats
                    .TryAddHeroRunDamageBonusPercent(
                        definition.BonusPercent
                    );


            case RunUpgradeType.HeroAttackSpeed:
            case RunUpgradeType.HeroAttackSpeedStrong:

                return combatStats
                    .TryAddHeroRunAttackSpeedBonusPercent(
                        definition.BonusPercent
                    );


            case RunUpgradeType.HeroCrit:
            case RunUpgradeType.HeroCritStrong:

                return combatStats
                    .TryAddHeroRunCritChanceBonusPercent(
                        definition.BonusPercent
                    );


            case RunUpgradeType.HeroHealth:
            case RunUpgradeType.HeroHealthStrong:

                return TryApplyHeroHealthUpgrade(
                    definition
                );


            case RunUpgradeType.HeroDoubleShot:

                ResolveReferences();

                if (playerAttack == null ||
                    balanceConfig?.HeroDoubleShot == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: Double Shot balance " +
                        "or PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                DoubleShotBalanceValues
                    doubleShotBalance =
                        balanceConfig.HeroDoubleShot;

                return playerAttack
                    .TryEnableDoubleShot(
                        doubleShotBalance
                            .ProjectileDamageMultiplier,
                        doubleShotBalance
                            .AttackIntervalMultiplier,
                        doubleShotBalance
                            .WindupPenalty,
                        doubleShotBalance
                            .ProjectileOffset
                    );


            case RunUpgradeType.HeroRicochet:

                ResolveReferences();

                if (playerAttack == null ||
                    balanceConfig?.HeroRicochet == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: Ricochet balance " +
                        "or PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                int nextRicochetStack =
                    GetStackCount(
                        RunUpgradeType.HeroRicochet
                    ) + 1;

                RicochetTierBalanceValues
                    ricochetTier =
                        balanceConfig
                            .HeroRicochet
                            .GetTier(
                                nextRicochetStack
                            );

                if (ricochetTier == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: Ricochet tier " +
                        $"{nextRicochetStack} was not found.",
                        this
                    );

                    return false;
                }

                return playerAttack
                    .TryUpgradeRicochet(
                        nextRicochetStack,
                        GetMaximumStacks(definition),
                        ricochetTier.ExtraTargets,
                        ricochetTier.DamageMultiplier,
                        balanceConfig
                            .HeroRicochet
                            .SearchRadius
                    );


            case RunUpgradeType.HeroPoison:

                ResolveReferences();

                if (playerAttack == null ||
                    balanceConfig?.HeroPoison == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: Poison balance " +
                        "or PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                PoisonBalanceValues
                    poisonBalance =
                        balanceConfig.HeroPoison;

                return playerAttack
                    .TryEnablePoison(
                        poisonBalance
                            .PrimaryApplyChance,
                        poisonBalance
                            .RicochetApplyChance,
                        poisonBalance
                            .DamageMultiplier,
                        poisonBalance
                            .Duration,
                        poisonBalance
                            .TickInterval
                    );


            case RunUpgradeType.HeroStun:

                ResolveReferences();

                if (playerAttack == null ||
                    balanceConfig?.HeroStun == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: Stun balance " +
                        "or PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                int nextStunStack =
                    GetStackCount(
                        RunUpgradeType.HeroStun
                    ) + 1;

                StunTierBalanceValues
                    stunTier =
                        balanceConfig
                            .HeroStun
                            .GetTier(
                                nextStunStack
                            );

                if (stunTier == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: Stun tier " +
                        $"{nextStunStack} was not found.",
                        this
                    );

                    return false;
                }

                return playerAttack
                    .TryUpgradeStun(
                        nextStunStack,
                        GetMaximumStacks(definition),
                        balanceConfig
                            .HeroStun
                            .PrimaryApplyChance,
                        balanceConfig
                            .HeroStun
                            .RicochetApplyChance,
                        stunTier.Duration
                    );


            case RunUpgradeType.HeroSlow:

                ResolveReferences();

                if (playerAttack == null ||
                    balanceConfig?.HeroSlow == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: Slow balance " +
                        "or PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                int nextSlowStack =
                    GetStackCount(
                        RunUpgradeType.HeroSlow
                    ) + 1;

                SlowTierBalanceValues
                    slowTier =
                        balanceConfig
                            .HeroSlow
                            .GetTier(
                                nextSlowStack
                            );

                if (slowTier == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: Slow tier " +
                        $"{nextSlowStack} was not found.",
                        this
                    );

                    return false;
                }

                return playerAttack
                    .TryUpgradeSlow(
                        nextSlowStack,
                        GetMaximumStacks(definition),
                        slowTier
                            .MovementReductionPercent,
                        balanceConfig
                            .HeroSlow
                            .Duration
                    );


            case RunUpgradeType.TowerDamage:
            case RunUpgradeType.TowerDamageStrong:

                return combatStats
                    .TryAddTowerRunDamageBonusPercent(
                        definition.BonusPercent
                    );


            case RunUpgradeType.TowerAttackSpeed:
            case RunUpgradeType.TowerAttackSpeedStrong:

                return combatStats
                    .TryAddTowerRunAttackSpeedBonusPercent(
                        definition.BonusPercent
                    );


            case RunUpgradeType.TowerHealth:
            case RunUpgradeType.TowerHealthStrong:

                return TryApplyTowerHealthUpgrade(
                    definition
                );


            default:

                Debug.LogError(
                    "RunUpgradeManager: Unsupported upgrade " +
                    $"type {definition.UpgradeType}.",
                    this
                );

                return false;
        }
    }


    private bool TryApplyHeroHealthUpgrade(
        RunUpgradeDefinition definition
    )
    {
        ResolveReferences();

        if (playerHealth == null)
        {
            Debug.LogError(
                "RunUpgradeManager: PlayerHealth was not found.",
                this
            );

            return false;
        }

        int permanentMaxHealth =
            combatStats.HeroMaxHealth -
            combatStats.HeroRunMaxHealthBonus;

        if (permanentMaxHealth <= 0)
        {
            Debug.LogError(
                "RunUpgradeManager: Hero permanent max health " +
                "must be greater than zero.",
                this
            );

            return false;
        }

        int healthToAdd =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    permanentMaxHealth *
                    definition.BonusPercent /
                    100f
                )
            );

        return playerHealth
            .TryAddRunMaxHealth(
                healthToAdd
            );
    }


    private bool TryApplyTowerHealthUpgrade(
        RunUpgradeDefinition definition
    )
    {
        ResolveReferences();

        if (combatStats == null)
        {
            Debug.LogError(
                "RunUpgradeManager: CombatStats was not found.",
                this
            );

            return false;
        }

        int permanentMaxHealth =
            combatStats.DefenseTowerMaxHealth -
            combatStats.DefenseTowerRunMaxHealthBonus;

        if (permanentMaxHealth <= 0)
        {
            Debug.LogError(
                "RunUpgradeManager: Defense tower permanent max health " +
                "must be greater than zero.",
                this
            );

            return false;
        }

        int healthToAdd =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    permanentMaxHealth *
                    definition.BonusPercent /
                    100f
                )
            );

        return combatStats
            .TryAddDefenseTowerRunMaxHealthBonus(
                healthToAdd
            );
    }


    private void BuildCurrentOptions(
        RunUpgradeCategory category
    )
    {
        currentOptions.Clear();

        if (balanceConfig == null)
        {
            Debug.LogError(
                "RunUpgradeManager: GameBalanceConfig is not assigned.",
                this
            );

            return;
        }

        IReadOnlyList<RunUpgradeDefinition>
            upgradeDefinitions =
                balanceConfig.RunUpgradeDefinitions;

        if (upgradeDefinitions == null)
        {
            Debug.LogError(
                "RunUpgradeManager: Run upgrade definitions " +
                "were not found in GameBalanceConfig.",
                this
            );

            return;
        }

        if (useDebugTestPool &&
            category == debugTestCategory &&
            debugTestUpgradeTypes.Count == 0)
        {
            Debug.LogWarning(
                "RunUpgradeManager: Debug Test Pool is enabled, " +
                "but no upgrade types were selected.",
                this
            );
        }

        List<RunUpgradeDefinition>
            availableOptions =
                new List<RunUpgradeDefinition>();

        HashSet<RunUpgradeType>
            addedTypes =
                new HashSet<RunUpgradeType>();


        foreach (
            RunUpgradeDefinition definition
            in upgradeDefinitions
        )
        {
            if (definition == null ||
                definition.Category != category ||
                !addedTypes.Add(
                    definition.UpgradeType
                ))
            {
                continue;
            }

            if (useDebugTestPool &&
                category == debugTestCategory &&
                !debugTestUpgradeTypes.Contains(
                    definition.UpgradeType
                ))
            {
                continue;
            }

            int stackCount =
                GetStackCount(
                    definition.UpgradeType
                );

            int maximumStacks =
                GetMaximumStacks(
                    definition
                );

            if (maximumStacks <= 0 ||
                stackCount >=
                maximumStacks)
            {
                continue;
            }

            availableOptions.Add(
                definition
            );
        }


        for (
            int i =
                availableOptions.Count - 1;
            i > 0;
            i--
        )
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    i + 1
                );

            (
                availableOptions[i],
                availableOptions[randomIndex]
            ) =
            (
                availableOptions[randomIndex],
                availableOptions[i]
            );
        }


        int optionsToOffer =
            Mathf.Min(
                balanceConfig.RunUpgradeOfferedOptionCount,
                availableOptions.Count
            );


        for (
            int i = 0;
            i < optionsToOffer;
            i++
        )
        {
            currentOptions.Add(
                availableOptions[i]
            );
        }


        debugAvailableUpgradeCount =
            availableOptions.Count;
    }


    private void ResolveReferences()
    {
        if (combatStats == null)
        {
            combatStats =
                GetComponent<CombatStats>();
        }

        if (playerProgression == null)
        {
            playerProgression =
                GetComponent<PlayerProgression>();
        }

        if (playerHealth == null)
        {
            playerHealth =
                FindAnyObjectByType<PlayerHealth>(
                    FindObjectsInactive.Include
                );
        }

        if (playerAttack == null)
        {
            playerAttack =
                FindAnyObjectByType<PlayerAttack>(
                    FindObjectsInactive.Include
                );
        }
    }


    private bool ValidateReferences()
    {
        ResolveReferences();

        bool referencesAreValid =
            true;

        if (combatStats == null)
        {
            Debug.LogError(
                "RunUpgradeManager: Assign CombatStats.",
                this
            );

            referencesAreValid =
                false;
        }

        if (playerProgression == null)
        {
            Debug.LogError(
                "RunUpgradeManager: Assign PlayerProgression.",
                this
            );

            referencesAreValid =
                false;
        }

        if (balanceConfig == null)
        {
            Debug.LogError(
                "RunUpgradeManager: Assign GameBalanceConfig.",
                this
            );

            referencesAreValid =
                false;
        }

        return referencesAreValid;
    }


    private void RefreshDebugInfo()
    {
        debugHeroDamageStacks =
            GetStackCount(
                RunUpgradeType.HeroDamage
            );

        debugHeroAttackSpeedStacks =
            GetStackCount(
                RunUpgradeType.HeroAttackSpeed
            );

        debugTowerDamageStacks =
            GetStackCount(
                RunUpgradeType.TowerDamage
            );

        debugTowerAttackSpeedStacks =
            GetStackCount(
                RunUpgradeType.TowerAttackSpeed
            );

        debugHeroDamageStrongStacks =
            GetStackCount(
                RunUpgradeType.HeroDamageStrong
            );

        debugHeroAttackSpeedStrongStacks =
            GetStackCount(
                RunUpgradeType.HeroAttackSpeedStrong
            );

        debugTowerDamageStrongStacks =
            GetStackCount(
                RunUpgradeType.TowerDamageStrong
            );

        debugTowerAttackSpeedStrongStacks =
            GetStackCount(
                RunUpgradeType.TowerAttackSpeedStrong
            );

        debugHeroCritStacks =
            GetStackCount(
                RunUpgradeType.HeroCrit
            );

        debugHeroCritStrongStacks =
            GetStackCount(
                RunUpgradeType.HeroCritStrong
            );

        debugHeroHealthStacks =
            GetStackCount(
                RunUpgradeType.HeroHealth
            );

        debugHeroHealthStrongStacks =
            GetStackCount(
                RunUpgradeType.HeroHealthStrong
            );

        debugTowerHealthStacks =
            GetStackCount(
                RunUpgradeType.TowerHealth
            );

        debugTowerHealthStrongStacks =
            GetStackCount(
                RunUpgradeType.TowerHealthStrong
            );

        debugHeroDoubleShotStacks =
            GetStackCount(
                RunUpgradeType.HeroDoubleShot
            );

        debugHeroRicochetStacks =
            GetStackCount(
                RunUpgradeType.HeroRicochet
            );

        debugHeroPoisonStacks =
            GetStackCount(
                RunUpgradeType.HeroPoison
            );

        debugHeroStunStacks =
            GetStackCount(
                RunUpgradeType.HeroStun
            );

        debugHeroSlowStacks =
            GetStackCount(
                RunUpgradeType.HeroSlow
            );

        if (!selectionInProgress)
        {
            debugAvailableUpgradeCount =
                0;
        }
    }
}
