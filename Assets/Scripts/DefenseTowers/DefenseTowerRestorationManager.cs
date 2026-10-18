using UnityEngine;

// Prototype IMGUI interface. One irreversible restoration choice per run.
[DisallowMultipleComponent]
public sealed class DefenseTowerRestorationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveManager waveManager;

    [Header("Unlock - Temporary Prototype Setting")]
    [Tooltip("Temporary test switch; later replace with a permanent hero-level unlock.")]
    [SerializeField] private bool testHeroHasUnlockedRestoration = true;

    [Header("Balance")]
    [Min(1)][SerializeField] private int unlockAfterCompletedWave = 10;
    [Min(0)][SerializeField] private int unusedTokenGoldReward = 100;

    [Header("Temporary Prototype UI")]
    [SerializeField] private bool showRestorationPanel = true;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool restorationTokenEarned;
    [SerializeField] private bool restorationTokenUsed;
    [SerializeField] private bool convertedToGold;
    [SerializeField] private bool restorationQueued;
    [SerializeField] private int lastCompletedWave;

    private enum MenuStep { Closed, ChooseRuins, Confirm }
    private MenuStep menuStep;
    private DefenseTowerHealth selectedRuin;
    private DefenseTowerHealth queuedRuin;
    private bool subscribed;
    private bool milestoneDecisionPending;
    private bool keptTokenAfterMilestone;
    private System.Action milestoneDecisionFinished;
    private float previousTimeScale = 1f;

    public bool CanRestore => testHeroHasUnlockedRestoration &&
                              restorationTokenEarned && !restorationTokenUsed;

    private void Awake() { ResolveReferences(); }

    private void OnEnable()
    {
        ResolveReferences();
        if (waveManager == null)
        {
            Debug.LogError("DefenseTowerRestorationManager: WaveManager not found.", this);
            return;
        }
        if (!subscribed)
        {
            waveManager.WaveCompleted += HandleWaveCompleted;
            subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (subscribed && waveManager != null)
            waveManager.WaveCompleted -= HandleWaveCompleted;
        subscribed = false;
        menuStep = MenuStep.Closed;
        selectedRuin = null;
        if (milestoneDecisionPending)
        {
            Time.timeScale = previousTimeScale;
            milestoneDecisionPending = false;
            milestoneDecisionFinished = null;
        }
    }

    private void ResolveReferences()
    {
        if (waveManager == null) waveManager = FindAnyObjectByType<WaveManager>();
    }

    private void HandleWaveCompleted(int completedWave)
    {
        lastCompletedWave = completedWave;

        // First complete a restoration reserved during this wave.
        if (restorationQueued)
            CompleteQueuedRestoration();

        if (completedWave == unlockAfterCompletedWave &&
            testHeroHasUnlockedRestoration && !restorationTokenEarned)
            EarnToken();

        if (waveManager != null && completedWave >= waveManager.TotalWaves)
        {
            menuStep = MenuStep.Closed;
            selectedRuin = null;
            ConvertUnusedTokenToGold();
        }
    }

    private void EarnToken()
    {
        if (restorationTokenEarned) return;
        restorationTokenEarned = true;
        Debug.Log("DefenseTowerRestorationManager: One FREE tower restoration earned.", this);
    }

    // Called after the wave-10 health reward. Blocks the next wave only if ruins exist.
    public bool TryBeginWaveTenDecision(System.Action onFinished)
    {
        if (!testHeroHasUnlockedRestoration || !showRestorationPanel ||
            waveManager == null || waveManager.CurrentWave != unlockAfterCompletedWave ||
            milestoneDecisionPending || keptTokenAfterMilestone)
            return false;

        EarnToken();
        if (!CanRestore || CountRuins() == 0)
        {
            // No ruins: keep the token silently. Button appears only if ruins appear later.
            keptTokenAfterMilestone = CanRestore;
            Debug.Log("DefenseTowerRestorationManager: Wave 10 has no ruins. Token retained without opening panel.", this);
            return false;
        }

        milestoneDecisionPending = true;
        menuStep = MenuStep.ChooseRuins;
        milestoneDecisionFinished = onFinished;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        Debug.Log("DefenseTowerRestorationManager: Wave 10 decision opened. Waiting for RESTORE or KEEP TOKEN.", this);
        return true;
    }

    private void FinishMilestoneDecision(bool keepToken)
    {
        if (!milestoneDecisionPending) return;
        keptTokenAfterMilestone = keepToken;
        milestoneDecisionPending = false;
        menuStep = MenuStep.Closed;
        selectedRuin = null;
        Time.timeScale = previousTimeScale;
        System.Action finished = milestoneDecisionFinished;
        milestoneDecisionFinished = null;
        Debug.Log(keepToken
            ? "DefenseTowerRestorationManager: KEEP TOKEN selected. Window closed; compact button enabled."
            : "DefenseTowerRestorationManager: Restoration confirmed; milestone window closed.", this);
        finished?.Invoke();
    }

    private bool CanOpenMenu => CanRestore && !restorationQueued &&
        waveManager != null && !waveManager.AreAllWavesCompleted &&
        (milestoneDecisionPending || keptTokenAfterMilestone) &&
        !(waveManager.IsWaveActive && waveManager.CurrentWave >= waveManager.TotalWaves);

    private int CountRuins()
    {
        int count = 0;
        foreach (DefenseTowerHealth tower in FindObjectsByType<DefenseTowerHealth>(FindObjectsInactive.Exclude))
            if (tower != null && tower.IsDestroyed) count++;
        return count;
    }

    private bool ConfirmRestoration()
    {
        if (!CanOpenMenu || selectedRuin == null || !selectedRuin.IsDestroyed)
        {
            menuStep = MenuStep.Closed;
            selectedRuin = null;
            return false;
        }

        DefenseTowerHealth tower = selectedRuin;
        bool duringWave = waveManager.IsWaveActive;
        bool fromMilestone = milestoneDecisionPending;

        // Reserve and consume before any other callback can request another tower.
        restorationTokenUsed = true;
        menuStep = MenuStep.Closed;
        selectedRuin = null;
        if (fromMilestone)
            FinishMilestoneDecision(false);

        if (duringWave)
        {
            queuedRuin = tower;
            restorationQueued = true;
            Debug.Log($"DefenseTowerRestorationManager: Confirmed {tower.name}. Token consumed. " +
                      $"Restoration queued until the end of wave {waveManager.CurrentWave}.", this);
            return true;
        }

        if (tower.TryRestoreFromRuins())
        {
            Debug.Log($"DefenseTowerRestorationManager: Restored {tower.name} between waves. Token consumed.", this);
            return true;
        }

        // Only a failed engine operation rolls back the spend, never a player decision.
        restorationTokenUsed = false;
        Debug.LogError("DefenseTowerRestorationManager: Restoration failed; token returned.", this);
        return false;
    }

    private void CompleteQueuedRestoration()
    {
        DefenseTowerHealth tower = queuedRuin;
        queuedRuin = null;
        restorationQueued = false;
        if (tower != null && tower.IsDestroyed && tower.TryRestoreFromRuins())
        {
            Debug.Log($"DefenseTowerRestorationManager: Restored {tower.name} after wave {lastCompletedWave}. " +
                      "Previously earned tower upgrades preserved.", this);
        }
        else
        {
            Debug.LogError("DefenseTowerRestorationManager: Queued ruin could not be restored. Token returned.", this);
            restorationTokenUsed = false;
        }
    }

    private void ConvertUnusedTokenToGold()
    {
        if (convertedToGold || !CanRestore || restorationQueued) return;
        if (unusedTokenGoldReward <= 0)
        {
            // No gold to add; token still expires at the end of the run.
            convertedToGold = true;
            restorationTokenUsed = true;
            Debug.Log("DefenseTowerRestorationManager: Unused token expired (gold reward = 0).", this);
            return;
        }

        PlayerProgression progression = FindAnyObjectByType<PlayerProgression>();
        if (progression == null || !progression.AddGold(unusedTokenGoldReward))
        {
            Debug.LogError("DefenseTowerRestorationManager: Unable to grant unused-token gold.", this);
            return;
        }
        restorationTokenUsed = true;
        convertedToGold = true;
        Debug.Log($"DefenseTowerRestorationManager: Final wave finished. Unused token converted to " +
                  $"{unusedTokenGoldReward} gold.", this);
    }

    private string BuildRestorationLabel(DefenseTowerHealth tower, int number)
    {
        DefenseTowerRunStats runStats = tower.GetComponent<DefenseTowerRunStats>();
        float damage = runStats != null ? runStats.DamageBonusPercent : 0f;
        float speed = runStats != null ? runStats.AttackSpeedBonusPercent : 0f;
        CombatStats baseStats = FindAnyObjectByType<CombatStats>();
        int baseMaxHealth = baseStats != null ? baseStats.DefenseTowerMaxHealth : tower.MaxHealth;
        int bonusHp = Mathf.Max(0, tower.MaxHealth - baseMaxHealth);
        return $"TOWER {number} | Damage +{damage:F0}% | Speed +{speed:F0}%\n" +
               $"HP +{bonusHp} | Max HP {tower.MaxHealth}";
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || !showRestorationPanel) return;
        if (!CanOpenMenu)
        {
            menuStep = MenuStep.Closed;
            selectedRuin = null;
            return;
        }

        DefenseTowerHealth[] towers = FindObjectsByType<DefenseTowerHealth>(FindObjectsInactive.Exclude);
        int count = CountRuins();
        if (count == 0 && menuStep == MenuStep.Confirm)
        {
            menuStep = MenuStep.ChooseRuins;
            selectedRuin = null;
        }

        float width = Mathf.Min(420f, Screen.width - 20f);
        if (menuStep == MenuStep.Closed)
        {
            // Button is deliberately forbidden before the wave-10 KEEP TOKEN decision.
            if (!keptTokenAfterMilestone) return;
            if (GUI.Button(new Rect(10f, 85f, Mathf.Min(210f, width), 44f), "RESTORE TOWER (1)"))
                menuStep = MenuStep.ChooseRuins;
            return;
        }

        float height = menuStep == MenuStep.ChooseRuins ? 154f + count * 64f : 200f;
        GUILayout.BeginArea(new Rect(10f, 85f, width, height), GUI.skin.box);

        if (menuStep == MenuStep.ChooseRuins)
        {
            GUILayout.Label("CHOOSE ONE RUIN (1 restoration)");
            GUILayout.Label(count == 0
                ? "No ruined towers yet. Your token is safe."
                : "Select a tower to review before confirming.");
            int number = 0;
            foreach (DefenseTowerHealth tower in towers)
            {
                if (tower == null || !tower.IsDestroyed) continue;
                number++;
                if (GUILayout.Button(BuildRestorationLabel(tower, number), GUILayout.Height(54f)))
                {
                    selectedRuin = tower;
                    menuStep = MenuStep.Confirm;
                    break;
                }
            }
            if (milestoneDecisionPending)
            {
                if (GUILayout.Button("KEEP TOKEN - Continue to wave 11", GUILayout.Height(36f)))
                    FinishMilestoneDecision(true);
            }
            else if (GUILayout.Button("CLOSE - Keep Token", GUILayout.Height(32f)))
                menuStep = MenuStep.Closed;
        }
        else
        {
            if (selectedRuin == null || !selectedRuin.IsDestroyed)
            {
                selectedRuin = null;
                menuStep = MenuStep.ChooseRuins;
            }
            else
            {
                GUILayout.Label("CONFIRM RESTORATION");
                GUILayout.Label(BuildRestorationLabel(selectedRuin, 1));
                GUILayout.Label(waveManager.IsWaveActive
                    ? "Restoration AFTER this wave. Final decision, no cancellation."
                    : "Restore immediately. Final decision, no cancellation.");
                if (GUILayout.Button("CONFIRM - SPEND THE ONLY TOKEN", GUILayout.Height(36f)))
                    ConfirmRestoration();
                if (GUILayout.Button("BACK - DO NOT SPEND", GUILayout.Height(32f)))
                {
                    selectedRuin = null;
                    menuStep = MenuStep.ChooseRuins;
                }
            }
        }
        GUILayout.EndArea();
    }

    [ContextMenu("Test Restoration/Grant Token Now")]
    private void TestGrantToken()
    {
        if (Application.isPlaying && testHeroHasUnlockedRestoration) EarnToken();
        // Test grants never bypass the mandatory wave-10 decision gate.
    }

    [ContextMenu("Test Restoration/Print State")]
    private void PrintState()
    {
        Debug.Log($"DefenseTowerRestorationManager: wave={lastCompletedWave}, earned={restorationTokenEarned}, " +
                  $"used={restorationTokenUsed}, queued={restorationQueued}, converted={convertedToGold}.", this);
    }

    private void OnValidate()
    {
        unlockAfterCompletedWave = Mathf.Max(1, unlockAfterCompletedWave);
        unusedTokenGoldReward = Mathf.Max(0, unusedTokenGoldReward);
    }
}
