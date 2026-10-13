using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BetweenWaveRewardManager : MonoBehaviour
{
    [Header("Master Balance Source")]
    [SerializeField]
    private GameBalanceConfig balanceConfig;

    [Header("System References")]
    [SerializeField]
    private WaveManager waveManager;

    [SerializeField]
    private RunUpgradeSelectionUI runUpgradeSelectionUI;

    [Header("Health References")]
    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private CoreTowerHealth coreTowerHealth;

    [Header("Debug - Do Not Edit")]
    [SerializeField]
    private bool isResolvingReward;

    [SerializeField]
    private int lastCompletedWave;

    [SerializeField]
    private int lastHeroHealthRestored;

    [SerializeField]
    private int lastCoreTowerHealthRestored;

    [SerializeField]
    private BetweenWaveRewardType lastResolvedRewardType;

    private bool waveEventSubscribed;
    private Coroutine continueRoutine;

    public bool IsResolvingReward =>
        isResolvingReward;


    private void Awake()
    {
        ResolveReferences();
        ValidateBalanceConfig();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToWaveEvent();
    }

    private void HandleWaveCompleted(
        int completedWave
    )
    {
        lastCompletedWave =
            completedWave;

        if (waveManager == null ||
            completedWave >= waveManager.TotalWaves)
        {
            isResolvingReward = false;
            lastResolvedRewardType =
                BetweenWaveRewardType.None;

            Debug.Log(
                "BetweenWaveRewardManager: Final wave " +
                "completed. No next wave will be started.",
                this
            );

            return;
        }

        if (balanceConfig == null)
        {
            isResolvingReward = false;

            Debug.LogError(
                "BetweenWaveRewardManager: GameBalanceConfig " +
                "is not assigned.",
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

        BetweenWaveRewardType rewardType =
            balanceConfig.GetBetweenWaveRewardType(
                completedWave
            );

        lastResolvedRewardType =
            rewardType;

        switch (rewardType)
        {
            case BetweenWaveRewardType.HealthRestore:
                RestoreHealthReward(
                    completedWave
                );

                BeginNextWaveAfterDelay(
                    balanceConfig
                        .BetweenWaveHealthRewardDelay
                );

                return;

            case BetweenWaveRewardType.HeroUpgrade:
                if (TryOpenUpgradeSelection(
                        completedWave,
                        RunUpgradeCategory.Hero
                    ))
                {
                    return;
                }

                break;

            case BetweenWaveRewardType.DefenseTowerUpgrade:
                if (TryOpenUpgradeSelection(
                        completedWave,
                        RunUpgradeCategory.DefenseTower
                    ))
                {
                    return;
                }

                break;
        }

        BeginNextWaveAfterDelay(
            balanceConfig.BetweenWaveNextWaveDelay
        );
    }

    private bool TryOpenUpgradeSelection(
        int completedWave,
        RunUpgradeCategory category
    )
    {
        if (runUpgradeSelectionUI == null)
        {
            Debug.LogError(
                "BetweenWaveRewardManager: Assign " +
                "RunUpgradeSelectionUI.",
                this
            );

            return false;
        }

        if (runUpgradeSelectionUI.TryOpenSelection(
                category,
                HandleUpgradeSelectionCompleted
            ))
        {
            Debug.Log(
                "BetweenWaveRewardManager: Waiting for " +
                $"the wave {completedWave} upgrade choice.",
                this
            );

            return true;
        }

        Debug.Log(
            "BetweenWaveRewardManager: No upgrade " +
            "selection was opened for " +
            $"{category}. The next wave will continue.",
            this
        );

        return false;
    }

    private void HandleUpgradeSelectionCompleted()
    {
        if (!isResolvingReward ||
            balanceConfig == null)
        {
            return;
        }

        BeginNextWaveAfterDelay(
            balanceConfig.BetweenWaveNextWaveDelay
        );
    }

    private void RestoreHealthReward(
        int completedWave
    )
    {
        if (balanceConfig == null)
        {
            return;
        }

        float restorePercent =
            balanceConfig
                .BetweenWaveHealthRestorePercent;

        lastHeroHealthRestored =
            playerHealth != null
                ? playerHealth.RestoreHealthPercent(
                    restorePercent
                )
                : 0;

        lastCoreTowerHealthRestored =
            coreTowerHealth != null
                ? coreTowerHealth.RestoreHealthPercent(
                    restorePercent
                )
                : 0;

        Debug.Log(
            "BetweenWaveRewardManager: Wave " +
            $"{completedWave} health reward resolved. " +
            $"Restore percent = {restorePercent:F1}%. " +
            $"Hero restored {lastHeroHealthRestored} HP. " +
            "CoreTower restored " +
            $"{lastCoreTowerHealthRestored} HP.",
            this
        );
    }

    private void BeginNextWaveAfterDelay(
        float delay
    )
    {
        if (continueRoutine != null)
        {
            StopCoroutine(
                continueRoutine
            );
        }

        continueRoutine =
            StartCoroutine(
                ContinueToNextWave(
                    Mathf.Max(
                        0f,
                        delay
                    )
                )
            );
    }

    private IEnumerator ContinueToNextWave(
        float delay
    )
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(
                delay
            );
        }

        continueRoutine = null;

        if (waveManager == null ||
            waveManager.AreAllWavesCompleted)
        {
            isResolvingReward = false;
            yield break;
        }

        bool waveStarted =
            waveManager.TryStartWave();

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

    private void ResolveReferences()
    {
        if (waveManager == null)
        {
            waveManager =
                FindAnyObjectByType<WaveManager>();
        }

        if (runUpgradeSelectionUI == null)
        {
            runUpgradeSelectionUI =
                FindAnyObjectByType<RunUpgradeSelectionUI>();
        }

        if (playerHealth == null)
        {
            playerHealth =
                FindAnyObjectByType<PlayerHealth>();
        }

        if (coreTowerHealth == null)
        {
            coreTowerHealth =
                FindAnyObjectByType<CoreTowerHealth>();
        }
    }

    private void ValidateBalanceConfig()
    {
        if (balanceConfig != null)
        {
            return;
        }

        Debug.LogError(
            "BetweenWaveRewardManager: Assign " +
            "GameBalanceConfig.",
            this
        );
    }

    private void SubscribeToWaveEvent()
    {
        if (waveEventSubscribed ||
            waveManager == null)
        {
            return;
        }

        waveManager.WaveCompleted +=
            HandleWaveCompleted;

        waveEventSubscribed = true;
    }

    [ContextMenu(
        "Test Rewards/Open Tower Upgrade Selection"
    )]
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

    [ContextMenu(
        "Test Rewards/Restore Health"
    )]
    private void TestRestoreHealthReward()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ResolveReferences();
        RestoreHealthReward(
            5
        );
    }

    private void UnsubscribeFromWaveEvent()
    {
        if (!waveEventSubscribed ||
            waveManager == null)
        {
            return;
        }

        waveManager.WaveCompleted -=
            HandleWaveCompleted;

        waveEventSubscribed = false;
    }

    private void OnDisable()
    {
        UnsubscribeFromWaveEvent();

        if (continueRoutine != null)
        {
            StopCoroutine(
                continueRoutine
            );

            continueRoutine = null;
        }

        isResolvingReward = false;
    }
}
