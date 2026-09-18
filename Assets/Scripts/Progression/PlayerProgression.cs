using System;
using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public class PlayerProgression : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField]
    private PermanentProgressionConfig progressionConfig;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool debugLoaded;
    [SerializeField] private string debugSaveFilePath;
    [SerializeField] private int debugGold;
    [SerializeField] private int debugHeroLevel;

    [SerializeField] private int debugHeroDamagePurchases;
    [SerializeField] private int debugHeroDamagePurchaseLimit;
    [SerializeField] private int debugNextHeroDamagePrice;
    [SerializeField] private float debugHeroPermanentDamageBonus;

    [SerializeField] private int debugHeroCriticalChancePurchases;
    [SerializeField] private int debugHeroCriticalChancePurchaseLimit;
    [SerializeField] private int debugNextHeroCriticalChancePrice;

    [SerializeField]
    private float debugHeroPermanentCriticalChanceBonusPoints;

    [SerializeField] private int debugHeroAttackSpeedPurchases;
    [SerializeField] private int debugHeroAttackSpeedPurchaseLimit;
    [SerializeField] private int debugNextHeroAttackSpeedPrice;

    [SerializeField]
    private float debugHeroPermanentAttackSpeedBonusPercent;

    [SerializeField] private int debugSharedMaxHealthPurchases;
    [SerializeField] private int debugSharedMaxHealthPurchaseLimit;
    [SerializeField] private int debugNextSharedMaxHealthPrice;
    [SerializeField] private float debugSharedMaxHealthBonusPercent;

    [SerializeField] private bool debugMapOneRewardClaimed;

    private PlayerProgressData progress;

    public event Action ProgressChanged;

    public int Gold =>
        progress != null ? progress.Gold : 0;

    public int HeroLevel =>
        progress != null ? progress.HeroLevel : 1;

    public float HeroPermanentDamageBonus
    {
        get
        {
            if (progress == null || progressionConfig == null)
            {
                return 0f;
            }

            return progressionConfig.GetTotalHeroDamageBonus(
                GetPurchaseCount(
                    PermanentUpgradeType.HeroDamage
                )
            );
        }
    }

    public float HeroPermanentCriticalChanceBonusPoints
    {
        get
        {
            if (progress == null || progressionConfig == null)
            {
                return 0f;
            }

            return progressionConfig
                .GetTotalHeroCriticalChanceBonusPoints(
                    GetPurchaseCount(
                        PermanentUpgradeType.HeroCriticalChance
                    )
                );
        }
    }

    public float HeroPermanentAttackSpeedBonusPercent
    {
        get
        {
            if (progress == null || progressionConfig == null)
            {
                return 0f;
            }

            return progressionConfig
                .GetTotalHeroAttackSpeedBonusPercent(
                    GetPurchaseCount(
                        PermanentUpgradeType.HeroAttackSpeed
                    )
                );
        }
    }

    public float SharedPermanentMaxHealthBonusPercent
    {
        get
        {
            if (progress == null || progressionConfig == null)
            {
                return 0f;
            }

            return progressionConfig
                .GetTotalSharedMaxHealthBonusPercent(
                    GetPurchaseCount(
                        PermanentUpgradeType.SharedMaxHealth
                    )
                );
        }
    }

    private void Awake()
    {
        progress = PlayerProgressSaveSystem.Load();

        debugLoaded = true;
        RefreshDebugInfo();

        if (progressionConfig == null)
        {
            Debug.LogError(
                "PlayerProgression: Assign PermanentProgressionConfig.",
                this
            );
        }

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

    public int GetPurchaseCount(
        PermanentUpgradeType upgradeType
    )
    {
        if (progress == null)
        {
            return 0;
        }

        return progress.GetPurchaseCount(upgradeType);
    }

    public int GetHeroDamagePurchaseLimit()
    {
        if (progressionConfig == null)
        {
            return 0;
        }

        return progressionConfig.GetRequiredHeroDamagePurchases(
            HeroLevel
        );
    }

    public int GetNextHeroDamagePurchasePrice()
    {
        if (progressionConfig == null)
        {
            return 0;
        }

        return progressionConfig.GetHeroDamagePurchasePrice(
            GetPurchaseCount(
                PermanentUpgradeType.HeroDamage
            )
        );
    }

    public bool IsHeroCriticalChanceUnlocked()
    {
        return HeroLevel >= 2;
    }

    public int GetHeroCriticalChancePurchaseLimit()
    {
        if (progressionConfig == null)
        {
            return 0;
        }

        return progressionConfig
            .GetRequiredHeroCriticalChancePurchases(
                HeroLevel
            );
    }

    public int GetNextHeroCriticalChancePurchasePrice()
    {
        if (progressionConfig == null)
        {
            return 0;
        }

        return progressionConfig
            .GetHeroCriticalChancePurchasePrice(
                GetPurchaseCount(
                    PermanentUpgradeType.HeroCriticalChance
                )
            );
    }

    public bool IsHeroAttackSpeedUnlocked()
    {
        return HeroLevel >= 5;
    }

    public int GetHeroAttackSpeedPurchaseLimit()
    {
        if (progressionConfig == null)
        {
            return 0;
        }

        return progressionConfig
            .GetRequiredHeroAttackSpeedPurchases(
                HeroLevel
            );
    }

    public int GetNextHeroAttackSpeedPurchasePrice()
    {
        if (progressionConfig == null)
        {
            return 0;
        }

        return progressionConfig
            .GetHeroAttackSpeedPurchasePrice(
                GetPurchaseCount(
                    PermanentUpgradeType.HeroAttackSpeed
                )
            );
    }

    public int GetSharedMaxHealthPurchaseLimit()
    {
        if (progressionConfig == null)
        {
            return 0;
        }

        return progressionConfig
            .GetSharedMaxHealthPurchaseLimit(
                HeroLevel
            );
    }

    public int GetNextSharedMaxHealthPurchasePrice()
    {
        if (progressionConfig == null)
        {
            return 0;
        }

        return progressionConfig
            .GetSharedMaxHealthPurchasePrice(
                GetPurchaseCount(
                    PermanentUpgradeType.SharedMaxHealth
                )
            );
    }

    public bool IsFirstVictoryRewardClaimed(int mapId)
    {
        return progress != null &&
            progress.IsFirstVictoryRewardClaimed(mapId);
    }

    public bool AddGold(int amount)
    {
        if (progress == null ||
            !progress.TryAddGold(amount))
        {
            return false;
        }

        return SaveAfterChange(
            $"Added {amount} gold. " +
            $"Total gold = {progress.Gold}."
        );
    }

    public bool TrySpendGold(int amount)
    {
        if (progress == null ||
            !progress.TrySpendGold(amount))
        {
            return false;
        }

        return SaveAfterChange(
            $"Spent {amount} gold. " +
            $"Remaining gold = {progress.Gold}."
        );
    }

    public bool TryPurchaseHeroDamage()
    {
        if (progress == null)
        {
            Debug.LogError(
                "PlayerProgression: Progress is not loaded.",
                this
            );

            return false;
        }

        if (progressionConfig == null)
        {
            Debug.LogError(
                "PlayerProgression: Assign PermanentProgressionConfig.",
                this
            );

            return false;
        }

        int currentPurchases = GetPurchaseCount(
            PermanentUpgradeType.HeroDamage
        );

        int purchaseLimit =
            GetHeroDamagePurchaseLimit();

        if (currentPurchases >= purchaseLimit)
        {
            Debug.Log(
                "PlayerProgression: Hero damage branch " +
                $"is complete for hero level {HeroLevel}. " +
                $"Purchases: {currentPurchases}/" +
                $"{purchaseLimit}.",
                this
            );

            return false;
        }

        int price =
            GetNextHeroDamagePurchasePrice();

        if (Gold < price)
        {
            Debug.Log(
                "PlayerProgression: Not enough gold for " +
                $"hero damage. Price = {price}, " +
                $"gold = {Gold}.",
                this
            );

            return false;
        }

        int levelBeforePurchase = HeroLevel;

        if (!progress.TryPurchaseUpgrade(
                PermanentUpgradeType.HeroDamage,
                price
            ))
        {
            Debug.LogError(
                "PlayerProgression: Hero damage purchase " +
                "was rejected after its conditions " +
                "were checked.",
                this
            );

            return false;
        }

        TryAdvanceHeroLevel();

        int newPurchaseCount = GetPurchaseCount(
            PermanentUpgradeType.HeroDamage
        );

        string levelMessage =
            HeroLevel > levelBeforePurchase
                ? $" Hero level increased to {HeroLevel}."
                : string.Empty;

        return SaveAfterChange(
            $"Hero damage purchase #{newPurchaseCount} " +
            $"completed. Price = {price}. " +
            $"Remaining gold = {Gold}. " +
            $"Permanent damage bonus = " +
            $"{HeroPermanentDamageBonus:F2}." +
            levelMessage
        );
    }

    public bool TryPurchaseHeroCriticalChance()
    {
        if (progress == null)
        {
            Debug.LogError(
                "PlayerProgression: Progress is not loaded.",
                this
            );

            return false;
        }

        if (progressionConfig == null)
        {
            Debug.LogError(
                "PlayerProgression: Assign PermanentProgressionConfig.",
                this
            );

            return false;
        }

        if (!IsHeroCriticalChanceUnlocked())
        {
            Debug.Log(
                "PlayerProgression: Hero critical chance " +
                "unlocks at hero level 2.",
                this
            );

            return false;
        }

        int currentPurchases = GetPurchaseCount(
            PermanentUpgradeType.HeroCriticalChance
        );

        int purchaseLimit =
            GetHeroCriticalChancePurchaseLimit();

        if (currentPurchases >= purchaseLimit)
        {
            Debug.Log(
                "PlayerProgression: Hero critical chance " +
                $"branch is complete for hero level " +
                $"{HeroLevel}. Purchases: " +
                $"{currentPurchases}/{purchaseLimit}.",
                this
            );

            return false;
        }

        int price =
            GetNextHeroCriticalChancePurchasePrice();

        if (Gold < price)
        {
            Debug.Log(
                "PlayerProgression: Not enough gold for " +
                $"hero critical chance. Price = {price}, " +
                $"gold = {Gold}.",
                this
            );

            return false;
        }

        int levelBeforePurchase = HeroLevel;

        if (!progress.TryPurchaseUpgrade(
                PermanentUpgradeType.HeroCriticalChance,
                price
            ))
        {
            Debug.LogError(
                "PlayerProgression: Hero critical chance " +
                "purchase was rejected after its " +
                "conditions were checked.",
                this
            );

            return false;
        }

        TryAdvanceHeroLevel();

        int newPurchaseCount = GetPurchaseCount(
            PermanentUpgradeType.HeroCriticalChance
        );

        string levelMessage =
            HeroLevel > levelBeforePurchase
                ? $" Hero level increased to {HeroLevel}."
                : string.Empty;

        return SaveAfterChange(
            $"Hero critical chance purchase " +
            $"#{newPurchaseCount} completed. " +
            $"Price = {price}. Remaining gold = {Gold}. " +
            $"Permanent critical chance bonus = " +
            $"{HeroPermanentCriticalChanceBonusPoints:F2} " +
            $"percentage point(s)." +
            levelMessage
        );
    }

    public bool TryPurchaseHeroAttackSpeed()
    {
        if (progress == null)
        {
            Debug.LogError(
                "PlayerProgression: Progress is not loaded.",
                this
            );

            return false;
        }

        if (progressionConfig == null)
        {
            Debug.LogError(
                "PlayerProgression: Assign PermanentProgressionConfig.",
                this
            );

            return false;
        }

        if (!IsHeroAttackSpeedUnlocked())
        {
            Debug.Log(
                "PlayerProgression: Hero attack speed " +
                "unlocks at hero level 5.",
                this
            );

            return false;
        }

        int currentPurchases = GetPurchaseCount(
            PermanentUpgradeType.HeroAttackSpeed
        );

        int purchaseLimit =
            GetHeroAttackSpeedPurchaseLimit();

        if (currentPurchases >= purchaseLimit)
        {
            Debug.Log(
                "PlayerProgression: Hero attack speed " +
                $"branch is complete for hero level " +
                $"{HeroLevel}. Purchases: " +
                $"{currentPurchases}/{purchaseLimit}.",
                this
            );

            return false;
        }

        int price =
            GetNextHeroAttackSpeedPurchasePrice();

        if (Gold < price)
        {
            Debug.Log(
                "PlayerProgression: Not enough gold for " +
                $"hero attack speed. Price = {price}, " +
                $"gold = {Gold}.",
                this
            );

            return false;
        }

        int levelBeforePurchase = HeroLevel;

        if (!progress.TryPurchaseUpgrade(
                PermanentUpgradeType.HeroAttackSpeed,
                price
            ))
        {
            Debug.LogError(
                "PlayerProgression: Hero attack speed " +
                "purchase was rejected after its " +
                "conditions were checked.",
                this
            );

            return false;
        }

        TryAdvanceHeroLevel();

        int newPurchaseCount = GetPurchaseCount(
            PermanentUpgradeType.HeroAttackSpeed
        );

        string levelMessage =
            HeroLevel > levelBeforePurchase
                ? $" Hero level increased to {HeroLevel}."
                : string.Empty;

        return SaveAfterChange(
            $"Hero attack speed purchase " +
            $"#{newPurchaseCount} completed. " +
            $"Price = {price}. Remaining gold = {Gold}. " +
            $"Permanent attack speed bonus = " +
            $"{HeroPermanentAttackSpeedBonusPercent:F2}%." +
            levelMessage
        );
    }

    public bool TryPurchaseSharedMaxHealth()
    {
        if (progress == null)
        {
            Debug.LogError(
                "PlayerProgression: Progress is not loaded.",
                this
            );

            return false;
        }

        if (progressionConfig == null)
        {
            Debug.LogError(
                "PlayerProgression: Assign PermanentProgressionConfig.",
                this
            );

            return false;
        }

        int currentPurchases = GetPurchaseCount(
            PermanentUpgradeType.SharedMaxHealth
        );

        int purchaseLimit =
            GetSharedMaxHealthPurchaseLimit();

        if (currentPurchases >= purchaseLimit)
        {
            Debug.Log(
                "PlayerProgression: Shared maximum health " +
                $"branch is complete for hero level " +
                $"{HeroLevel}. Purchases: " +
                $"{currentPurchases}/{purchaseLimit}.",
                this
            );

            return false;
        }

        int price =
            GetNextSharedMaxHealthPurchasePrice();

        if (Gold < price)
        {
            Debug.Log(
                "PlayerProgression: Not enough gold for " +
                $"shared maximum health. Price = {price}, " +
                $"gold = {Gold}.",
                this
            );

            return false;
        }

        if (!progress.TryPurchaseUpgrade(
                PermanentUpgradeType.SharedMaxHealth,
                price
            ))
        {
            Debug.LogError(
                "PlayerProgression: Shared maximum health " +
                "purchase was rejected after its conditions " +
                "were checked.",
                this
            );

            return false;
        }

        int newPurchaseCount = GetPurchaseCount(
            PermanentUpgradeType.SharedMaxHealth
        );

        return SaveAfterChange(
            $"Shared maximum health purchase " +
            $"#{newPurchaseCount} completed. " +
            $"Price = {price}. Remaining gold = {Gold}. " +
            $"Permanent maximum health bonus for Hero, " +
            $"CoreTower and defense towers = " +
            $"{SharedPermanentMaxHealthBonusPercent:F2}%."
        );
    }

    public bool TryClaimFirstVictoryReward(
        int mapId,
        int goldReward
    )
    {
        if (progress == null ||
            !progress.TryClaimFirstVictoryReward(
                mapId,
                goldReward
            ))
        {
            Debug.Log(
                "PlayerProgression: First victory reward " +
                $"for map {mapId} was not claimed. " +
                "Check the map ID, reward, or previous claim.",
                this
            );

            return false;
        }

        return SaveAfterChange(
            $"First victory reward claimed for map {mapId}. " +
            $"Gold reward = {goldReward}. " +
            $"Total gold = {progress.Gold}."
        );
    }

    private void TryAdvanceHeroLevel()
    {
        if (progress == null || progressionConfig == null)
        {
            return;
        }

        while (AreRequirementsMetForNextLevel())
        {
            if (!progress.TryIncreaseHeroLevel())
            {
                break;
            }
        }
    }

    private bool AreRequirementsMetForNextLevel()
    {
        int currentLevel = progress.HeroLevel;

        int requiredDamage =
            progressionConfig
                .GetRequiredHeroDamagePurchases(
                    currentLevel
                );

        int requiredCriticalChance =
            progressionConfig
                .GetRequiredHeroCriticalChancePurchases(
                    currentLevel
                );

        int requiredAttackSpeed =
            progressionConfig
                .GetRequiredHeroAttackSpeedPurchases(
                    currentLevel
                );

        return
            progress.GetPurchaseCount(
                PermanentUpgradeType.HeroDamage
            ) >= requiredDamage
            &&
            progress.GetPurchaseCount(
                PermanentUpgradeType.HeroCriticalChance
            ) >= requiredCriticalChance
            &&
            progress.GetPurchaseCount(
                PermanentUpgradeType.HeroAttackSpeed
            ) >= requiredAttackSpeed;
    }

    private bool SaveAfterChange(string successMessage)
    {
        bool saveSucceeded =
            PlayerProgressSaveSystem.Save(progress);

        RefreshDebugInfo();

        if (!saveSucceeded)
        {
            Debug.LogError(
                "PlayerProgression: Progress changed in " +
                "memory, but saving failed.",
                this
            );

            return false;
        }

        Debug.Log(
            "PlayerProgression: " + successMessage,
            this
        );

        ProgressChanged?.Invoke();

        return true;
    }

    private void RefreshDebugInfo()
    {
        debugSaveFilePath =
            PlayerProgressSaveSystem.SaveFilePath;

        if (progress == null)
        {
            return;
        }

        debugGold = progress.Gold;
        debugHeroLevel = progress.HeroLevel;

        debugHeroDamagePurchases =
            progress.GetPurchaseCount(
                PermanentUpgradeType.HeroDamage
            );

        debugHeroDamagePurchaseLimit =
            GetHeroDamagePurchaseLimit();

        debugNextHeroDamagePrice =
            GetNextHeroDamagePurchasePrice();

        debugHeroPermanentDamageBonus =
            HeroPermanentDamageBonus;

        debugHeroCriticalChancePurchases =
            progress.GetPurchaseCount(
                PermanentUpgradeType.HeroCriticalChance
            );

        debugHeroCriticalChancePurchaseLimit =
            GetHeroCriticalChancePurchaseLimit();

        debugNextHeroCriticalChancePrice =
            GetNextHeroCriticalChancePurchasePrice();

        debugHeroPermanentCriticalChanceBonusPoints =
            HeroPermanentCriticalChanceBonusPoints;

        debugHeroAttackSpeedPurchases =
            progress.GetPurchaseCount(
                PermanentUpgradeType.HeroAttackSpeed
            );

        debugHeroAttackSpeedPurchaseLimit =
            GetHeroAttackSpeedPurchaseLimit();

        debugNextHeroAttackSpeedPrice =
            GetNextHeroAttackSpeedPurchasePrice();

        debugHeroPermanentAttackSpeedBonusPercent =
            HeroPermanentAttackSpeedBonusPercent;

        debugSharedMaxHealthPurchases =
            progress.GetPurchaseCount(
                PermanentUpgradeType.SharedMaxHealth
            );

        debugSharedMaxHealthPurchaseLimit =
            GetSharedMaxHealthPurchaseLimit();

        debugNextSharedMaxHealthPrice =
            GetNextSharedMaxHealthPurchasePrice();

        debugSharedMaxHealthBonusPercent =
            SharedPermanentMaxHealthBonusPercent;

        debugMapOneRewardClaimed =
            progress.IsFirstVictoryRewardClaimed(1);
    }

    [ContextMenu("Test Progress/Add 500 Gold")]
    private void TestAdd500Gold()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        AddGold(500);
    }

    [ContextMenu("Test Progress/Add 20000 Gold")]
    private void TestAdd20000Gold()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        AddGold(20000);
    }

    [ContextMenu("Test Progress/Add 100000 Gold")]
    private void TestAdd100000Gold()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        AddGold(100000);
    }

    [ContextMenu("Test Progress/Spend 200 Gold")]
    private void TestSpend200Gold()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        bool spendSucceeded =
            TrySpendGold(200);

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
        {
            return;
        }

        progress.SetHeroLevelForTesting(3);

        SaveAfterChange(
            "Hero level set to 3 for testing."
        );
    }

    [ContextMenu(
        "Test Progress/Add One Purchase To Every Stat"
    )]
    private void TestAddOnePurchaseToEveryStat()
    {
        if (!Application.isPlaying || progress == null)
        {
            return;
        }

        foreach (
            PermanentUpgradeType upgradeType
            in Enum.GetValues(
                typeof(PermanentUpgradeType)
            )
        )
        {
            progress.TryAddPurchaseForTesting(
                upgradeType
            );
        }

        SaveAfterChange(
            "Added one test purchase to every " +
            "permanent stat."
        );
    }

    [ContextMenu(
        "Test Progress/Claim Map 1 First Reward (+100 Gold)"
    )]
    private void TestClaimMapOneFirstReward()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TryClaimFirstVictoryReward(1, 100);
    }

    [ContextMenu(
        "Test Purchases/Buy Hero Damage Once"
    )]
    private void TestBuyHeroDamageOnce()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TryPurchaseHeroDamage();
    }

    [ContextMenu(
        "Test Purchases/Buy Hero Critical Chance Once"
    )]
    private void TestBuyHeroCriticalChanceOnce()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TryPurchaseHeroCriticalChance();
    }

    [ContextMenu(
        "Test Purchases/Buy Hero Attack Speed Once"
    )]
    private void TestBuyHeroAttackSpeedOnce()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TryPurchaseHeroAttackSpeed();
    }

    [ContextMenu(
        "Test Purchases/Buy Shared Max Health Once"
    )]
    private void TestBuySharedMaxHealthOnce()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TryPurchaseSharedMaxHealth();
    }

    [ContextMenu(
        "Test Purchases/Complete Current Hero Damage Branch"
    )]
    private void TestCompleteCurrentHeroDamageBranch()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        int targetPurchaseCount =
            GetHeroDamagePurchaseLimit();

        while (
            GetPurchaseCount(
                PermanentUpgradeType.HeroDamage
            ) < targetPurchaseCount
        )
        {
            if (!TryPurchaseHeroDamage())
            {
                break;
            }
        }
    }

    [ContextMenu(
        "Test Purchases/Complete Current Hero Critical Chance Branch"
    )]
    private void TestCompleteCurrentHeroCriticalChanceBranch()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (!IsHeroCriticalChanceUnlocked())
        {
            TryPurchaseHeroCriticalChance();
            return;
        }

        int targetPurchaseCount =
            GetHeroCriticalChancePurchaseLimit();

        while (
            GetPurchaseCount(
                PermanentUpgradeType.HeroCriticalChance
            ) < targetPurchaseCount
        )
        {
            if (!TryPurchaseHeroCriticalChance())
            {
                break;
            }
        }
    }

    [ContextMenu(
        "Test Purchases/Complete Current Hero Attack Speed Branch"
    )]
    private void TestCompleteCurrentHeroAttackSpeedBranch()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (!IsHeroAttackSpeedUnlocked())
        {
            TryPurchaseHeroAttackSpeed();
            return;
        }

        int targetPurchaseCount =
            GetHeroAttackSpeedPurchaseLimit();

        while (
            GetPurchaseCount(
                PermanentUpgradeType.HeroAttackSpeed
            ) < targetPurchaseCount
        )
        {
            if (!TryPurchaseHeroAttackSpeed())
            {
                break;
            }
        }
    }

    [ContextMenu(
        "Test Purchases/Complete Current Shared Max Health Branch"
    )]
    private void TestCompleteCurrentSharedMaxHealthBranch()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        int targetPurchaseCount =
            GetSharedMaxHealthPurchaseLimit();

        while (
            GetPurchaseCount(
                PermanentUpgradeType.SharedMaxHealth
            ) < targetPurchaseCount
        )
        {
            if (!TryPurchaseSharedMaxHealth())
            {
                break;
            }
        }
    }

    [ContextMenu(
        "Test Purchases/Print Hero Damage Purchase Report"
    )]
    private void TestPrintHeroDamagePurchaseReport()
    {
        if (!Application.isPlaying || progress == null)
        {
            return;
        }

        Debug.Log(
            $"PlayerProgression | " +
            $"Hero level: {HeroLevel} | " +
            $"Gold: {Gold} | " +
            $"Damage purchases: " +
            $"{GetPurchaseCount(PermanentUpgradeType.HeroDamage)}/" +
            $"{GetHeroDamagePurchaseLimit()} | " +
            $"Next price: " +
            $"{GetNextHeroDamagePurchasePrice()} | " +
            $"Permanent damage bonus: " +
            $"{HeroPermanentDamageBonus:F2}",
            this
        );
    }

    [ContextMenu(
        "Test Purchases/Print Hero Critical Chance Purchase Report"
    )]
    private void TestPrintHeroCriticalChancePurchaseReport()
    {
        if (!Application.isPlaying || progress == null)
        {
            return;
        }

        Debug.Log(
            $"PlayerProgression | " +
            $"Hero level: {HeroLevel} | " +
            $"Gold: {Gold} | " +
            $"Critical chance unlocked: " +
            $"{IsHeroCriticalChanceUnlocked()} | " +
            $"Critical chance purchases: " +
            $"{GetPurchaseCount(PermanentUpgradeType.HeroCriticalChance)}/" +
            $"{GetHeroCriticalChancePurchaseLimit()} | " +
            $"Next price: " +
            $"{GetNextHeroCriticalChancePurchasePrice()} | " +
            $"Permanent critical chance bonus: " +
            $"{HeroPermanentCriticalChanceBonusPoints:F2} " +
            $"percentage point(s)",
            this
        );
    }

    [ContextMenu(
        "Test Purchases/Print Hero Attack Speed Purchase Report"
    )]
    private void TestPrintHeroAttackSpeedPurchaseReport()
    {
        if (!Application.isPlaying || progress == null)
        {
            return;
        }

        Debug.Log(
            $"PlayerProgression | " +
            $"Hero level: {HeroLevel} | " +
            $"Gold: {Gold} | " +
            $"Attack speed unlocked: " +
            $"{IsHeroAttackSpeedUnlocked()} | " +
            $"Attack speed purchases: " +
            $"{GetPurchaseCount(PermanentUpgradeType.HeroAttackSpeed)}/" +
            $"{GetHeroAttackSpeedPurchaseLimit()} | " +
            $"Next price: " +
            $"{GetNextHeroAttackSpeedPurchasePrice()} | " +
            $"Permanent attack speed bonus: " +
            $"{HeroPermanentAttackSpeedBonusPercent:F2}%",
            this
        );
    }

    [ContextMenu(
        "Test Purchases/Print Shared Max Health Purchase Report"
    )]
    private void TestPrintSharedMaxHealthPurchaseReport()
    {
        if (!Application.isPlaying || progress == null)
        {
            return;
        }

        Debug.Log(
            $"PlayerProgression | " +
            $"Hero level: {HeroLevel} | " +
            $"Gold: {Gold} | " +
            $"Shared max health purchases: " +
            $"{GetPurchaseCount(PermanentUpgradeType.SharedMaxHealth)}/" +
            $"{GetSharedMaxHealthPurchaseLimit()} | " +
            $"Next price: " +
            $"{GetNextSharedMaxHealthPurchasePrice()} | " +
            $"Permanent max health bonus for all " +
            $"allied targets: " +
            $"{SharedPermanentMaxHealthBonusPercent:F2}%",
            this
        );
    }

    [ContextMenu("Test Progress/Print Progress")]
    private void TestPrintProgress()
    {
        if (!Application.isPlaying || progress == null)
        {
            return;
        }

        Debug.Log(
            $"PlayerProgression | " +
            $"Gold: {progress.Gold} | " +
            $"Hero level: {progress.HeroLevel} | " +
            $"Damage: " +
            $"{progress.GetPurchaseCount(PermanentUpgradeType.HeroDamage)} | " +
            $"Crit: " +
            $"{progress.GetPurchaseCount(PermanentUpgradeType.HeroCriticalChance)} | " +
            $"Attack speed: " +
            $"{progress.GetPurchaseCount(PermanentUpgradeType.HeroAttackSpeed)} | " +
            $"Shared max health: " +
            $"{progress.GetPurchaseCount(PermanentUpgradeType.SharedMaxHealth)} | " +
            $"Map 1 reward claimed: " +
            $"{progress.IsFirstVictoryRewardClaimed(1)}",
            this
        );
    }

    [ContextMenu(
        "Test Progress/Reset Save To Defaults"
    )]
    private void TestResetSaveToDefaults()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (!PlayerProgressSaveSystem.DeleteSaveFile())
        {
            return;
        }

        progress = new PlayerProgressData();
        progress.Validate();

        SaveAfterChange(
            "Save reset to default progress."
        );
    }
}