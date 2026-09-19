using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ProgressionMenuUI : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField]
    private PlayerProgression playerProgression;

    [Header("Top Bar")]
    [SerializeField] private TMP_Text heroLevelText;
    [SerializeField] private TMP_Text goldText;

    [Header("Damage")]
    [SerializeField] private TMP_Text damageInfoText;
    [SerializeField] private Button damageBuyButton;
    [SerializeField] private TMP_Text damageBuyButtonText;

    [Header("Shared Maximum Health")]
    [SerializeField] private TMP_Text healthInfoText;
    [SerializeField] private Button healthBuyButton;
    [SerializeField] private TMP_Text healthBuyButtonText;

    [Header("Critical Chance")]
    [SerializeField] private TMP_Text criticalChanceInfoText;
    [SerializeField] private Button criticalChanceBuyButton;
    [SerializeField] private TMP_Text criticalChanceBuyButtonText;

    [Header("Attack Speed")]
    [SerializeField] private TMP_Text attackSpeedInfoText;
    [SerializeField] private Button attackSpeedBuyButton;
    [SerializeField] private TMP_Text attackSpeedBuyButtonText;

    private void OnEnable()
    {
        if (!ValidateReferences())
        {
            return;
        }

        damageBuyButton.onClick.AddListener(
            HandleDamagePurchase
        );

        healthBuyButton.onClick.AddListener(
            HandleHealthPurchase
        );

        criticalChanceBuyButton.onClick.AddListener(
            HandleCriticalChancePurchase
        );

        attackSpeedBuyButton.onClick.AddListener(
            HandleAttackSpeedPurchase
        );

        playerProgression.ProgressChanged += RefreshAll;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (damageBuyButton != null)
        {
            damageBuyButton.onClick.RemoveListener(
                HandleDamagePurchase
            );
        }

        if (healthBuyButton != null)
        {
            healthBuyButton.onClick.RemoveListener(
                HandleHealthPurchase
            );
        }

        if (criticalChanceBuyButton != null)
        {
            criticalChanceBuyButton.onClick.RemoveListener(
                HandleCriticalChancePurchase
            );
        }

        if (attackSpeedBuyButton != null)
        {
            attackSpeedBuyButton.onClick.RemoveListener(
                HandleAttackSpeedPurchase
            );
        }

        if (playerProgression != null)
        {
            playerProgression.ProgressChanged -= RefreshAll;
        }
    }

    private void HandleDamagePurchase()
    {
        if (!playerProgression.TryPurchaseHeroDamage())
        {
            RefreshAll();
        }
    }

    private void HandleHealthPurchase()
    {
        if (!playerProgression.TryPurchaseSharedMaxHealth())
        {
            RefreshAll();
        }
    }

    private void HandleCriticalChancePurchase()
    {
        if (!playerProgression.TryPurchaseHeroCriticalChance())
        {
            RefreshAll();
        }
    }

    private void HandleAttackSpeedPurchase()
    {
        if (!playerProgression.TryPurchaseHeroAttackSpeed())
        {
            RefreshAll();
        }
    }

    private void RefreshAll()
    {
        if (playerProgression == null)
        {
            return;
        }

        heroLevelText.text =
            $"LEVEL {FormatInteger(playerProgression.HeroLevel)}";

        goldText.text =
            $"GOLD {FormatInteger(playerProgression.Gold)}";

        RefreshDamage();
        RefreshHealth();
        RefreshCriticalChance();
        RefreshAttackSpeed();
    }

    private void RefreshDamage()
    {
        int purchases = playerProgression.GetPurchaseCount(
            PermanentUpgradeType.HeroDamage
        );

        int limit =
            playerProgression.GetHeroDamagePurchaseLimit();

        int price =
            playerProgression.GetNextHeroDamagePurchasePrice();

        SetUpgradeState(
            damageInfoText,
            damageBuyButton,
            damageBuyButtonText,
            purchases,
            limit,
            playerProgression.HeroPermanentDamageBonus,
            string.Empty,
            price
        );
    }

    private void RefreshHealth()
    {
        int purchases = playerProgression.GetPurchaseCount(
            PermanentUpgradeType.SharedMaxHealth
        );

        int limit =
            playerProgression.GetSharedMaxHealthPurchaseLimit();

        int price =
            playerProgression.GetNextSharedMaxHealthPurchasePrice();

        SetUpgradeState(
            healthInfoText,
            healthBuyButton,
            healthBuyButtonText,
            purchases,
            limit,
            playerProgression.SharedPermanentMaxHealthBonusPercent,
            "%",
            price
        );
    }

    private void RefreshCriticalChance()
    {
        bool isUnlocked =
            playerProgression.IsHeroCriticalChanceUnlocked();

        SetUpgradeRowVisible(
            criticalChanceInfoText,
            isUnlocked
        );

        if (!isUnlocked)
        {
            return;
        }

        int purchases = playerProgression.GetPurchaseCount(
            PermanentUpgradeType.HeroCriticalChance
        );

        int limit = playerProgression
            .GetHeroCriticalChancePurchaseLimit();

        int price = playerProgression
            .GetNextHeroCriticalChancePurchasePrice();

        SetUpgradeState(
            criticalChanceInfoText,
            criticalChanceBuyButton,
            criticalChanceBuyButtonText,
            purchases,
            limit,
            playerProgression
                .HeroPermanentCriticalChanceBonusPoints,
            "%",
            price
        );
    }

    private void RefreshAttackSpeed()
    {
        bool isUnlocked =
            playerProgression.IsHeroAttackSpeedUnlocked();

        SetUpgradeRowVisible(
            attackSpeedInfoText,
            isUnlocked
        );

        if (!isUnlocked)
        {
            return;
        }

        int purchases = playerProgression.GetPurchaseCount(
            PermanentUpgradeType.HeroAttackSpeed
        );

        int limit = playerProgression
            .GetHeroAttackSpeedPurchaseLimit();

        int price = playerProgression
            .GetNextHeroAttackSpeedPurchasePrice();

        SetUpgradeState(
            attackSpeedInfoText,
            attackSpeedBuyButton,
            attackSpeedBuyButtonText,
            purchases,
            limit,
            playerProgression
                .HeroPermanentAttackSpeedBonusPercent,
            "%",
            price
        );
    }

    private static void SetUpgradeRowVisible(
        TMP_Text infoText,
        bool isVisible
    )
    {
        GameObject row = infoText.transform.parent.gameObject;

        if (row.activeSelf != isVisible)
        {
            row.SetActive(isVisible);
        }
    }

    private void SetUpgradeState(
        TMP_Text infoText,
        Button buyButton,
        TMP_Text buyButtonText,
        int purchases,
        int limit,
        float bonus,
        string bonusSuffix,
        int price
    )
    {
        infoText.text =
            $"PURCHASES: {FormatInteger(purchases)} / " +
            $"{FormatInteger(limit)}\n" +
            $"BONUS: +{FormatBonus(bonus)}{bonusSuffix}";

        bool isComplete = purchases >= limit;

        if (isComplete)
        {
            buyButtonText.text = "MAX";
            buyButton.interactable = false;
            return;
        }

        bool canAfford = playerProgression.Gold >= price;

        buyButtonText.text = canAfford
            ? $"BUY\n{FormatInteger(price)} GOLD"
            : $"NEED\n{FormatInteger(price)} GOLD";

        buyButton.interactable = canAfford;
    }

    private bool ValidateReferences()
    {
        bool allReferencesAssigned = true;

        allReferencesAssigned &= ValidateReference(
            playerProgression,
            nameof(playerProgression)
        );

        allReferencesAssigned &= ValidateReference(
            heroLevelText,
            nameof(heroLevelText)
        );

        allReferencesAssigned &= ValidateReference(
            goldText,
            nameof(goldText)
        );

        allReferencesAssigned &= ValidateReference(
            damageInfoText,
            nameof(damageInfoText)
        );

        allReferencesAssigned &= ValidateReference(
            damageBuyButton,
            nameof(damageBuyButton)
        );

        allReferencesAssigned &= ValidateReference(
            damageBuyButtonText,
            nameof(damageBuyButtonText)
        );

        allReferencesAssigned &= ValidateReference(
            healthInfoText,
            nameof(healthInfoText)
        );

        allReferencesAssigned &= ValidateReference(
            healthBuyButton,
            nameof(healthBuyButton)
        );

        allReferencesAssigned &= ValidateReference(
            healthBuyButtonText,
            nameof(healthBuyButtonText)
        );

        allReferencesAssigned &= ValidateReference(
            criticalChanceInfoText,
            nameof(criticalChanceInfoText)
        );

        allReferencesAssigned &= ValidateReference(
            criticalChanceBuyButton,
            nameof(criticalChanceBuyButton)
        );

        allReferencesAssigned &= ValidateReference(
            criticalChanceBuyButtonText,
            nameof(criticalChanceBuyButtonText)
        );

        allReferencesAssigned &= ValidateReference(
            attackSpeedInfoText,
            nameof(attackSpeedInfoText)
        );

        allReferencesAssigned &= ValidateReference(
            attackSpeedBuyButton,
            nameof(attackSpeedBuyButton)
        );

        allReferencesAssigned &= ValidateReference(
            attackSpeedBuyButtonText,
            nameof(attackSpeedBuyButtonText)
        );

        return allReferencesAssigned;
    }

    private bool ValidateReference(
        Object reference,
        string fieldName
    )
    {
        if (reference != null)
        {
            return true;
        }

        Debug.LogError(
            $"ProgressionMenuUI: Assign '{fieldName}' " +
            "in the Inspector.",
            this
        );

        return false;
    }

    private static string FormatInteger(int value)
    {
        return value
            .ToString(
                "N0",
                CultureInfo.InvariantCulture
            )
            .Replace(',', ' ');
    }

    private static string FormatBonus(float value)
    {
        return value.ToString(
            "0.##",
            CultureInfo.InvariantCulture
        );
    }
}
