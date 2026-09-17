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

    [Header("Test Inputs - Permanent Health (Before Play)")]
    [SerializeField, Min(0)]
    private int permanentHeroMaxHealthBonus = 0;

    [SerializeField, Min(0)]
    private int permanentCoreTowerMaxHealthBonus = 0;

    [SerializeField, Min(0)]
    private int permanentDefenseTowerMaxHealthBonus = 0;

    // Общая прибавка забега для всех защитных башен, включая будущие.
    private int defenseTowerRunMaxHealthBonus;

    public int DefenseTowerRunMaxHealthBonus => defenseTowerRunMaxHealthBonus;

    // Каждая уже созданная башня получает уведомление об одной новой прибавке.
    internal event System.Action<int> DefenseTowerRunHealthBonusAdded;

    // Временная прибавка CoreTower хранится отдельно от бонуса героя.
    private int coreTowerRunMaxHealthBonus;

    public int CoreTowerRunMaxHealthBonus => coreTowerRunMaxHealthBonus;

    // Бонус забега изменяется только через выдачу усиления живому герою.
    // Он не сериализуется и не сохраняется в сцене.
    private int heroRunMaxHealthBonus;

    public int HeroRunMaxHealthBonus => heroRunMaxHealthBonus;

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

            long totalHealth =
                (long)Mathf.Max(1, balanceConfig.HeroBaseMaxHealth)
                + Mathf.Max(0, permanentHeroMaxHealthBonus)
                + heroRunMaxHealthBonus;

            return (int)System.Math.Min(int.MaxValue, totalHealth);
        }
    }

    public int CoreTowerMaxHealth
    {
        get
        {
            if (balanceConfig == null)
                return 0;

            long totalHealth =
                (long)Mathf.Max(1, balanceConfig.CoreTowerBaseMaxHealth)
                + Mathf.Max(0, permanentCoreTowerMaxHealthBonus)
                + coreTowerRunMaxHealthBonus;

            return (int)System.Math.Min(int.MaxValue, totalHealth);
        }
    }

    public int DefenseTowerMaxHealth
    {
        get
        {
            if (balanceConfig == null)
                return 0;

            long totalHealth =
                (long)Mathf.Max(1, balanceConfig.DefenseTowerBaseMaxHealth)
                + Mathf.Max(0, permanentDefenseTowerMaxHealthBonus)
                + defenseTowerRunMaxHealthBonus;

            return (int)System.Math.Min(int.MaxValue, totalHealth);
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

    // Служебный метод: выдача усиления начинается в PlayerHealth.
    internal bool TryAddHeroRunMaxHealthBonus(
        PlayerHealth recipient,
        int amount
    )
    {
        if (!Application.isPlaying || balanceConfig == null || amount <= 0)
        {
            return false;
        }

        // Проверяем получателя ДО изменения общего бонуса.
        if (recipient == null || !recipient.CanReceiveHealthUpgradeFrom(this))
        {
            return false;
        }

        // Базу и постоянные тестовые бонусы задаём до запуска забега.
        if (recipient.MaxHealth != HeroMaxHealth)
        {
            Debug.LogError(
                "CombatStats: Hero health settings changed during Play. " +
                "Stop Play and configure permanent health before restarting.",
                this
            );
            return false;
        }

        // Защита от переполнения целочисленного здоровья.
        if (amount > int.MaxValue - HeroMaxHealth)
        {
            return false;
        }

        heroRunMaxHealthBonus += amount;
        return true;
    }

    // Выдача усиления начинается в CoreTowerHealth.
    internal bool TryAddCoreTowerRunMaxHealthBonus(
        CoreTowerHealth recipient,
        int amount
    )
    {
        if (!Application.isPlaying || balanceConfig == null || amount <= 0)
        {
            return false;
        }

        // Уничтоженная или ещё не настроенная CoreTower бонус не получает.
        if (recipient == null || !recipient.CanReceiveHealthUpgradeFrom(this))
        {
            return false;
        }

        if (recipient.MaxHealth != CoreTowerMaxHealth)
        {
            Debug.LogError(
                "CombatStats: CoreTower health settings changed during Play. " +
                "Stop Play and configure permanent health before restarting.",
                this
            );
            return false;
        }

        if (amount > int.MaxValue - CoreTowerMaxHealth)
        {
            return false;
        }

        coreTowerRunMaxHealthBonus += amount;
        return true;
    }

    // Вызывается один раз на выбранное усиление, а не отдельно для каждой башни.
    // Бонус можно получить даже до постройки первой башни.
    public bool TryAddDefenseTowerRunMaxHealthBonus(int amount)
    {
        if (!Application.isPlaying || balanceConfig == null || amount <= 0)
        {
            return false;
        }

        if (amount > int.MaxValue - DefenseTowerMaxHealth)
        {
            return false;
        }

        defenseTowerRunMaxHealthBonus += amount;

        // Сначала сохраняем общий бонус, затем уведомляем существующие башни.
        DefenseTowerRunHealthBonusAdded?.Invoke(amount);

        Debug.Log(
            $"CombatStats: Defense tower run HP upgrade +{amount}. " +
            $"Total run HP bonus = {DefenseTowerRunMaxHealthBonus}. " +
            $"Defense tower max HP = {DefenseTowerMaxHealth}.",
            this
        );

        return true;
    }

    [ContextMenu("Test Tower Health/Add 50 Run Max HP To All Towers")]
    private void TestAdd50DefenseTowerRunMaxHP()
    {
        if (!Application.isPlaying)
            return;

        TryAddDefenseTowerRunMaxHealthBonus(50);
    }

    [ContextMenu("Print Defense Tower Health Report")]
    private void PrintDefenseTowerHealthReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError("CombatStats: Assign GameBalanceConfig.", this);
            return;
        }

        Debug.Log(
            $"CombatStats | Base defense tower HP: {balanceConfig.DefenseTowerBaseMaxHealth} | " +
            $"Permanent HP bonus: {Mathf.Max(0, permanentDefenseTowerMaxHealthBonus)} | " +
            $"Run HP bonus: {DefenseTowerRunMaxHealthBonus} | " +
            $"Defense tower max HP: {DefenseTowerMaxHealth}",
            this
        );
    }

    [ContextMenu("Print Core Tower Health Report")]
    private void PrintCoreTowerHealthReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError("CombatStats: Assign GameBalanceConfig.", this);
            return;
        }

        Debug.Log(
            $"CombatStats | Base CoreTower HP: {balanceConfig.CoreTowerBaseMaxHealth} | " +
            $"Permanent HP bonus: {Mathf.Max(0, permanentCoreTowerMaxHealthBonus)} | " +
            $"Run HP bonus: {CoreTowerRunMaxHealthBonus} | " +
            $"CoreTower max HP: {CoreTowerMaxHealth}",
            this
        );
    }

    [ContextMenu("Print Hero Health Report")]
    private void PrintHeroHealthReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError("CombatStats: Assign GameBalanceConfig.", this);
            return;
        }

        Debug.Log(
            $"CombatStats | Base hero HP: {balanceConfig.HeroBaseMaxHealth} | " +
            $"Permanent HP bonus: {Mathf.Max(0, permanentHeroMaxHealthBonus)} | " +
            $"Run HP bonus: {HeroRunMaxHealthBonus} | " +
            $"Hero max HP: {HeroMaxHealth}",
            this
        );
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
