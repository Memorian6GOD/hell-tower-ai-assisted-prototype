using UnityEngine;

[CreateAssetMenu(
    fileName = "PermanentProgressionConfig",
    menuName = "Hell Tower/Permanent Progression Config"
)]
public class PermanentProgressionConfig : ScriptableObject
{
    [Header("Master Balance Source")]

    [Tooltip(
        "Temporary compatibility reference. " +
        "All permanent progression balance values are now " +
        "stored in GameBalanceConfig."
    )]
    [SerializeField]
    private GameBalanceConfig balanceConfig;


    public int PurchasesPerRequiredBranch
    {
        get
        {
            if (balanceConfig == null)
            {
                return 1;
            }

            return balanceConfig
                .PurchasesPerRequiredBranch;
        }
    }

    public float HeroDamagePerPurchase
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return
                balanceConfig.HeroBaseDamage *
                balanceConfig.HeroDamagePercentPerPurchase /
                100f;
        }
    }

    public float HeroCriticalChancePointsPerPurchase
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig
                .HeroCriticalChancePointsPerPurchase;
        }
    }

    public float HeroAttackSpeedPercentPerPurchase
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig
                .HeroAttackSpeedPercentPerPurchase;
        }
    }

    public int SharedMaxHealthPurchasesPerHeroLevel
    {
        get
        {
            if (balanceConfig == null)
            {
                return 1;
            }

            return balanceConfig
                .SharedMaxHealthPurchasesPerHeroLevel;
        }
    }

    public float SharedMaxHealthPercentPerPurchase
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig
                .SharedMaxHealthPercentPerPurchase;
        }
    }


    public int GetHeroDamagePurchasePrice(
        int currentPurchaseCount
    )
    {
        if (balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetHeroDamagePurchasePrice(
                currentPurchaseCount
            );
    }

    public int GetHeroCriticalChancePurchasePrice(
        int currentPurchaseCount
    )
    {
        if (balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetHeroCriticalChancePurchasePrice(
                currentPurchaseCount
            );
    }

    public int GetHeroAttackSpeedPurchasePrice(
        int currentPurchaseCount
    )
    {
        if (balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetHeroAttackSpeedPurchasePrice(
                currentPurchaseCount
            );
    }

    public int GetSharedMaxHealthPurchasePrice(
        int currentPurchaseCount
    )
    {
        if (balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetSharedMaxHealthPurchasePrice(
                currentPurchaseCount
            );
    }


    public int GetRequiredHeroDamagePurchases(
        int currentHeroLevel
    )
    {
        if (balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetRequiredHeroDamagePurchases(
                currentHeroLevel
            );
    }

    public int GetRequiredHeroCriticalChancePurchases(
        int currentHeroLevel
    )
    {
        if (balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetRequiredHeroCriticalChancePurchases(
                currentHeroLevel
            );
    }

    public int GetRequiredHeroAttackSpeedPurchases(
        int currentHeroLevel
    )
    {
        if (balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetRequiredHeroAttackSpeedPurchases(
                currentHeroLevel
            );
    }

    public int GetSharedMaxHealthPurchaseLimit(
        int currentHeroLevel
    )
    {
        if (balanceConfig == null)
        {
            return 0;
        }

        return balanceConfig
            .GetSharedMaxHealthPurchaseLimit(
                currentHeroLevel
            );
    }


    public float GetTotalHeroDamageBonus(
        int purchaseCount
    )
    {
        if (balanceConfig == null)
        {
            return 0f;
        }

        return balanceConfig
            .GetTotalHeroDamageBonus(
                purchaseCount
            );
    }

    public float GetTotalHeroCriticalChanceBonusPoints(
        int purchaseCount
    )
    {
        if (balanceConfig == null)
        {
            return 0f;
        }

        return balanceConfig
            .GetTotalHeroCriticalChanceBonusPoints(
                purchaseCount
            );
    }

    public float GetTotalHeroAttackSpeedBonusPercent(
        int purchaseCount
    )
    {
        if (balanceConfig == null)
        {
            return 0f;
        }

        return balanceConfig
            .GetTotalHeroAttackSpeedBonusPercent(
                purchaseCount
            );
    }

    public float GetTotalSharedMaxHealthBonusPercent(
        int purchaseCount
    )
    {
        if (balanceConfig == null)
        {
            return 0f;
        }

        return balanceConfig
            .GetTotalSharedMaxHealthBonusPercent(
                purchaseCount
            );
    }
}