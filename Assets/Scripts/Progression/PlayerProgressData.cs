using System;
using System.Collections.Generic;
using UnityEngine;

public enum PermanentUpgradeType
{
    HeroDamage,
    HeroCriticalChance,
    HeroAttackSpeed,
    HeroMaxHealth,
    CoreTowerMaxHealth,
    DefenseTowerMaxHealth
}

[Serializable]
public class PlayerProgressData
{
    public const int CurrentSaveVersion = 1;

    [SerializeField] private int saveVersion = CurrentSaveVersion;
    [SerializeField] private int gold;
    [SerializeField] private int heroLevel = 1;

    [Header("Permanent Upgrade Purchase Counts")]
    [SerializeField] private int heroDamagePurchases;
    [SerializeField] private int heroCriticalChancePurchases;
    [SerializeField] private int heroAttackSpeedPurchases;
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
        saveVersion = CurrentSaveVersion;
        gold = Mathf.Max(0, gold);
        heroLevel = Mathf.Max(1, heroLevel);

        heroDamagePurchases = Mathf.Max(0, heroDamagePurchases);
        heroCriticalChancePurchases =
            Mathf.Max(0, heroCriticalChancePurchases);
        heroAttackSpeedPurchases =
            Mathf.Max(0, heroAttackSpeedPurchases);
        heroMaxHealthPurchases =
            Mathf.Max(0, heroMaxHealthPurchases);
        coreTowerMaxHealthPurchases =
            Mathf.Max(0, coreTowerMaxHealthPurchases);
        defenseTowerMaxHealthPurchases =
            Mathf.Max(0, defenseTowerMaxHealthPurchases);

        RemoveInvalidAndDuplicateMapIds();
    }

    public int GetPurchaseCount(PermanentUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case PermanentUpgradeType.HeroDamage:
                return heroDamagePurchases;

            case PermanentUpgradeType.HeroCriticalChance:
                return heroCriticalChancePurchases;

            case PermanentUpgradeType.HeroAttackSpeed:
                return heroAttackSpeedPurchases;

            case PermanentUpgradeType.HeroMaxHealth:
                return heroMaxHealthPurchases;

            case PermanentUpgradeType.CoreTowerMaxHealth:
                return coreTowerMaxHealthPurchases;

            case PermanentUpgradeType.DefenseTowerMaxHealth:
                return defenseTowerMaxHealthPurchases;

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

    internal bool TryClaimFirstVictoryReward(
        int mapId,
        int goldReward
    )
    {
        if (mapId <= 0 || goldReward < 0 ||
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

            case PermanentUpgradeType.HeroMaxHealth:
                heroMaxHealthPurchases++;
                return true;

            case PermanentUpgradeType.CoreTowerMaxHealth:
                coreTowerMaxHealthPurchases++;
                return true;

            case PermanentUpgradeType.DefenseTowerMaxHealth:
                defenseTowerMaxHealthPurchases++;
                return true;

            default:
                return false;
        }
    }

    internal void SetHeroLevelForTesting(int level)
    {
        heroLevel = Mathf.Max(1, level);
    }

    private void RemoveInvalidAndDuplicateMapIds()
    {
        if (mapsWithClaimedFirstVictoryReward == null)
        {
            mapsWithClaimedFirstVictoryReward = new List<int>();
            return;
        }

        List<int> validUniqueMapIds = new();
        HashSet<int> seenMapIds = new();

        for (int i = 0;
             i < mapsWithClaimedFirstVictoryReward.Count;
             i++)
        {
            int mapId = mapsWithClaimedFirstVictoryReward[i];

            if (mapId <= 0 || !seenMapIds.Add(mapId))
            {
                continue;
            }

            validUniqueMapIds.Add(mapId);
        }

        mapsWithClaimedFirstVictoryReward = validUniqueMapIds;
    }
}
