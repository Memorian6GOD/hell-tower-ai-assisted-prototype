using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PermanentProgressionConfig",
    menuName = "Hell Tower/Permanent Progression Config"
)]
public class PermanentProgressionConfig : ScriptableObject
{
    [Header("Level Progression")]
    [SerializeField, Min(1)]
    private int purchasesPerRequiredBranch = 10;

    [Header("Hero Damage")]
    [SerializeField, Min(0.01f)]
    private float heroDamagePerPurchase = 2f;

    [SerializeField, Min(1)]
    private int heroDamageStartingPrice = 100;

    [SerializeField, Min(0)]
    private int heroDamagePriceIncreasePerPurchase = 75;

    [Header("Hero Critical Chance")]
    [SerializeField, Min(0.01f)]
    private float heroCriticalChancePointsPerPurchase = 0.5f;

    [SerializeField, Min(1)]
    private int heroCriticalChanceStartingPrice = 100;

    [SerializeField, Min(0)]
    private int heroCriticalChancePriceIncreasePerPurchase = 75;

    [Header("Hero Attack Speed")]
    [SerializeField, Min(0.01f)]
    private float heroAttackSpeedPercentPerPurchase = 2f;

    [SerializeField, Min(1)]
    private int heroAttackSpeedStartingPrice = 100;

    [SerializeField, Min(0)]
    private int heroAttackSpeedPriceIncreasePerPurchase = 75;

    [Header("Shared Maximum Health")]
    [SerializeField, Min(1)]
    private int sharedMaxHealthPurchasesPerHeroLevel = 5;

    [SerializeField, Min(0.01f)]
    private float sharedMaxHealthPercentPerPurchase = 5f;

    [SerializeField, Min(1)]
    private int sharedMaxHealthStartingPrice = 100;

    [SerializeField, Min(0)]
    private int sharedMaxHealthPriceIncreasePerPurchase = 75;

    public int PurchasesPerRequiredBranch =>
        Mathf.Max(1, purchasesPerRequiredBranch);

    public float HeroDamagePerPurchase =>
        Mathf.Max(0.01f, heroDamagePerPurchase);

    public float HeroCriticalChancePointsPerPurchase =>
        Mathf.Max(0.01f, heroCriticalChancePointsPerPurchase);

    public float HeroAttackSpeedPercentPerPurchase =>
        Mathf.Max(0.01f, heroAttackSpeedPercentPerPurchase);

    public int SharedMaxHealthPurchasesPerHeroLevel =>
        Mathf.Max(1, sharedMaxHealthPurchasesPerHeroLevel);

    public float SharedMaxHealthPercentPerPurchase =>
        Mathf.Max(0.01f, sharedMaxHealthPercentPerPurchase);

    public int GetHeroDamagePurchasePrice(int currentPurchaseCount)
    {
        int safePurchaseCount = Mathf.Max(0, currentPurchaseCount);

        long price =
            (long)Mathf.Max(1, heroDamageStartingPrice)
            + (long)Mathf.Max(
                0,
                heroDamagePriceIncreasePerPurchase
            ) * safePurchaseCount;

        return (int)Math.Min(int.MaxValue, price);
    }

    public int GetHeroCriticalChancePurchasePrice(
        int currentPurchaseCount
    )
    {
        int safePurchaseCount = Mathf.Max(0, currentPurchaseCount);

        long price =
            (long)Mathf.Max(
                1,
                heroCriticalChanceStartingPrice
            )
            + (long)Mathf.Max(
                0,
                heroCriticalChancePriceIncreasePerPurchase
            ) * safePurchaseCount;

        return (int)Math.Min(int.MaxValue, price);
    }

    public int GetHeroAttackSpeedPurchasePrice(
        int currentPurchaseCount
    )
    {
        int safePurchaseCount = Mathf.Max(0, currentPurchaseCount);

        long price =
            (long)Mathf.Max(1, heroAttackSpeedStartingPrice)
            + (long)Mathf.Max(
                0,
                heroAttackSpeedPriceIncreasePerPurchase
            ) * safePurchaseCount;

        return (int)Math.Min(int.MaxValue, price);
    }

    public int GetSharedMaxHealthPurchasePrice(
        int currentPurchaseCount
    )
    {
        int safePurchaseCount = Mathf.Max(0, currentPurchaseCount);

        long price =
            (long)Mathf.Max(1, sharedMaxHealthStartingPrice)
            + (long)Mathf.Max(
                0,
                sharedMaxHealthPriceIncreasePerPurchase
            ) * safePurchaseCount;

        return (int)Math.Min(int.MaxValue, price);
    }

    public int GetRequiredHeroDamagePurchases(
        int currentHeroLevel
    )
    {
        int safeHeroLevel = Mathf.Max(1, currentHeroLevel);

        return MultiplyAndClampToIntMax(
            safeHeroLevel,
            PurchasesPerRequiredBranch
        );
    }

    public int GetRequiredHeroCriticalChancePurchases(
        int currentHeroLevel
    )
    {
        int safeHeroLevel = Mathf.Max(1, currentHeroLevel);

        if (safeHeroLevel < 2)
        {
            return 0;
        }

        return MultiplyAndClampToIntMax(
            safeHeroLevel - 1,
            PurchasesPerRequiredBranch
        );
    }

    public int GetRequiredHeroAttackSpeedPurchases(
        int currentHeroLevel
    )
    {
        int safeHeroLevel = Mathf.Max(1, currentHeroLevel);

        if (safeHeroLevel < 5)
        {
            return 0;
        }

        return MultiplyAndClampToIntMax(
            safeHeroLevel - 4,
            PurchasesPerRequiredBranch
        );
    }

    public int GetSharedMaxHealthPurchaseLimit(
        int currentHeroLevel
    )
    {
        int safeHeroLevel = Mathf.Max(1, currentHeroLevel);

        return MultiplyAndClampToIntMax(
            safeHeroLevel,
            SharedMaxHealthPurchasesPerHeroLevel
        );
    }

    public float GetTotalHeroDamageBonus(int purchaseCount)
    {
        int safePurchaseCount = Mathf.Max(0, purchaseCount);

        double bonus =
            safePurchaseCount
            * (double)HeroDamagePerPurchase;

        return (float)Math.Min(float.MaxValue, bonus);
    }

    public float GetTotalHeroCriticalChanceBonusPoints(
        int purchaseCount
    )
    {
        int safePurchaseCount = Mathf.Max(0, purchaseCount);

        double bonus =
            safePurchaseCount
            * (double)HeroCriticalChancePointsPerPurchase;

        return (float)Math.Min(float.MaxValue, bonus);
    }

    public float GetTotalHeroAttackSpeedBonusPercent(
        int purchaseCount
    )
    {
        int safePurchaseCount = Mathf.Max(0, purchaseCount);

        double bonus =
            safePurchaseCount
            * (double)HeroAttackSpeedPercentPerPurchase;

        return (float)Math.Min(float.MaxValue, bonus);
    }

    public float GetTotalSharedMaxHealthBonusPercent(
        int purchaseCount
    )
    {
        int safePurchaseCount = Mathf.Max(0, purchaseCount);

        double bonus =
            safePurchaseCount
            * (double)SharedMaxHealthPercentPerPurchase;

        return (float)Math.Min(float.MaxValue, bonus);
    }

    private static int MultiplyAndClampToIntMax(
        int firstValue,
        int secondValue
    )
    {
        long result = (long)firstValue * secondValue;
        return (int)Math.Min(int.MaxValue, result);
    }
}