using UnityEngine;

[RequireComponent(typeof(PlayerProgression))]
[DisallowMultipleComponent]
public class CombatStats : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField]
    private GameBalanceConfig balanceConfig;

    [SerializeField]
    private PlayerProgression playerProgression;

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

    // Общий временный бонус для всех защитных башен.
    // Его получают уже построенные и будущие башни.
    private int defenseTowerRunMaxHealthBonus;

    public int DefenseTowerRunMaxHealthBonus =>
        defenseTowerRunMaxHealthBonus;

    internal event System.Action<int>
        DefenseTowerRunHealthBonusAdded;

    // Временный бонус CoreTower хранится отдельно.
    private int coreTowerRunMaxHealthBonus;

    public int CoreTowerRunMaxHealthBonus =>
        coreTowerRunMaxHealthBonus;

    // Этот бонус существует только во время текущего забега.
    private int heroRunMaxHealthBonus;

    public int HeroRunMaxHealthBonus =>
        heroRunMaxHealthBonus;

    [Header("Test Inputs - Run Upgrades")]

    [SerializeField, Min(0f)]
    private float heroDamageBonusPercent = 0f;

    [SerializeField, Min(0f)]
    private float towerDamageBonusPercent = 0f;

    [SerializeField, Min(0f)]
    private float heroAttackSpeedBonusPercent = 0f;

    [SerializeField, Min(0f)]
    private float towerAttackSpeedBonusPercent = 0f;

    // Относительная прибавка:
    // постоянный шанс 5% с бонусом 20% даёт 6%.
    [SerializeField, Min(0f)]
    private float heroCritChanceBonusPercent = 0f;

    private void Awake()
    {
        if (playerProgression == null)
        {
            playerProgression =
                GetComponent<PlayerProgression>();
        }

        if (playerProgression == null)
        {
            Debug.LogError(
                "CombatStats: PlayerProgression was not " +
                "found on GameProgression.",
                this
            );
        }
    }

    public float SavedHeroDamageBonus
    {
        get
        {
            if (playerProgression == null)
            {
                return 0f;
            }

            return playerProgression
                .HeroPermanentDamageBonus;
        }
    }

    public float SavedHeroCriticalChanceBonusPoints
    {
        get
        {
            if (playerProgression == null)
            {
                return 0f;
            }

            return playerProgression
                .HeroPermanentCriticalChanceBonusPoints;
        }
    }

    public float SavedHeroAttackSpeedBonusPercent
    {
        get
        {
            if (playerProgression == null)
            {
                return 0f;
            }

            return playerProgression
                .HeroPermanentAttackSpeedBonusPercent;
        }
    }

    public float SavedSharedMaxHealthBonusPercent
    {
        get
        {
            if (playerProgression == null)
            {
                return 0f;
            }

            return playerProgression
                .SharedPermanentMaxHealthBonusPercent;
        }
    }

    public int HeroMaxHealth
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0;
            }

            return CalculateMaxHealth(
                balanceConfig.HeroBaseMaxHealth,
                permanentHeroMaxHealthBonus,
                heroRunMaxHealthBonus
            );
        }
    }

    public int CoreTowerMaxHealth
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0;
            }

            return CalculateMaxHealth(
                balanceConfig.CoreTowerBaseMaxHealth,
                permanentCoreTowerMaxHealthBonus,
                coreTowerRunMaxHealthBonus
            );
        }
    }

    public int DefenseTowerMaxHealth
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0;
            }

            return CalculateMaxHealth(
                balanceConfig.DefenseTowerBaseMaxHealth,
                permanentDefenseTowerMaxHealthBonus,
                defenseTowerRunMaxHealthBonus
            );
        }
    }

    public float HeroPermanentDamage
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return
                balanceConfig.HeroBaseDamage
                + Mathf.Max(
                    0f,
                    permanentDamageBonus
                )
                + SavedHeroDamageBonus;
        }
    }

    public float HeroDamage
    {
        get
        {
            float multiplier =
                1f + heroDamageBonusPercent / 100f;

            return HeroPermanentDamage * multiplier;
        }
    }

    public float TowerDamage
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            float multiplier =
                1f + towerDamageBonusPercent / 100f;

            return
                HeroPermanentDamage
                * balanceConfig
                    .DefenseTowerDamageCoefficient
                * multiplier;
        }
    }

    public float HeroAttackSpeedMultiplier
    {
        get
        {
            float permanentMultiplier =
                1f
                + (
                    Mathf.Max(
                        0f,
                        permanentAttackSpeedBonusPercent
                    )
                    + SavedHeroAttackSpeedBonusPercent
                ) / 100f;

            float runMultiplier =
                1f
                + Mathf.Max(
                    0f,
                    heroAttackSpeedBonusPercent
                ) / 100f;

            return permanentMultiplier * runMultiplier;
        }
    }

    public float HeroAttackInterval
    {
        get
        {
            if (balanceConfig == null)
            {
                return 1f;
            }

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
            return
                1f
                + Mathf.Max(
                    0f,
                    towerAttackSpeedBonusPercent
                ) / 100f;
        }
    }

    public float TowerAttackInterval
    {
        get
        {
            if (balanceConfig == null)
            {
                return 1f;
            }

            float interval =
                balanceConfig
                    .DefenseTowerBaseAttackInterval
                / TowerAttackSpeedMultiplier;

            return Mathf.Max(0.01f, interval);
        }
    }

    public float HeroPermanentCritChancePercent
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            float chance =
                balanceConfig.HeroBaseCritChancePercent
                + Mathf.Max(
                    0f,
                    permanentCritChanceBonusPoints
                )
                + SavedHeroCriticalChanceBonusPoints;

            return Mathf.Clamp(chance, 0f, 100f);
        }
    }

    public float HeroCritChancePercent
    {
        get
        {
            float multiplier =
                1f
                + Mathf.Max(
                    0f,
                    heroCritChanceBonusPercent
                ) / 100f;

            float chance =
                HeroPermanentCritChancePercent
                * multiplier;

            return Mathf.Clamp(chance, 0f, 100f);
        }
    }

    public float HeroCritDamageMultiplier
    {
        get
        {
            if (balanceConfig == null)
            {
                return 1f;
            }

            return Mathf.Max(
                1f,
                balanceConfig.HeroCritDamageMultiplier
            );
        }
    }

    internal bool TryAddHeroRunMaxHealthBonus(
        PlayerHealth recipient,
        int amount
    )
    {
        if (!Application.isPlaying ||
            balanceConfig == null ||
            amount <= 0)
        {
            return false;
        }

        if (recipient == null ||
            !recipient.CanReceiveHealthUpgradeFrom(this))
        {
            return false;
        }

        if (recipient.MaxHealth != HeroMaxHealth)
        {
            Debug.LogError(
                "CombatStats: Hero health settings changed " +
                "during Play. Stop Play and configure " +
                "permanent health before restarting.",
                this
            );

            return false;
        }

        if (amount > int.MaxValue - HeroMaxHealth)
        {
            return false;
        }

        heroRunMaxHealthBonus += amount;

        return true;
    }

    internal bool TryAddCoreTowerRunMaxHealthBonus(
        CoreTowerHealth recipient,
        int amount
    )
    {
        if (!Application.isPlaying ||
            balanceConfig == null ||
            amount <= 0)
        {
            return false;
        }

        if (recipient == null ||
            !recipient.CanReceiveHealthUpgradeFrom(this))
        {
            return false;
        }

        if (recipient.MaxHealth != CoreTowerMaxHealth)
        {
            Debug.LogError(
                "CombatStats: CoreTower health settings " +
                "changed during Play. Stop Play and " +
                "configure permanent health before restarting.",
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

    public bool TryAddDefenseTowerRunMaxHealthBonus(
        int amount
    )
    {
        if (!Application.isPlaying ||
            balanceConfig == null ||
            amount <= 0)
        {
            return false;
        }

        if (amount >
            int.MaxValue - DefenseTowerMaxHealth)
        {
            return false;
        }

        defenseTowerRunMaxHealthBonus += amount;

        DefenseTowerRunHealthBonusAdded?.Invoke(
            amount
        );

        Debug.Log(
            "CombatStats: Defense tower run HP " +
            $"upgrade +{amount}. " +
            $"Total run HP bonus = " +
            $"{DefenseTowerRunMaxHealthBonus}. " +
            $"Defense tower max HP = " +
            $"{DefenseTowerMaxHealth}.",
            this
        );

        return true;
    }

    private int CalculateMaxHealth(
        int baseMaxHealth,
        int manualPermanentBonus,
        int runBonus
    )
    {
        int safeBaseMaxHealth =
            Mathf.Max(1, baseMaxHealth);

        long totalHealth =
            (long)safeBaseMaxHealth
            + CalculateSavedMaxHealthBonus(
                safeBaseMaxHealth
            )
            + Mathf.Max(
                0,
                manualPermanentBonus
            )
            + Mathf.Max(
                0,
                runBonus
            );

        return (int)System.Math.Min(
            int.MaxValue,
            totalHealth
        );
    }

    private int CalculateSavedMaxHealthBonus(
        int baseMaxHealth
    )
    {
        int safeBaseMaxHealth =
            Mathf.Max(1, baseMaxHealth);

        double rawBonus =
            safeBaseMaxHealth
            * (double)Mathf.Max(
                0f,
                SavedSharedMaxHealthBonusPercent
            )
            / 100d;

        if (rawBonus >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)System.Math.Round(
            rawBonus,
            System.MidpointRounding.AwayFromZero
        );
    }

    [ContextMenu(
        "Test Tower Health/Add 50 Run Max HP To All Towers"
    )]
    private void TestAdd50DefenseTowerRunMaxHP()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TryAddDefenseTowerRunMaxHealthBonus(50);
    }

    [ContextMenu(
        "Print Defense Tower Health Report"
    )]
    private void PrintDefenseTowerHealthReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: Assign GameBalanceConfig.",
                this
            );

            return;
        }

        Debug.Log(
            "CombatStats | " +
            $"Base defense tower HP: " +
            $"{balanceConfig.DefenseTowerBaseMaxHealth} | " +
            $"Saved shared HP bonus: " +
            $"{SavedSharedMaxHealthBonusPercent:F2}% " +
            $"(+{CalculateSavedMaxHealthBonus(balanceConfig.DefenseTowerBaseMaxHealth)} HP) | " +
            $"Manual permanent HP bonus: " +
            $"{Mathf.Max(0, permanentDefenseTowerMaxHealthBonus)} | " +
            $"Run HP bonus: " +
            $"{DefenseTowerRunMaxHealthBonus} | " +
            $"Defense tower max HP: " +
            $"{DefenseTowerMaxHealth}",
            this
        );
    }

    [ContextMenu(
        "Print Core Tower Health Report"
    )]
    private void PrintCoreTowerHealthReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: Assign GameBalanceConfig.",
                this
            );

            return;
        }

        Debug.Log(
            "CombatStats | " +
            $"Base CoreTower HP: " +
            $"{balanceConfig.CoreTowerBaseMaxHealth} | " +
            $"Saved shared HP bonus: " +
            $"{SavedSharedMaxHealthBonusPercent:F2}% " +
            $"(+{CalculateSavedMaxHealthBonus(balanceConfig.CoreTowerBaseMaxHealth)} HP) | " +
            $"Manual permanent HP bonus: " +
            $"{Mathf.Max(0, permanentCoreTowerMaxHealthBonus)} | " +
            $"Run HP bonus: " +
            $"{CoreTowerRunMaxHealthBonus} | " +
            $"CoreTower max HP: " +
            $"{CoreTowerMaxHealth}",
            this
        );
    }

    [ContextMenu("Print Hero Health Report")]
    private void PrintHeroHealthReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: Assign GameBalanceConfig.",
                this
            );

            return;
        }

        Debug.Log(
            "CombatStats | " +
            $"Base hero HP: " +
            $"{balanceConfig.HeroBaseMaxHealth} | " +
            $"Saved shared HP bonus: " +
            $"{SavedSharedMaxHealthBonusPercent:F2}% " +
            $"(+{CalculateSavedMaxHealthBonus(balanceConfig.HeroBaseMaxHealth)} HP) | " +
            $"Manual permanent HP bonus: " +
            $"{Mathf.Max(0, permanentHeroMaxHealthBonus)} | " +
            $"Run HP bonus: " +
            $"{HeroRunMaxHealthBonus} | " +
            $"Hero max HP: " +
            $"{HeroMaxHealth}",
            this
        );
    }

    [ContextMenu("Print Damage Report")]
    private void PrintDamageReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: Assign GameBalanceConfig.",
                this
            );

            return;
        }

        Debug.Log(
            "CombatStats | " +
            $"Base hero damage: " +
            $"{balanceConfig.HeroBaseDamage:F2} | " +
            $"Saved purchase bonus: " +
            $"{SavedHeroDamageBonus:F2} | " +
            $"Manual test bonus: " +
            $"{Mathf.Max(0f, permanentDamageBonus):F2} | " +
            $"Permanent hero damage: " +
            $"{HeroPermanentDamage:F2} | " +
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
                "CombatStats: Assign GameBalanceConfig.",
                this
            );

            return;
        }

        Debug.Log(
            "CombatStats | " +
            $"Saved hero speed bonus: " +
            $"{SavedHeroAttackSpeedBonusPercent:F2}% | " +
            $"Manual permanent speed bonus: " +
            $"{Mathf.Max(0f, permanentAttackSpeedBonusPercent):F2}% | " +
            $"Run hero speed bonus: " +
            $"{Mathf.Max(0f, heroAttackSpeedBonusPercent):F2}% | " +
            $"Hero speed multiplier: " +
            $"{HeroAttackSpeedMultiplier:F2} | " +
            $"Hero attack interval: " +
            $"{HeroAttackInterval:F3} s | " +
            $"Tower speed multiplier: " +
            $"{TowerAttackSpeedMultiplier:F2} | " +
            $"Tower attack interval: " +
            $"{TowerAttackInterval:F3} s",
            this
        );
    }

    [ContextMenu("Print Crit Report")]
    private void PrintCritReport()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: Assign GameBalanceConfig.",
                this
            );

            return;
        }

        Debug.Log(
            "CombatStats | " +
            $"Base crit chance: " +
            $"{balanceConfig.HeroBaseCritChancePercent:F2}% | " +
            $"Saved purchase bonus: " +
            $"{SavedHeroCriticalChanceBonusPoints:F2} point(s) | " +
            $"Manual test bonus: " +
            $"{Mathf.Max(0f, permanentCritChanceBonusPoints):F2} point(s) | " +
            $"Permanent crit chance: " +
            $"{HeroPermanentCritChancePercent:F2}% | " +
            $"Final crit chance: " +
            $"{HeroCritChancePercent:F2}% | " +
            $"Crit damage multiplier: " +
            $"{HeroCritDamageMultiplier:F2}",
            this
        );
    }
}