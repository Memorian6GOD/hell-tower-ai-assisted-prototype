using System;
using System.Collections.Generic;
using UnityEngine;

public enum RunUpgradeType
{
    HeroDamage,
    HeroAttackSpeed
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
    [SerializeField] private int debugHeroDamageStacks;
    [SerializeField] private int debugHeroAttackSpeedStacks;
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

    public bool TryBeginSelection()
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

        if (!IsSelectionUnlocked)
        {
            return false;
        }

        BuildCurrentOptions();

        if (currentOptions.Count == 0)
        {
            Debug.LogWarning(
                "RunUpgradeManager: No run upgrades are " +
                "available. Every upgrade may have reached " +
                "its stack limit.",
                this
            );

            return false;
        }

        selectionInProgress = true;
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

            default:
                Debug.LogError(
                    "RunUpgradeManager: Unsupported upgrade " +
                    $"type {definition.UpgradeType}.",
                    this
                );

                return false;
        }
    }

    private void BuildCurrentOptions()
    {
        currentOptions.Clear();

        List<RunUpgradeDefinition> availableOptions =
            new();

        HashSet<RunUpgradeType> addedTypes = new();

        foreach (RunUpgradeDefinition definition
                 in upgradeDefinitions)
        {
            if (definition == null ||
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

        if (upgradeDefinitions.Count > 0)
        {
            return;
        }

        upgradeDefinitions.Add(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroDamage,
                "DAMAGE",
                "+10% hero damage",
                10f
            )
        );

        upgradeDefinitions.Add(
            new RunUpgradeDefinition(
                RunUpgradeType.HeroAttackSpeed,
                "ATTACK SPEED",
                "+10% hero attack speed",
                10f
            )
        );
    }

    private void RefreshDebugInfo()
    {
        debugHeroDamageStacks =
            GetStackCount(RunUpgradeType.HeroDamage);

        debugHeroAttackSpeedStacks =
            GetStackCount(RunUpgradeType.HeroAttackSpeed);

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
