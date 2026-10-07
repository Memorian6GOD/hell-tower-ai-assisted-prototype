using System;
using System.Collections.Generic;
using UnityEngine;

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

    // Crit upgrades were added after all existing values
    // so old serialized data keeps its meaning.
    HeroCrit,
    HeroCritStrong,

    // Health upgrades were added after all existing values
    // so old serialized data keeps its meaning.
    HeroHealth,
    HeroHealthStrong,

    // Unique hero mechanics were added after all existing values
    // so old serialized data keeps its meaning.
    HeroDoubleShot,

    // Ricochet was appended after Double Shot
    // so previously serialized enum values stay unchanged.
    HeroRicochet,

    // Poison was appended after Ricochet
    // so previously serialized enum values stay unchanged.
    HeroPoison,

    // Stun was appended after Poison
    // so previously serialized enum values stay unchanged.
    HeroStun,

    // Slow was appended after Stun
    // so previously serialized enum values stay unchanged.
    HeroSlow
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

    [SerializeField, TextArea(2, 4)]
    private string description = "Upgrade description.";

    [SerializeField, Min(0.01f)]
    private float bonusPercent = 10f;

    [SerializeField, Range(1, 3)]
    private int maximumStacks = 3;

    public RunUpgradeType UpgradeType => upgradeType;

    public string DisplayName => displayName;

    public string Description => description;

    public float BonusPercent =>
        Mathf.Max(0.01f, bonusPercent);

    public int MaximumStacks =>
        Mathf.Clamp(maximumStacks, 1, 3);

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
        this.upgradeType = upgradeType;
        this.displayName = displayName;
        this.description = description;
        this.bonusPercent = bonusPercent;

        this.maximumStacks = Mathf.Clamp(
            maximumStacks,
            1,
            3
        );
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = upgradeType.ToString();
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            description = "Upgrade description.";
        }

        bonusPercent = Mathf.Max(
            0.01f,
            bonusPercent
        );

        maximumStacks = Mathf.Clamp(
            maximumStacks,
            1,
            3
        );
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

    [Header("Selection Settings")]
    [SerializeField, Min(1)]
    private int unlockHeroLevel = 2;

    [SerializeField, Range(2, 3)]
    private int offeredOptionCount = 2;

    [Header("Available Run Upgrades")]
    [SerializeField]
    private List<RunUpgradeDefinition>
        upgradeDefinitions = new();

    [Header("Debug Card Testing")]
    [Tooltip("When enabled, only the selected upgrade types are allowed to appear for the chosen category.")]
    [SerializeField]
    private bool useDebugTestPool;

    [SerializeField]
    private RunUpgradeCategory debugTestCategory = RunUpgradeCategory.Hero;

    [SerializeField]
    private List<RunUpgradeType> debugTestUpgradeTypes = new();

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

    private readonly Dictionary<RunUpgradeType, int>
        stackCounts = new();

    private readonly List<RunUpgradeDefinition>
        currentOptions = new();

    public bool IsSelectionUnlocked =>
        playerProgression != null &&
        playerProgression.HeroLevel >= unlockHeroLevel;

    public bool IsSelectionInProgress =>
        selectionInProgress;

    public IReadOnlyList<RunUpgradeDefinition>
        CurrentOptions => currentOptions;

    private void Awake()
    {
        ResolveReferences();
        EnsureDefaultDefinitions();
        RefreshDebugInfo();
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
        return TryBeginSelectionInternal(category);
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

        BuildCurrentOptions(category);

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

        selectionInProgress = true;
        debugCurrentCategory = category;

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
        selectedUpgrade = null;

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
            GetStackCount(definition.UpgradeType);

        if (currentStackCount >=
            definition.MaximumStacks)
        {
            Debug.LogError(
                "RunUpgradeManager: The selected upgrade " +
                "already reached its stack limit.",
                this
            );

            return false;
        }

        if (!TryApplyUpgrade(definition))
        {
            return false;
        }

        int newStackCount =
            currentStackCount + 1;

        stackCounts[definition.UpgradeType] =
            newStackCount;

        selectedUpgrade = definition;

        selectionInProgress = false;

        currentOptions.Clear();

        RefreshDebugInfo();

        Debug.Log(
            "RunUpgradeManager: Selected " +
            $"'{definition.DisplayName}'. Stack " +
            $"{newStackCount}/" +
            $"{definition.MaximumStacks}.",
            this
        );

        return true;
    }

    public void CancelSelection()
    {
        selectionInProgress = false;

        currentOptions.Clear();

        RefreshDebugInfo();
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

                if (playerAttack == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                return playerAttack
                    .TryEnableDoubleShot();

            case RunUpgradeType.HeroRicochet:
                ResolveReferences();

                if (playerAttack == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                return playerAttack
                    .TryUpgradeRicochet();

            case RunUpgradeType.HeroPoison:
                ResolveReferences();

                if (playerAttack == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                return playerAttack
                    .TryEnablePoison();

            case RunUpgradeType.HeroStun:
                ResolveReferences();

                if (playerAttack == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                return playerAttack
                    .TryUpgradeStun();

            case RunUpgradeType.HeroSlow:
                ResolveReferences();

                if (playerAttack == null)
                {
                    Debug.LogError(
                        "RunUpgradeManager: PlayerAttack was not found.",
                        this
                    );

                    return false;
                }

                return playerAttack
                    .TryUpgradeSlow();

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

        // The health percentage is calculated from the hero's
        // permanent maximum HP before run-health cards.
        // This prevents HP cards from multiplying one another.
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

        return playerHealth.TryAddRunMaxHealth(
            healthToAdd
        );
    }

    private void BuildCurrentOptions(
        RunUpgradeCategory category
    )
    {
        currentOptions.Clear();

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
            availableOptions = new();

        HashSet<RunUpgradeType>
            addedTypes = new();

        foreach (RunUpgradeDefinition definition
                 in upgradeDefinitions)
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

            if (stackCount >=
                definition.MaximumStacks)
            {
                continue;
            }

            availableOptions.Add(definition);
        }

        for (int i =
                 availableOptions.Count - 1;
             i > 0;
             i--)
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
                offeredOptionCount,
                availableOptions.Count
            );

        for (int i = 0;
             i < optionsToOffer;
             i++)
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

        bool referencesAreValid = true;

        if (combatStats == null)
        {
            Debug.LogError(
                "RunUpgradeManager: Assign CombatStats.",
                this
            );

            referencesAreValid = false;
        }

        if (playerProgression == null)
        {
            Debug.LogError(
                "RunUpgradeManager: Assign " +
                "PlayerProgression.",
                this
            );

            referencesAreValid = false;
        }

        return referencesAreValid;
    }

    private void EnsureDefaultDefinitions()
    {
        if (upgradeDefinitions == null)
        {
            upgradeDefinitions =
                new List<RunUpgradeDefinition>();
        }

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroDamage,
                "DAMAGE",
                "+10% hero damage",
                10f,
                3
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroAttackSpeed,
                "ATTACK SPEED",
                "+10% hero attack speed",
                10f,
                3
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.TowerDamage,
                "TOWER DAMAGE",
                "Increases defense tower damage by 10%.",
                10f,
                3
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.TowerAttackSpeed,
                "TOWER ATTACK SPEED",
                "Increases defense tower attack speed by 10%.",
                10f,
                3
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroDamageStrong,
                "POWERFUL DAMAGE",
                "+20% hero damage",
                20f,
                2
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroAttackSpeedStrong,
                "RAPID ATTACK",
                "+20% hero attack speed",
                20f,
                2
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.TowerDamageStrong,
                "POWERFUL TOWER",
                "Increases defense tower damage by 20%.",
                20f,
                2
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.TowerAttackSpeedStrong,
                "RAPID TOWER",
                "Increases defense tower attack speed by 20%.",
                20f,
                2
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroCrit,
                "CRITICAL CHANCE",
                "Increases hero critical chance.",
                20f,
                3
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroCritStrong,
                "DEADLY PRECISION",
                "Greatly increases hero critical chance.",
                40f,
                2
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroHealth,
                "VITALITY",
                "Increases hero maximum health.",
                7f,
                3
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroHealthStrong,
                "GREATER VITALITY",
                "Greatly increases hero maximum health.",
                15f,
                2
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroDoubleShot,
                "DOUBLE SHOT",
                "Fires two projectiles at the same target. " +
                "Each projectile deals reduced damage and " +
                "the attack takes longer to prepare.",
                85f,
                1
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroRicochet,
                "RICOCHET",
                "Projectiles bounce to additional enemies. " +
                "Further stacks improve the number of targets " +
                "and ricochet damage.",
                33f,
                3
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroPoison,
                "POISON",
                "Projectile hits can poison enemies and deal " +
                "damage over time. Ricochet hits have a lower " +
                "chance to apply poison.",
                50f,
                1
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroStun,
                "STUN",
                "Projectile hits can briefly stop enemies. " +
                "A second stack increases the stun duration.",
                10f,
                2
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroSlow,
                "SLOW",
                "Projectile hits slow enemy movement. " +
                "A second stack increases the movement reduction.",
                15f,
                2
            )
        );
    }

    private void EnsureDefinition(
        RunUpgradeDefinition definitionToAdd
    )
    {
        foreach (RunUpgradeDefinition definition
                 in upgradeDefinitions)
        {
            if (definition != null &&
                definition.UpgradeType ==
                definitionToAdd.UpgradeType)
            {
                return;
            }
        }

        upgradeDefinitions.Add(
            definitionToAdd
        );
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
            debugAvailableUpgradeCount = 0;
        }
    }

    private void Reset()
    {
        ResolveReferences();
        EnsureDefaultDefinitions();
    }

    private void OnValidate()
    {
        unlockHeroLevel =
            Mathf.Max(
                1,
                unlockHeroLevel
            );

        offeredOptionCount =
            Mathf.Clamp(
                offeredOptionCount,
                2,
                3
            );

        EnsureDefaultDefinitions();

        if (debugTestUpgradeTypes == null)
        {
            debugTestUpgradeTypes =
                new List<RunUpgradeType>();
        }

        foreach (RunUpgradeDefinition definition
                 in upgradeDefinitions)
        {
            definition?.Validate();
        }
    }
}