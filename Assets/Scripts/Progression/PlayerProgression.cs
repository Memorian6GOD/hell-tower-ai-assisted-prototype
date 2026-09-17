using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProgression : MonoBehaviour
{
    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool debugLoaded;
    [SerializeField] private string debugSaveFilePath;
    [SerializeField] private int debugGold;
    [SerializeField] private int debugHeroLevel;
    [SerializeField] private int debugHeroDamagePurchases;
    [SerializeField] private int debugHeroCriticalChancePurchases;
    [SerializeField] private int debugHeroAttackSpeedPurchases;
    [SerializeField] private int debugHeroMaxHealthPurchases;
    [SerializeField] private int debugCoreTowerMaxHealthPurchases;
    [SerializeField] private int debugDefenseTowerMaxHealthPurchases;
    [SerializeField] private bool debugMapOneRewardClaimed;

    private PlayerProgressData progress;

    public event Action ProgressChanged;

    public int Gold => progress != null ? progress.Gold : 0;
    public int HeroLevel => progress != null ? progress.HeroLevel : 1;

    private void Awake()
    {
        progress = PlayerProgressSaveSystem.Load();
        debugLoaded = true;
        RefreshDebugInfo();

        Debug.Log(
            $"PlayerProgression: Progress loaded. " +
            $"Gold = {Gold}, Hero level = {HeroLevel}. " +
            $"Save file: {PlayerProgressSaveSystem.SaveFilePath}",
            this
        );
    }

    private void LateUpdate()
    {
        RefreshDebugInfo();
    }

    public int GetPurchaseCount(PermanentUpgradeType upgradeType)
    {
        if (progress == null)
        {
            return 0;
        }

        return progress.GetPurchaseCount(upgradeType);
    }

    public bool IsFirstVictoryRewardClaimed(int mapId)
    {
        return progress != null &&
            progress.IsFirstVictoryRewardClaimed(mapId);
    }

    public bool AddGold(int amount)
    {
        if (progress == null || !progress.TryAddGold(amount))
        {
            return false;
        }

        return SaveAfterChange(
            $"Added {amount} gold. Total gold = {progress.Gold}."
        );
    }

    public bool TrySpendGold(int amount)
    {
        if (progress == null || !progress.TrySpendGold(amount))
        {
            return false;
        }

        return SaveAfterChange(
            $"Spent {amount} gold. Remaining gold = {progress.Gold}."
        );
    }

    public bool TryClaimFirstVictoryReward(
        int mapId,
        int goldReward
    )
    {
        if (progress == null ||
            !progress.TryClaimFirstVictoryReward(mapId, goldReward))
        {
            Debug.Log(
                $"PlayerProgression: First victory reward for map {mapId} " +
                "was not claimed. Check the map ID, reward, or previous claim.",
                this
            );
            return false;
        }

        return SaveAfterChange(
            $"First victory reward claimed for map {mapId}. " +
            $"Gold reward = {goldReward}. Total gold = {progress.Gold}."
        );
    }

    private bool SaveAfterChange(string successMessage)
    {
        bool saveSucceeded =
            PlayerProgressSaveSystem.Save(progress);

        RefreshDebugInfo();

        if (!saveSucceeded)
        {
            Debug.LogError(
                "PlayerProgression: Progress changed in memory, " +
                "but saving failed.",
                this
            );
            return false;
        }

        Debug.Log("PlayerProgression: " + successMessage, this);
        ProgressChanged?.Invoke();
        return true;
    }

    private void RefreshDebugInfo()
    {
        debugSaveFilePath = PlayerProgressSaveSystem.SaveFilePath;

        if (progress == null)
        {
            return;
        }

        debugGold = progress.Gold;
        debugHeroLevel = progress.HeroLevel;

        debugHeroDamagePurchases = progress.GetPurchaseCount(
            PermanentUpgradeType.HeroDamage
        );

        debugHeroCriticalChancePurchases = progress.GetPurchaseCount(
            PermanentUpgradeType.HeroCriticalChance
        );

        debugHeroAttackSpeedPurchases = progress.GetPurchaseCount(
            PermanentUpgradeType.HeroAttackSpeed
        );

        debugHeroMaxHealthPurchases = progress.GetPurchaseCount(
            PermanentUpgradeType.HeroMaxHealth
        );

        debugCoreTowerMaxHealthPurchases = progress.GetPurchaseCount(
            PermanentUpgradeType.CoreTowerMaxHealth
        );

        debugDefenseTowerMaxHealthPurchases = progress.GetPurchaseCount(
            PermanentUpgradeType.DefenseTowerMaxHealth
        );

        debugMapOneRewardClaimed =
            progress.IsFirstVictoryRewardClaimed(1);
    }

    [ContextMenu("Test Progress/Add 500 Gold")]
    private void TestAdd500Gold()
    {
        if (!Application.isPlaying)
            return;

        AddGold(500);
    }

    [ContextMenu("Test Progress/Spend 200 Gold")]
    private void TestSpend200Gold()
    {
        if (!Application.isPlaying)
            return;

        bool spendSucceeded = TrySpendGold(200);

        if (!spendSucceeded)
        {
            Debug.Log(
                "PlayerProgression: Could not spend 200 gold.",
                this
            );
        }
    }

    [ContextMenu("Test Progress/Set Hero Level To 3")]
    private void TestSetHeroLevelTo3()
    {
        if (!Application.isPlaying || progress == null)
            return;

        progress.SetHeroLevelForTesting(3);
        SaveAfterChange("Hero level set to 3 for testing.");
    }

    [ContextMenu("Test Progress/Add One Purchase To Every Stat")]
    private void TestAddOnePurchaseToEveryStat()
    {
        if (!Application.isPlaying || progress == null)
            return;

        foreach (PermanentUpgradeType upgradeType in
                 Enum.GetValues(typeof(PermanentUpgradeType)))
        {
            progress.TryAddPurchaseForTesting(upgradeType);
        }

        SaveAfterChange(
            "Added one test purchase to every permanent stat."
        );
    }

    [ContextMenu("Test Progress/Claim Map 1 First Reward (+100 Gold)")]
    private void TestClaimMapOneFirstReward()
    {
        if (!Application.isPlaying)
            return;

        TryClaimFirstVictoryReward(1, 100);
    }

    [ContextMenu("Test Progress/Print Progress")]
    private void TestPrintProgress()
    {
        if (!Application.isPlaying || progress == null)
            return;

        Debug.Log(
            $"PlayerProgression | Gold: {progress.Gold} | " +
            $"Hero level: {progress.HeroLevel} | " +
            $"Damage: {progress.GetPurchaseCount(PermanentUpgradeType.HeroDamage)} | " +
            $"Crit: {progress.GetPurchaseCount(PermanentUpgradeType.HeroCriticalChance)} | " +
            $"Attack speed: {progress.GetPurchaseCount(PermanentUpgradeType.HeroAttackSpeed)} | " +
            $"Hero HP: {progress.GetPurchaseCount(PermanentUpgradeType.HeroMaxHealth)} | " +
            $"CoreTower HP: {progress.GetPurchaseCount(PermanentUpgradeType.CoreTowerMaxHealth)} | " +
            $"Defense tower HP: {progress.GetPurchaseCount(PermanentUpgradeType.DefenseTowerMaxHealth)} | " +
            $"Map 1 reward claimed: {progress.IsFirstVictoryRewardClaimed(1)}",
            this
        );
    }

    [ContextMenu("Test Progress/Reset Save To Defaults")]
    private void TestResetSaveToDefaults()
    {
        if (!Application.isPlaying)
            return;

        if (!PlayerProgressSaveSystem.DeleteSaveFile())
        {
            return;
        }

        progress = new PlayerProgressData();
        progress.Validate();

        SaveAfterChange("Save reset to default progress.");
    }
}
