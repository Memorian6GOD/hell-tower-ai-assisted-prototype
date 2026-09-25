using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BetweenWaveRewardManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private WaveManager waveManager;

    [SerializeField]
    private RunUpgradeSelectionUI runUpgradeSelectionUI;

    [Header("Health References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private CoreTowerHealth coreTowerHealth;

    [Header("Reward Settings")]
    [SerializeField, Range(0f, 100f)]
    private float healthRestorePercent = 25f;

    [SerializeField, Min(0f)]
    private float nextWaveDelay = 0.5f;

    [SerializeField, Min(0f)]
    private float healthRewardDelay = 2f;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool isResolvingReward;
    [SerializeField] private int lastCompletedWave;
    [SerializeField] private int lastHeroHealthRestored;
    [SerializeField] private int lastCoreTowerHealthRestored;

    private bool waveEventSubscribed;
    private Coroutine continueRoutine;

    public bool IsResolvingReward => isResolvingReward;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToWaveEvent();
    }

    private void HandleWaveCompleted(int completedWave)
    {
        lastCompletedWave = completedWave;

        if (waveManager == null ||
            completedWave >= waveManager.TotalWaves)
        {
            isResolvingReward = false;

            Debug.Log(
                "BetweenWaveRewardManager: Final wave " +
                "completed. No next wave will be started.",
                this
            );

            return;
        }

        if (isResolvingReward)
        {
            Debug.LogWarning(
                "BetweenWaveRewardManager: Another reward " +
                "is already being resolved.",
                this
            );

            return;
        }

        isResolvingReward = true;

        if (IsHealthRewardWave(completedWave))
        {
            RestoreHealthReward(completedWave);
            BeginNextWaveAfterDelay(healthRewardDelay);
            return;
        }

        if (TryGetUpgradeCategory(
                completedWave,
                out RunUpgradeCategory category
            ))
        {
            if (runUpgradeSelectionUI == null)
            {
                Debug.LogError(
                    "BetweenWaveRewardManager: Assign " +
                    "RunUpgradeSelectionUI.",
                    this
                );
            }
            else if (runUpgradeSelectionUI.TryOpenSelection(
                    category,
                    HandleUpgradeSelectionCompleted
                ))
            {
                Debug.Log(
                    "BetweenWaveRewardManager: Waiting for " +
                    $"the wave {completedWave} upgrade choice.",
                    this
                );

                return;
            }

            Debug.Log(
                "BetweenWaveRewardManager: No upgrade " +
                "selection was opened for " +
                $"{category}. The next wave will continue.",
                this
            );
        }

        BeginNextWaveAfterDelay(nextWaveDelay);
    }

    private void HandleUpgradeSelectionCompleted()
    {
        if (!isResolvingReward)
        {
            return;
        }

        BeginNextWaveAfterDelay(nextWaveDelay);
    }

    private void RestoreHealthReward(int completedWave)
    {
        lastHeroHealthRestored = playerHealth != null
            ? playerHealth.RestoreHealthPercent(
                healthRestorePercent
            )
            : 0;

        lastCoreTowerHealthRestored = coreTowerHealth != null
            ? coreTowerHealth.RestoreHealthPercent(
                healthRestorePercent
            )
            : 0;

        Debug.Log(
            "BetweenWaveRewardManager: Wave " +
            $"{completedWave} health reward resolved. " +
            $"Hero restored {lastHeroHealthRestored} HP. " +
            "CoreTower restored " +
            $"{lastCoreTowerHealthRestored} HP.",
            this
        );
    }

    private void BeginNextWaveAfterDelay(float delay)
    {
        if (continueRoutine != null)
        {
            StopCoroutine(continueRoutine);
        }

        continueRoutine = StartCoroutine(
            ContinueToNextWave(delay)
        );
    }

    private IEnumerator ContinueToNextWave(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        continueRoutine = null;

        if (waveManager == null ||
            waveManager.AreAllWavesCompleted)
        {
            isResolvingReward = false;
            yield break;
        }

        bool waveStarted = waveManager.TryStartWave();
        isResolvingReward = false;

        if (!waveStarted)
        {
            Debug.LogError(
                "BetweenWaveRewardManager: The next wave " +
                "could not be started.",
                this
            );
        }
    }

    private static bool IsHealthRewardWave(int completedWave)
    {
        return completedWave == 5 || completedWave == 10;
    }

    private static bool TryGetUpgradeCategory(
        int completedWave,
        out RunUpgradeCategory category
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
                category = RunUpgradeCategory.Hero;
                return true;

            case 2:
            case 4:
            case 7:
            case 9:
            case 12:
            case 14:
                category = RunUpgradeCategory.DefenseTower;
                return true;

            default:
                category = RunUpgradeCategory.Hero;
                return false;
        }
    }

    private void ResolveReferences()
    {
        if (waveManager == null)
        {
            waveManager = FindAnyObjectByType<WaveManager>();
        }

        if (runUpgradeSelectionUI == null)
        {
            runUpgradeSelectionUI =
                FindAnyObjectByType<RunUpgradeSelectionUI>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        if (coreTowerHealth == null)
        {
            coreTowerHealth =
                FindAnyObjectByType<CoreTowerHealth>();
        }
    }

    private void SubscribeToWaveEvent()
    {
        if (waveEventSubscribed || waveManager == null)
        {
            return;
        }

        waveManager.WaveCompleted += HandleWaveCompleted;
        waveEventSubscribed = true;
    }

    [ContextMenu("Test Rewards/Open Tower Upgrade Selection")]
    private void TestOpenTowerUpgradeSelection()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ResolveReferences();

        if (runUpgradeSelectionUI != null)
        {
            runUpgradeSelectionUI.TryOpenSelection(
                RunUpgradeCategory.DefenseTower,
                null
            );
        }
    }

    [ContextMenu("Test Rewards/Restore Health")]
    private void TestRestoreHealthReward()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ResolveReferences();
        RestoreHealthReward(5);
    }

    private void UnsubscribeFromWaveEvent()
    {
        if (!waveEventSubscribed || waveManager == null)
        {
            return;
        }

        waveManager.WaveCompleted -= HandleWaveCompleted;
        waveEventSubscribed = false;
    }

    private void OnValidate()
    {
        healthRestorePercent = Mathf.Clamp(
            healthRestorePercent,
            0f,
            100f
        );

        nextWaveDelay = Mathf.Max(0f, nextWaveDelay);
        healthRewardDelay = Mathf.Max(0f, healthRewardDelay);
    }

    private void OnDisable()
    {
        UnsubscribeFromWaveEvent();

        if (continueRoutine != null)
        {
            StopCoroutine(continueRoutine);
            continueRoutine = null;
        }

        isResolvingReward = false;
    }
}
