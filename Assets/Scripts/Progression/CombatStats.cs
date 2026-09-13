using UnityEngine;

[DisallowMultipleComponent]
public class CombatStats : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameBalanceConfig balanceConfig;

    [Header("Test Inputs - Permanent Progression")]

    [SerializeField, Min(0f)]
    private float permanentDamageBonus = 0f;

    [SerializeField, Min(0f)]
    private float permanentAttackSpeedBonusPercent = 0f;

    // Процентные пункты: 3% + 2 пункта = 5%.
    [SerializeField, Range(0f, 100f)]
    private float permanentCritChanceBonusPoints = 0f;

    [Header("Test Inputs - Run Upgrades")]

    [SerializeField, Min(0f)]
    private float heroDamageBonusPercent = 0f;

    [SerializeField, Min(0f)]
    private float towerDamageBonusPercent = 0f;

    [SerializeField, Min(0f)]
    private float heroAttackSpeedBonusPercent = 0f;

    [SerializeField, Min(0f)]
    private float towerAttackSpeedBonusPercent = 0f;

    // Относительная прибавка: шанс 5% с бонусом 20% даёт 6%.
    [SerializeField, Min(0f)]
    private float heroCritChanceBonusPercent = 0f;

    public int HeroMaxHealth
    {
        get
        {
            if (balanceConfig == null)
                return 0;

            return Mathf.Max(1, balanceConfig.HeroBaseMaxHealth);
        }
    }

    public int CoreTowerMaxHealth
    {
        get
        {
            if (balanceConfig == null)
                return 0;

            return Mathf.Max(1, balanceConfig.CoreTowerBaseMaxHealth);
        }
    }

    public int DefenseTowerMaxHealth
    {
        get
        {
            if (balanceConfig == null)
                return 0;

            return Mathf.Max(1, balanceConfig.DefenseTowerBaseMaxHealth);
        }
    }

    public float HeroPermanentDamage
    {
        get
        {
            if (balanceConfig == null)
                return 0f;

            return balanceConfig.HeroBaseDamage + permanentDamageBonus;
        }
    }

    public float HeroDamage
    {
        get
        {
            float multiplier = 1f + heroDamageBonusPercent / 100f;
            return HeroPermanentDamage * multiplier;
        }
    }

    public float TowerDamage
    {
        get
        {
            if (balanceConfig == null)
                return 0f;

            float multiplier = 1f + towerDamageBonusPercent / 100f;

            return HeroPermanentDamage
                * balanceConfig.DefenseTowerDamageCoefficient
                * multiplier;
        }
    }

    public float HeroAttackSpeedMultiplier
    {
        get
        {
            float permanentMultiplier =
                1f + Mathf.Max(0f, permanentAttackSpeedBonusPercent) / 100f;

            float runMultiplier =
                1f + Mathf.Max(0f, heroAttackSpeedBonusPercent) / 100f;

            return permanentMultiplier * runMultiplier;
        }
    }

    public float HeroAttackInterval
    {
        get
        {
            if (balanceConfig == null)
                return 1f;

            float interval =
                balanceConfig.HeroBaseAttackInterval
                / HeroAttackSpeedMultiplier;

            return Mathf.Max(0.01f, interval);
        }
    }

    public float TowerAttackSpeedMultiplier
    {
        get
        {
            return 1f + Mathf.Max(
                0f, towerAttackSpeedBonusPercent
            ) / 100f;
        }
    }

    public float TowerAttackInterval
    {
        get
        {
            if (balanceConfig == null)
                return 1f;

            float interval =
                balanceConfig.DefenseTowerBaseAttackInterval
                / TowerAttackSpeedMultiplier;

            return Mathf.Max(0.01f, interval);
        }
    }

    public float HeroPermanentCritChancePercent
    {
        get
        {
            if (balanceConfig == null)
                return 0f;

            float chance =
                balanceConfig.HeroBaseCritChancePercent
                + Mathf.Max(0f, permanentCritChanceBonusPoints);

            return Mathf.Clamp(chance, 0f, 100f);
        }
    }

    public float HeroCritChancePercent
    {
        get
        {
            float multiplier =
                1f + Mathf.Max(0f, heroCritChanceBonusPercent) / 100f;

            float chance = HeroPermanentCritChancePercent * multiplier;

            return Mathf.Clamp(chance, 0f, 100f);
        }
    }

    public float HeroCritDamageMultiplier
    {
        get
        {
            if (balanceConfig == null)
                return 1f;

            return Mathf.Max(1f, balanceConfig.HeroCritDamageMultiplier);
        }
    }

    [ContextMenu("Print Damage Report")]
    private void PrintDamageReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: Assign GameBalanceConfig.", this
            );
            return;
        }

        Debug.Log(
            $"CombatStats | " +
            $"Permanent hero damage: {HeroPermanentDamage:F2} | " +
            $"Hero damage: {HeroDamage:F2} | " +
            $"Tower damage: {TowerDamage:F2}",
            this
        );
    }

    [ContextMenu("Print Attack Speed Report")]
    private void PrintAttackSpeedReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: Assign GameBalanceConfig.", this
            );
            return;
        }

        Debug.Log(
            $"CombatStats | " +
            $"Hero speed multiplier: {HeroAttackSpeedMultiplier:F2} | " +
            $"Hero attack interval: {HeroAttackInterval:F3} s | " +
            $"Tower speed multiplier: {TowerAttackSpeedMultiplier:F2} | " +
            $"Tower attack interval: {TowerAttackInterval:F3} s",
            this
        );
    }

    [ContextMenu("Print Crit Report")]
    private void PrintCritReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: Assign GameBalanceConfig.", this
            );
            return;
        }

        Debug.Log(
            $"CombatStats | " +
            $"Permanent crit chance: {HeroPermanentCritChancePercent:F2}% | " +
            $"Final crit chance: {HeroCritChancePercent:F2}% | " +
            $"Crit damage multiplier: {HeroCritDamageMultiplier:F2}",
            this
        );
    }
}