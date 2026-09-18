using System;
using System.Collections.Generic;
using UnityEngine;

public enum PermanentUpgradeType
{
    HeroDamage,
    HeroCriticalChance,
    HeroAttackSpeed,
    SharedMaxHealth
}

[Serializable]
public class PlayerProgressData
{
    public const int CurrentSaveVersion = 2;

    [SerializeField] private int saveVersion = CurrentSaveVersion;
    [SerializeField] private int gold;
    [SerializeField] private int heroLevel = 1;

    [Header("Permanent Upgrade Purchase Counts")]
    [SerializeField] private int heroDamagePurchases;
    [SerializeField] private int heroCriticalChancePurchases;
    [SerializeField] private int heroAttackSpeedPurchases;
    [SerializeField] private int sharedMaxHealthPurchases;

    [Header("Legacy Health Counts - Save Version 1")]
    [SerializeField] private int heroMaxHealthPurchases;
    [SerializeField] private int coreTowerMaxHealthPurchases;
    [SerializeField] private int defenseTowerMaxHealthPurchases;

    [Header("Map Rewards")]
    [SerializeField]
    private List<int> mapsWithClaimedFirstVictoryReward = new();

    public int SaveVersion => saveVersion;
    public int Gold => gold;
    public int HeroLevel => heroLevel;

    public IReadOnlyList<int> MapsWithClaimedFirstVictoryReward =>
        mapsWithClaimedFirstVictoryReward;

    public void Validate()
    {
        gold = Mathf.Max(0, gold);
        heroLevel = Mathf.Max(1, heroLevel);

        heroDamagePurchases = Mathf.Max(0, heroDamagePurchases);

        heroCriticalChancePurchases =
            Mathf.Max(0, heroCriticalChancePurchases);

        heroAttackSpeedPurchases =
            Mathf.Max(0, heroAttackSpeedPurchases);

        sharedMaxHealthPurchases =
            Mathf.Max(0, sharedMaxHealthPurchases);

        heroMaxHealthPurchases =
            Mathf.Max(0, heroMaxHealthPurchases);

        coreTowerMaxHealthPurchases =
            Mathf.Max(0, coreTowerMaxHealthPurchases);

        defenseTowerMaxHealthPurchases =
            Mathf.Max(0, defenseTowerMaxHealthPurchases);

        MigrateLegacyHealthPurchases();
        RemoveInvalidAndDuplicateMapIds();

        saveVersion = CurrentSaveVersion;
    }

    public int GetPurchaseCount(
        PermanentUpgradeType upgradeType
    )
    {
        switch (upgradeType)
        {
            case PermanentUpgradeType.HeroDamage:
                return heroDamagePurchases;

            case PermanentUpgradeType.HeroCriticalChance:
                return heroCriticalChancePurchases;

            case PermanentUpgradeType.HeroAttackSpeed:
                return heroAttackSpeedPurchases;

            case PermanentUpgradeType.SharedMaxHealth:
                return sharedMaxHealthPurchases;

            default:
                return 0;
        }
    }

    public bool IsFirstVictoryRewardClaimed(int mapId)
    {
        return mapId > 0 &&
            mapsWithClaimedFirstVictoryReward.Contains(mapId);
    }

    internal bool TryAddGold(int amount)
    {
        if (amount <= 0 || amount > int.MaxValue - gold)
        {
            return false;
        }

        gold += amount;
        return true;
    }

    internal bool TrySpendGold(int amount)
    {
        if (amount <= 0 || gold < amount)
        {
            return false;
        }

        gold -= amount;
        return true;
    }

    internal bool TryPurchaseUpgrade(
        PermanentUpgradeType upgradeType,
        int price
    )
    {
        if (price <= 0 || gold < price)
        {
            return false;
        }

        int currentCount = GetPurchaseCount(upgradeType);

        if (currentCount >= int.MaxValue)
        {
            return false;
        }

        switch (upgradeType)
        {
            case PermanentUpgradeType.HeroDamage:
                heroDamagePurchases++;
                break;

            case PermanentUpgradeType.HeroCriticalChance:
                heroCriticalChancePurchases++;
                break;

            case PermanentUpgradeType.HeroAttackSpeed:
                heroAttackSpeedPurchases++;
                break;

            case PermanentUpgradeType.SharedMaxHealth:
                sharedMaxHealthPurchases++;
                break;

            default:
                return false;
        }

        gold -= price;
        return true;
    }

    internal bool TryIncreaseHeroLevel()
    {
        if (heroLevel >= int.MaxValue)
        {
            return false;
        }

        heroLevel++;
        return true;
    }

    internal bool TryClaimFirstVictoryReward(
        int mapId,
        int goldReward
    )
    {
        if (mapId <= 0 ||
            goldReward < 0 ||
            IsFirstVictoryRewardClaimed(mapId))
        {
            return false;
        }

        if (goldReward > int.MaxValue - gold)
        {
            return false;
        }

        gold += goldReward;
        mapsWithClaimedFirstVictoryReward.Add(mapId);

        return true;
    }

    internal bool TryAddPurchaseForTesting(
        PermanentUpgradeType upgradeType
    )
    {
        int currentCount = GetPurchaseCount(upgradeType);

        if (currentCount >= int.MaxValue)
        {
            return false;
        }

        switch (upgradeType)
        {
            case PermanentUpgradeType.HeroDamage:
                heroDamagePurchases++;
                return true;

            case PermanentUpgradeType.HeroCriticalChance:
                heroCriticalChancePurchases++;
                return true;

            case PermanentUpgradeType.HeroAttackSpeed:
                heroAttackSpeedPurchases++;
                return true;

            case PermanentUpgradeType.SharedMaxHealth:
                sharedMaxHealthPurchases++;
                return true;

            default:
                return false;
        }
    }

    internal void SetHeroLevelForTesting(int level)
    {
        heroLevel = Mathf.Max(1, level);
    }

    private void MigrateLegacyHealthPurchases()
    {
        if (saveVersion >= 2)
        {
            return;
        }

        int legacyPurchaseCount = Mathf.Max(
            heroMaxHealthPurchases,
            Mathf.Max(
                coreTowerMaxHealthPurchases,
                defenseTowerMaxHealthPurchases
            )
        );

        sharedMaxHealthPurchases = Mathf.Max(
            sharedMaxHealthPurchases,
            legacyPurchaseCount
        );

        heroMaxHealthPurchases = 0;
        coreTowerMaxHealthPurchases = 0;
        defenseTowerMaxHealthPurchases = 0;
    }

    private void RemoveInvalidAndDuplicateMapIds()
    {
        if (mapsWithClaimedFirstVictoryReward == null)
        {
            mapsWithClaimedFirstVictoryReward =
                new List<int>();

            return;
        }

        List<int> validUniqueMapIds = new();
        HashSet<int> seenMapIds = new();

        for (int i = 0;
             i < mapsWithClaimedFirstVictoryReward.Count;
             i++)
        {
            int mapId =
                mapsWithClaimedFirstVictoryReward[i];

            if (mapId <= 0 || !seenMapIds.Add(mapId))
            {
                continue;
            }

            validUniqueMapIds.Add(mapId);
        }

        mapsWithClaimedFirstVictoryReward =
            validUniqueMapIds;
    }
}