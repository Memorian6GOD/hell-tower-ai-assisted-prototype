using TMPro;
using UnityEngine;

public class PreWavePreparationManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private WaveManager waveManager;

    [SerializeField]
    private DefenseTowerPlacement
        towerPlacement;

    [Header("Player References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject playerVisual;
    [SerializeField] private GameObject playerHealthCanvas;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerAttack;

    [Header("UI References")]
    [SerializeField] private TMP_Text preparationTimerText;
    [SerializeField] private GameObject coreTowerHealthUI;

    [SerializeField]
    private RunUpgradeSelectionUI runUpgradeSelectionUI;

    [Header("Build Grid")]
    [SerializeField] private GameObject defenseTowerBuildGrid;

    [Header("Preparation Settings")]
    [SerializeField] private float preparationDuration = 30f;
    [SerializeField] private float waveStartDelay = 2f;
    [SerializeField] private int availableTowerSlots = 1;

    [SerializeField]
    private bool startPreparationOnStart = true;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool isPreparationActive;
    [SerializeField] private bool isWaveStartDelayActive;
    [SerializeField] private float remainingTime;
    [SerializeField] private float waveStartDelayRemaining;
    [SerializeField] private bool playerIsVisible;
    [SerializeField] private bool isRunUpgradeSelectionActive;
    [SerializeField] private bool initialRunUpgradeResolved;

    private bool placementEventSubscribed;

    public bool IsPreparationActive =>
        isPreparationActive;

    public bool IsWaveStartDelayActive =>
        isWaveStartDelayActive;

    public float RemainingTime =>
        remainingTime;

    public float WaveStartDelayRemaining =>
        waveStartDelayRemaining;

    public bool PlayerIsVisible =>
        playerIsVisible;

    public bool IsRunUpgradeSelectionActive =>
        isRunUpgradeSelectionActive;

    private void OnEnable()
    {
        SubscribeToPlacementEvent();
    }

    private void Start()
    {
        SubscribeToPlacementEvent();

        HideTimer();
        HideBuildGrid();

        if (startPreparationOnStart)
        {
            BeginPreparation();
        }
    }

    private void Update()
    {
        if (isPreparationActive)
        {
            UpdatePreparation();
            return;
        }

        if (isWaveStartDelayActive)
        {
            UpdateWaveStartDelay();
        }
    }

    private void UpdatePreparation()
    {
        HideCoreTowerHealthUI();

        remainingTime -= Time.deltaTime;

        if (remainingTime < 0f)
        {
            remainingTime = 0f;
        }

        UpdateTimerText();

        if (remainingTime > 0f)
        {
            return;
        }

        FinishPreparation(
            "Preparation time expired."
        );
    }

    private void UpdateWaveStartDelay()
    {
        waveStartDelayRemaining -=
            Time.deltaTime;

        if (waveStartDelayRemaining < 0f)
        {
            waveStartDelayRemaining = 0f;
        }

        if (waveStartDelayRemaining > 0f)
        {
            return;
        }

        StartFirstWave();
    }

    [ContextMenu("Begin Preparation")]
    public void BeginPreparation()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Preparation works only in Play Mode."
            );

            return;
        }

        if (isPreparationActive ||
            isWaveStartDelayActive)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Preparation or start delay " +
                "is already active."
            );

            return;
        }

        if (!ValidateReferences())
        {
            return;
        }

        if (waveManager.CurrentWave > 0 ||
            waveManager.IsWaveActive)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Preparation is only available " +
                "before the first wave."
            );

            return;
        }

        SubscribeToPlacementEvent();

        MovePlayerToSpawnPoint();
        HidePlayer();
        HideCoreTowerHealthUI();

        remainingTime =
            Mathf.Max(
                0f,
                preparationDuration
            );

        waveStartDelayRemaining = 0f;

        isPreparationActive = true;
        isWaveStartDelayActive = false;
        isRunUpgradeSelectionActive = false;
        initialRunUpgradeResolved = false;

        ShowTimer();
        ShowBuildGrid();
        UpdateTimerText();

        towerPlacement.OpenPlacement(
            availableTowerSlots
        );

        Debug.Log(
            "PreWavePreparationManager: " +
            "Preparation started for " +
            remainingTime.ToString("F1") +
            " seconds. Available tower slots: " +
            availableTowerSlots +
            "."
        );

        if (remainingTime <= 0f)
        {
            FinishPreparation(
                "Preparation duration is zero."
            );
        }
    }

    private void HandleAllTowerSlotsUsed()
    {
        if (!isPreparationActive)
        {
            return;
        }

        FinishPreparation(
            "All tower slots were used."
        );
    }

    private void FinishPreparation(string reason)
    {
        if (!isPreparationActive)
        {
            return;
        }

        if (!initialRunUpgradeResolved &&
            runUpgradeSelectionUI != null &&
            runUpgradeSelectionUI.IsSelectionUnlocked)
        {
            if (runUpgradeSelectionUI.TryOpenSelection(
                    HandleInitialRunUpgradeSelected
                ))
            {
                PauseForRunUpgradeSelection(reason);
                return;
            }

            Debug.LogWarning(
                "PreWavePreparationManager: Run upgrade " +
                "selection was unlocked but could not open. " +
                "The first wave will start without it.",
                this
            );
        }

        initialRunUpgradeResolved = true;
        BeginWaveStartDelay(reason);
    }

    private void PauseForRunUpgradeSelection(
        string reason
    )
    {
        isPreparationActive = false;
        remainingTime = 0f;

        towerPlacement.ClosePlacement();

        HideTimer();
        HideBuildGrid();
        HideCoreTowerHealthUI();

        isRunUpgradeSelectionActive = true;

        Debug.Log(
            "PreWavePreparationManager: " +
            reason +
            " Waiting for the initial run upgrade choice.",
            this
        );
    }

    private void HandleInitialRunUpgradeSelected()
    {
        if (!isRunUpgradeSelectionActive)
        {
            return;
        }

        initialRunUpgradeResolved = true;

        BeginWaveStartDelay(
            "Initial run upgrade selected."
        );
    }

    private void BeginWaveStartDelay(
        string reason
    )
    {
        if (!isPreparationActive &&
            !isRunUpgradeSelectionActive)
        {
            return;
        }

        isPreparationActive = false;
        isRunUpgradeSelectionActive = false;
        remainingTime = 0f;

        towerPlacement.ClosePlacement();

        HideTimer();
        HideBuildGrid();
        ShowCoreTowerHealthUI();

        MovePlayerToSpawnPoint();
        ShowPlayer();

        waveStartDelayRemaining =
            Mathf.Max(
                0f,
                waveStartDelay
            );

        isWaveStartDelayActive = true;

        Debug.Log(
            "PreWavePreparationManager: " +
            reason +
            " First wave starts in " +
            waveStartDelayRemaining.ToString("F1") +
            " seconds."
        );

        if (waveStartDelayRemaining <= 0f)
        {
            StartFirstWave();
        }
    }

    private void StartFirstWave()
    {
        if (!isWaveStartDelayActive)
        {
            return;
        }

        isWaveStartDelayActive = false;
        waveStartDelayRemaining = 0f;

        Debug.Log(
            "PreWavePreparationManager: " +
            "Starting the first wave."
        );

        waveManager.StartWave();
    }

    private void MovePlayerToSpawnPoint()
    {
        if (playerRoot == null ||
            playerSpawnPoint == null)
        {
            return;
        }

        playerRoot.SetPositionAndRotation(
            playerSpawnPoint.position,
            playerSpawnPoint.rotation
        );
    }

    private void HidePlayer()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        if (playerVisual != null)
        {
            playerVisual.SetActive(false);
        }

        if (playerHealthCanvas != null)
        {
            playerHealthCanvas.SetActive(false);
        }

        playerIsVisible = false;
    }

    private void ShowPlayer()
    {
        if (playerVisual != null)
        {
            playerVisual.SetActive(true);
        }

        if (playerHealthCanvas != null)
        {
            playerHealthCanvas.SetActive(true);
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }

        playerIsVisible = true;
    }

    private void ShowTimer()
    {
        if (preparationTimerText != null)
        {
            preparationTimerText.gameObject.SetActive(
                true
            );
        }
    }

    private void HideTimer()
    {
        if (preparationTimerText != null)
        {
            preparationTimerText.gameObject.SetActive(
                false
            );
        }
    }

    private void UpdateTimerText()
    {
        if (preparationTimerText == null)
        {
            return;
        }

        int displayedSeconds =
            Mathf.CeilToInt(
                remainingTime
            );

        preparationTimerText.text =
            "����������: " +
            displayedSeconds;
    }

    private void ShowCoreTowerHealthUI()
    {
        if (coreTowerHealthUI != null)
        {
            coreTowerHealthUI.SetActive(
                true
            );
        }
    }

    private void HideCoreTowerHealthUI()
    {
        if (coreTowerHealthUI != null)
        {
            coreTowerHealthUI.SetActive(
                false
            );
        }
    }

    private void ShowBuildGrid()
    {
        if (defenseTowerBuildGrid != null)
        {
            defenseTowerBuildGrid.SetActive(
                true
            );
        }
    }

    private void HideBuildGrid()
    {
        if (defenseTowerBuildGrid != null)
        {
            defenseTowerBuildGrid.SetActive(
                false
            );
        }
    }

    private void SubscribeToPlacementEvent()
    {
        if (placementEventSubscribed ||
            towerPlacement == null)
        {
            return;
        }

        towerPlacement.AllTowerSlotsUsed +=
            HandleAllTowerSlotsUsed;

        placementEventSubscribed = true;
    }

    private void UnsubscribeFromPlacementEvent()
    {
        if (!placementEventSubscribed ||
            towerPlacement == null)
        {
            return;
        }

        towerPlacement.AllTowerSlotsUsed -=
            HandleAllTowerSlotsUsed;

        placementEventSubscribed = false;
    }

    private bool ValidateReferences()
    {
        bool referencesAreValid = true;

        if (waveManager == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Wave Manager is not assigned."
            );

            referencesAreValid = false;
        }

        if (towerPlacement == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Tower Placement is not assigned."
            );

            referencesAreValid = false;
        }

        if (playerRoot == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Player Root is not assigned."
            );

            referencesAreValid = false;
        }

        if (playerSpawnPoint == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Player Spawn Point is not assigned."
            );

            referencesAreValid = false;
        }

        if (playerVisual == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Player Visual is not assigned."
            );

            referencesAreValid = false;
        }

        if (playerHealthCanvas == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Player Health Canvas is not assigned."
            );

            referencesAreValid = false;
        }

        if (playerCollider == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Player Collider is not assigned."
            );

            referencesAreValid = false;
        }

        if (playerMovement == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Player Movement is not assigned."
            );

            referencesAreValid = false;
        }

        if (playerAttack == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Player Attack is not assigned."
            );

            referencesAreValid = false;
        }

        if (preparationTimerText == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Preparation Timer Text is not assigned."
            );

            referencesAreValid = false;
        }

        if (coreTowerHealthUI == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Core Tower Health UI is not assigned."
            );

            referencesAreValid = false;
        }

        if (runUpgradeSelectionUI == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: Run Upgrade " +
                "Selection UI is not assigned. Heroes at " +
                "level 2 or above will not receive the " +
                "initial run upgrade choice."
            );
        }

        if (defenseTowerBuildGrid == null)
        {
            Debug.LogWarning(
                "PreWavePreparationManager: " +
                "Defense Tower Build Grid is not assigned."
            );

            referencesAreValid = false;
        }

        return referencesAreValid;
    }

    private void OnValidate()
    {
        preparationDuration =
            Mathf.Max(
                0f,
                preparationDuration
            );

        waveStartDelay =
            Mathf.Max(
                0f,
                waveStartDelay
            );

        availableTowerSlots =
            Mathf.Max(
                1,
                availableTowerSlots
            );
    }

    private void OnDisable()
    {
        UnsubscribeFromPlacementEvent();
    }
}
