using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public sealed class RunUpgradeCardUI
{
    [SerializeField] private GameObject cardRoot;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text stackText;

    [NonSerialized] private UnityAction clickListener;

    public bool IsConfigured =>
        cardRoot != null &&
        selectButton != null &&
        titleText != null &&
        descriptionText != null;

    public void Show(
        RunUpgradeDefinition definition,
        int currentStacks,
        UnityAction onSelected
    )
    {
        RemoveListener();

        titleText.text = definition.DisplayName;
        descriptionText.text = definition.Description;
        if (stackText != null)
        {
            stackText.text =
                $"{currentStacks + 1} / {definition.MaximumStacks}";
        }

        clickListener = onSelected;
        selectButton.onClick.AddListener(clickListener);
        selectButton.interactable = true;
        cardRoot.SetActive(true);
    }

    public void Hide()
    {
        RemoveListener();

        if (cardRoot != null)
        {
            cardRoot.SetActive(false);
        }
    }

    private void RemoveListener()
    {
        if (selectButton != null &&
            clickListener != null)
        {
            selectButton.onClick.RemoveListener(
                clickListener
            );
        }

        clickListener = null;
    }
}

[DisallowMultipleComponent]
public class RunUpgradeSelectionUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField]
    private RunUpgradeManager runUpgradeManager;

    [Header("Selection Window")]
    [SerializeField] private GameObject selectionRoot;
    [SerializeField] private TMP_Text titleText;

    [SerializeField]
    private List<RunUpgradeCardUI> optionCards = new();

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool isOpen;
    [SerializeField] private int visibleOptionCount;

    private Action selectionCompleted;

    public bool IsOpen => isOpen;

    public bool IsSelectionUnlocked =>
        runUpgradeManager != null &&
        runUpgradeManager.IsSelectionUnlocked;

    private void Awake()
    {
        CloseVisuals();
    }

    public bool TryOpenInitialHeroSelection(
        Action onSelectionCompleted
    )
    {
        if (!IsSelectionUnlocked)
        {
            return false;
        }

        return TryOpenSelectionInternal(
            RunUpgradeCategory.Hero,
            onSelectionCompleted,
            true
        );
    }

    public bool TryOpenSelection(
        RunUpgradeCategory category,
        Action onSelectionCompleted
    )
    {
        return TryOpenSelectionInternal(
            category,
            onSelectionCompleted,
            false
        );
    }

    private bool TryOpenSelectionInternal(
        RunUpgradeCategory category,
        Action onSelectionCompleted,
        bool isInitialSelection
    )
    {
        if (isOpen)
        {
            return false;
        }

        if (!ValidateReferences())
        {
            return false;
        }

        bool selectionStarted = isInitialSelection
            ? runUpgradeManager.TryBeginInitialHeroSelection()
            : runUpgradeManager.TryBeginSelection(category);

        if (!selectionStarted)
        {
            return false;
        }

        IReadOnlyList<RunUpgradeDefinition> options =
            runUpgradeManager.CurrentOptions;

        if (options.Count > optionCards.Count)
        {
            Debug.LogError(
                "RunUpgradeSelectionUI: Not enough UI cards. " +
                $"Needed {options.Count}, assigned " +
                $"{optionCards.Count}.",
                this
            );

            runUpgradeManager.CancelSelection();
            return false;
        }

        for (int i = 0; i < optionCards.Count; i++)
        {
            if (i >= options.Count)
            {
                optionCards[i].Hide();
                continue;
            }

            int capturedOptionIndex = i;
            RunUpgradeDefinition definition = options[i];
            int currentStacks = runUpgradeManager.GetStackCount(
                definition.UpgradeType
            );

            optionCards[i].Show(
                definition,
                currentStacks,
                () => HandleOptionSelected(
                    capturedOptionIndex
                )
            );
        }

        selectionCompleted = onSelectionCompleted;
        visibleOptionCount = options.Count;
        isOpen = true;

        if (titleText != null)
        {
            titleText.text = category ==
                RunUpgradeCategory.DefenseTower
                ? "CHOOSE A TOWER UPGRADE"
                : "CHOOSE A HERO UPGRADE";
        }

        selectionRoot.SetActive(true);

        Debug.Log(
            "RunUpgradeSelectionUI: Selection opened.",
            this
        );

        return true;
    }

    private void HandleOptionSelected(int optionIndex)
    {
        if (!isOpen)
        {
            return;
        }

        if (!runUpgradeManager.TrySelectOption(
                optionIndex,
                out RunUpgradeDefinition selectedUpgrade
            ))
        {
            return;
        }

        Action completedCallback = selectionCompleted;

        CloseVisuals();

        Debug.Log(
            "RunUpgradeSelectionUI: Selected " +
            $"'{selectedUpgrade.DisplayName}'.",
            this
        );

        completedCallback?.Invoke();
    }

    private bool ValidateReferences()
    {
        bool referencesAreValid = true;

        if (runUpgradeManager == null)
        {
            Debug.LogError(
                "RunUpgradeSelectionUI: Assign " +
                "RunUpgradeManager.",
                this
            );

            referencesAreValid = false;
        }

        if (selectionRoot == null)
        {
            Debug.LogError(
                "RunUpgradeSelectionUI: Assign Selection Root.",
                this
            );

            referencesAreValid = false;
        }

        if (optionCards == null || optionCards.Count < 2)
        {
            Debug.LogError(
                "RunUpgradeSelectionUI: Assign at least " +
                "two option cards.",
                this
            );

            referencesAreValid = false;
        }
        else
        {
            for (int i = 0; i < optionCards.Count; i++)
            {
                if (optionCards[i] != null &&
                    optionCards[i].IsConfigured)
                {
                    continue;
                }

                Debug.LogError(
                    "RunUpgradeSelectionUI: Option card " +
                    $"{i + 1} is not fully configured.",
                    this
                );

                referencesAreValid = false;
            }
        }

        return referencesAreValid;
    }

    private void CloseVisuals()
    {
        selectionCompleted = null;
        visibleOptionCount = 0;
        isOpen = false;

        if (optionCards != null)
        {
            foreach (RunUpgradeCardUI optionCard
                     in optionCards)
            {
                optionCard?.Hide();
            }
        }

        if (selectionRoot != null &&
            selectionRoot.activeSelf)
        {
            selectionRoot.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (isOpen && runUpgradeManager != null)
        {
            runUpgradeManager.CancelSelection();
        }

        CloseVisuals();
    }
}
