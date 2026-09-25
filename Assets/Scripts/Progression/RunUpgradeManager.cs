using System;
using System.Collections.Generic;
using UnityEngine;

public enum RunUpgradeType
{
    HeroDamage,
    HeroAttackSpeed,
    TowerDamage,
    TowerAttackSpeed
}

public enum RunUpgradeCategory
{
    Hero,
    DefenseTower
}

[Serializable]
public sealed class RunUpgradeDefinition
{
    [SerializeField] private RunUpgradeType upgradeType;
    [SerializeField] private string displayName = "UPGRADE";

    [SerializeField, TextArea(2, 4)]
    private string description = "Upgrade description.";

    [SerializeField, Min(0.01f)]
    private float bonusPercent = 10f;

    [SerializeField, Range(1, 3)]
    private int maximumStacks = 3;

    public RunUpgradeType UpgradeType => upgradeType;
    public string DisplayName => displayName;
    public string Description => description;
    public float BonusPercent => Mathf.Max(0.01f, bonusPercent);
    public int MaximumStacks => Mathf.Clamp(maximumStacks, 1, 3);

    public RunUpgradeCategory Category
    {
        get
        {
            switch (upgradeType)
            {
                case RunUpgradeType.TowerDamage:
                case RunUpgradeType.TowerAttackSpeed:
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
        float bonusPercent
    )
    {
        this.upgradeType = upgradeType;
        this.displayName = displayName;
        this.description = description;
        this.bonusPercent = bonusPercent;
        maximumStacks = 3;
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

        bonusPercent = Mathf.Max(0.01f, bonusPercent);
        maximumStacks = Mathf.Clamp(maximumStacks, 1, 3);
    }
}

[DisallowMultipleComponent]
public class RunUpgradeManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private CombatStats combatStats;
    [SerializeField] private PlayerProgression playerProgression;

    [Header("Selection Settings")]
    [SerializeField, Min(1)] private int unlockHeroLevel = 2;
    [SerializeField, Range(2, 3)] private int offeredOptionCount = 2;

    [Header("Available Run Upgrades")]
    [SerializeField]
    private List<RunUpgradeDefinition> upgradeDefinitions = new();

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool selectionInProgress;
    [SerializeField] private RunUpgradeCategory debugCurrentCategory;
    [SerializeField] private int debugHeroDamageStacks;
    [SerializeField] private int debugHeroAttackSpeedStacks;
    [SerializeField] private int debugTowerDamageStacks;
    [SerializeField] private int debugTowerAttackSpeedStacks;
    [SerializeField] private int debugAvailableUpgradeCount;

    private readonly Dictionary<RunUpgradeType, int>
        stackCounts = new();

    private readonly List<RunUpgradeDefinition>
        currentOptions = new();

    public bool IsSelectionUnlocked =>
        playerProgression != null &&
        playerProgression.HeroLevel >= unlockHeroLevel;

    public bool IsSelectionInProgress =>
        selectionInProgress;

    public IReadOnlyList<RunUpgradeDefinition> CurrentOptions =>
        currentOptions;

    private void Awake()
    {
        ResolveReferences();
        EnsureDefaultDefinitions();
        RefreshDebugInfo();
    }

    public int GetStackCount(RunUpgradeType upgradeType)
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

        if (currentStackCount >= definition.MaximumStacks)
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

        int newStackCount = currentStackCount + 1;

        stackCounts[definition.UpgradeType] =
            newStackCount;

        selectedUpgrade = definition;
        selectionInProgress = false;
        currentOptions.Clear();
        RefreshDebugInfo();

        Debug.Log(
            "RunUpgradeManager: Selected " +
            $"'{definition.DisplayName}'. Stack " +
            $"{newStackCount}/{definition.MaximumStacks}.",
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
                return combatStats
                    .TryAddHeroRunDamageBonusPercent(
                        definition.BonusPercent
                    );

            case RunUpgradeType.HeroAttackSpeed:
                return combatStats
                    .TryAddHeroRunAttackSpeedBonusPercent(
                        definition.BonusPercent
                    );

            case RunUpgradeType.TowerDamage:
                return combatStats
                    .TryAddTowerRunDamageBonusPercent(
                        definition.BonusPercent
                    );

            case RunUpgradeType.TowerAttackSpeed:
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

    private void BuildCurrentOptions(
        RunUpgradeCategory category
    )
    {
        currentOptions.Clear();

        List<RunUpgradeDefinition> availableOptions =
            new();

        HashSet<RunUpgradeType> addedTypes = new();

        foreach (RunUpgradeDefinition definition
                 in upgradeDefinitions)
        {
            if (definition == null ||
                definition.Category != category ||
                !addedTypes.Add(definition.UpgradeType))
            {
                continue;
            }

            int stackCount =
                GetStackCount(definition.UpgradeType);

            if (stackCount >= definition.MaximumStacks)
            {
                continue;
            }

            availableOptions.Add(definition);
        }

        for (int i = availableOptions.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex = UnityEngine.Random.Range(
                0,
                i + 1
            );

            (availableOptions[i], availableOptions[randomIndex]) =
                (availableOptions[randomIndex], availableOptions[i]);
        }

        int optionsToOffer = Mathf.Min(
            offeredOptionCount,
            availableOptions.Count
        );

        for (int i = 0; i < optionsToOffer; i++)
        {
            currentOptions.Add(availableOptions[i]);
        }

        debugAvailableUpgradeCount =
            availableOptions.Count;
    }

    private void ResolveReferences()
    {
        if (combatStats == null)
        {
            combatStats = GetComponent<CombatStats>();
        }

        if (playerProgression == null)
        {
            playerProgression =
                GetComponent<PlayerProgression>();
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
                "RunUpgradeManager: Assign PlayerProgression.",
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
                10f
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroAttackSpeed,
                "ATTACK SPEED",
                "+10% hero attack speed",
                10f
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.TowerDamage,
                "TOWER DAMAGE",
                "Increases defense tower damage by 10%.",
                10f
            )
        );

        EnsureDefinition(
            new RunUpgradeDefinition(
                RunUpgradeType.TowerAttackSpeed,
                "TOWER ATTACK SPEED",
                "Increases defense tower attack speed by 10%.",
                10f
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

        upgradeDefinitions.Add(definitionToAdd);
    }

    private void RefreshDebugInfo()
    {
        debugHeroDamageStacks =
            GetStackCount(RunUpgradeType.HeroDamage);

        debugHeroAttackSpeedStacks =
            GetStackCount(RunUpgradeType.HeroAttackSpeed);

        debugTowerDamageStacks =
            GetStackCount(RunUpgradeType.TowerDamage);

        debugTowerAttackSpeedStacks =
            GetStackCount(RunUpgradeType.TowerAttackSpeed);

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
        unlockHeroLevel = Mathf.Max(1, unlockHeroLevel);
        offeredOptionCount = Mathf.Clamp(offeredOptionCount, 2, 3);

        EnsureDefaultDefinitions();

        foreach (RunUpgradeDefinition definition
                 in upgradeDefinitions)
        {
            definition?.Validate();
        }
    }
}
