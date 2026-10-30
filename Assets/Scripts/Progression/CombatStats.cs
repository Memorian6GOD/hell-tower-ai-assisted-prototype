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


    // =========================================================
    // RUN STATE - HEALTH
    // =========================================================

    private int defenseTowerRunMaxHealthBonus;

    public int DefenseTowerRunMaxHealthBonus =>
        defenseTowerRunMaxHealthBonus;

    internal event System.Action<int>
        DefenseTowerRunHealthBonusAdded;

    private int coreTowerRunMaxHealthBonus;

    public int CoreTowerRunMaxHealthBonus =>
        coreTowerRunMaxHealthBonus;

    private int heroRunMaxHealthBonus;

    public int HeroRunMaxHealthBonus =>
        heroRunMaxHealthBonus;


    // =========================================================
    // RUN STATE - COMBAT BONUSES
    // =========================================================

    private float heroDamageBonusPercent;
    private float towerDamageBonusPercent;

    private float heroAttackSpeedBonusPercent;
    private float towerAttackSpeedBonusPercent;

    private float heroCritChanceBonusPercent;
    private float heroMoveSpeedBonusPercent;


    public float HeroRunDamageBonusPercent =>
        Mathf.Max(
            0f,
            heroDamageBonusPercent
        );

    public float HeroRunAttackSpeedBonusPercent =>
        Mathf.Max(
            0f,
            heroAttackSpeedBonusPercent
        );

    public float HeroRunMoveSpeedBonusPercent =>
        Mathf.Max(
            0f,
            heroMoveSpeedBonusPercent
        );

    public float HeroRunCritChanceBonusPercent =>
        Mathf.Max(
            0f,
            heroCritChanceBonusPercent
        );

    public float TowerRunDamageBonusPercent =>
        Mathf.Max(
            0f,
            towerDamageBonusPercent
        );

    public float TowerRunAttackSpeedBonusPercent =>
        Mathf.Max(
            0f,
            towerAttackSpeedBonusPercent
        );


    // =========================================================
    // BASE COMBAT SETTINGS
    // =========================================================

    public float HeroMoveSpeed
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig.HeroBaseMoveSpeed *
                (1f + HeroRunMoveSpeedBonusPercent / 100f);
        }
    }

    public float HeroAttackRadius
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig
                .HeroAttackRadius;
        }
    }

    public float HeroBaseAttackWindup
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0.01f;
            }

            return balanceConfig
                .HeroBaseAttackWindup;
        }
    }

    public float HeroProjectileSpeed
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig
                .HeroProjectileSpeed;
        }
    }

    public float TowerAttackRadius
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig
                .DefenseTowerAttackRadius;
        }
    }

    public float TowerProjectileSpeed
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig
                .DefenseTowerProjectileSpeed;
        }
    }


    // =========================================================
    // UNITY
    // =========================================================

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

        if (balanceConfig == null)
        {
            Debug.LogError(
                "CombatStats: GameBalanceConfig is not assigned.",
                this
            );
        }
    }


    // =========================================================
    // SAVED PERMANENT PROGRESSION
    // =========================================================

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


    // =========================================================
    // HEALTH
    // =========================================================

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
                defenseTowerRunMaxHealthBonus
            );
        }
    }


    // =========================================================
    // HERO DAMAGE
    // =========================================================

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
                +
                SavedHeroDamageBonus;
        }
    }

    public float HeroDamage
    {
        get
        {
            float multiplier =
                1f +
                HeroRunDamageBonusPercent /
                100f;

            return
                HeroPermanentDamage *
                multiplier;
        }
    }


    // =========================================================
    // DEFENSE TOWER DAMAGE
    // =========================================================

    public float TowerDamage
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            float multiplier =
                1f +
                TowerRunDamageBonusPercent /
                100f;

            return
                HeroPermanentDamage
                *
                balanceConfig
                    .DefenseTowerDamageCoefficient
                *
                multiplier;
        }
    }


    // =========================================================
    // HERO ATTACK SPEED
    // =========================================================

    public float HeroAttackSpeedMultiplier
    {
        get
        {
            float permanentMultiplier =
                1f +
                SavedHeroAttackSpeedBonusPercent /
                100f;

            float runMultiplier =
                1f +
                HeroRunAttackSpeedBonusPercent /
                100f;

            return
                permanentMultiplier *
                runMultiplier;
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
                balanceConfig
                    .HeroBaseAttackInterval
                /
                HeroAttackSpeedMultiplier;

            return Mathf.Max(
                0.01f,
                interval
            );
        }
    }


    // =========================================================
    // DEFENSE TOWER ATTACK SPEED
    // =========================================================

    public float TowerAttackSpeedMultiplier
    {
        get
        {
            return
                1f +
                TowerRunAttackSpeedBonusPercent /
                100f;
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
                /
                TowerAttackSpeedMultiplier;

            return Mathf.Max(
                0.01f,
                interval
            );
        }
    }


    // =========================================================
    // CRITICAL HIT
    // =========================================================

    public float HeroPermanentCritChancePercent
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            float chance =
                balanceConfig
                    .HeroBaseCritChancePercent
                +
                SavedHeroCriticalChanceBonusPoints;

            return Mathf.Clamp(
                chance,
                0f,
                100f
            );
        }
    }

    public float HeroCritChancePercent
    {
        get
        {
            float multiplier =
                1f +
                HeroRunCritChanceBonusPercent /
                100f;

            float chance =
                HeroPermanentCritChancePercent
                *
                multiplier;

            return Mathf.Clamp(
                chance,
                0f,
                100f
            );
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
                balanceConfig
                    .HeroCritDamageMultiplier
            );
        }
    }


    // =========================================================
    // RUN UPGRADE APPLICATION
    // =========================================================

    public bool TryAddHeroRunDamageBonusPercent(
        float amount
    )
    {
        return TryAddRunBonusPercent(
            ref heroDamageBonusPercent,
            amount,
            "hero damage"
        );
    }

    public bool TryAddHeroRunAttackSpeedBonusPercent(
        float amount
    )
    {
        return TryAddRunBonusPercent(
            ref heroAttackSpeedBonusPercent,
            amount,
            "hero attack speed"
        );
    }

    public bool TryAddHeroRunMoveSpeedBonusPercent(
        float amount
    )
    {
        return TryAddRunBonusPercent(
            ref heroMoveSpeedBonusPercent,
            amount,
            "hero movement speed"
        );
    }

    public bool TryAddHeroRunCritChanceBonusPercent(
        float amount
    )
    {
        return TryAddRunBonusPercent(
            ref heroCritChanceBonusPercent,
            amount,
            "hero critical chance"
        );
    }

    public bool TryAddTowerRunDamageBonusPercent(
        float amount
    )
    {
        return TryAddRunBonusPercent(
            ref towerDamageBonusPercent,
            amount,
            "defense tower damage"
        );
    }

    public bool TryAddTowerRunAttackSpeedBonusPercent(
        float amount
    )
    {
        return TryAddRunBonusPercent(
            ref towerAttackSpeedBonusPercent,
            amount,
            "defense tower attack speed"
        );
    }

    private bool TryAddRunBonusPercent(
        ref float currentBonusPercent,
        float amount,
        string statName
    )
    {
        if (!Application.isPlaying ||
            balanceConfig == null ||
            amount <= 0f ||
            float.IsNaN(amount) ||
            float.IsInfinity(amount))
        {
            return false;
        }

        double newBonus =
            Mathf.Max(
                0f,
                currentBonusPercent
            )
            +
            (double)amount;

        if (newBonus > float.MaxValue)
        {
            return false;
        }

        currentBonusPercent =
            (float)newBonus;

        Debug.Log(
            "CombatStats: Run " +
            $"{statName} bonus increased by " +
            $"{amount:F2}%. Total run bonus = " +
            $"{currentBonusPercent:F2}%.",
            this
        );

        return true;
    }


    // =========================================================
    // RUN HEALTH APPLICATION
    // =========================================================

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
                "balance before restarting.",
                this
            );

            return false;
        }

        if (amount >
            int.MaxValue -
            HeroMaxHealth)
        {
            return false;
        }

        heroRunMaxHealthBonus +=
            amount;

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
                "configure balance before restarting.",
                this
            );

            return false;
        }

        if (amount >
            int.MaxValue -
            CoreTowerMaxHealth)
        {
            return false;
        }

        coreTowerRunMaxHealthBonus +=
            amount;

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
            int.MaxValue -
            DefenseTowerMaxHealth)
        {
            return false;
        }

        defenseTowerRunMaxHealthBonus +=
            amount;

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


    // =========================================================
    // INTERNAL CALCULATIONS
    // =========================================================

    private int CalculateMaxHealth(
        int baseMaxHealth,
        int runBonus
    )
    {
        int safeBaseMaxHealth =
            Mathf.Max(
                1,
                baseMaxHealth
            );

        long totalHealth =
            (long)safeBaseMaxHealth
            +
            CalculateSavedMaxHealthBonus(
                safeBaseMaxHealth
            )
            +
            Mathf.Max(
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
            Mathf.Max(
                1,
                baseMaxHealth
            );

        double rawBonus =
            safeBaseMaxHealth
            *
            (double)Mathf.Max(
                0f,
                SavedSharedMaxHealthBonusPercent
            )
            /
            100d;

        if (rawBonus >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)System.Math.Round(
            rawBonus,
            System.MidpointRounding.AwayFromZero
        );
    }


    // =========================================================
    // DIAGNOSTICS
    // =========================================================

    [ContextMenu(
        "Test Tower Health/Add 50 Run Max HP To All Towers"
    )]
    private void TestAdd50DefenseTowerRunMaxHP()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TryAddDefenseTowerRunMaxHealthBonus(
            50
        );
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
            $"Run HP bonus: " +
            $"{CoreTowerRunMaxHealthBonus} | " +
            $"CoreTower max HP: " +
            $"{CoreTowerMaxHealth}",
            this
        );
    }

    [ContextMenu(
        "Print Hero Health Report"
    )]
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
            $"Run HP bonus: " +
            $"{HeroRunMaxHealthBonus} | " +
            $"Hero max HP: " +
            $"{HeroMaxHealth}",
            this
        );
    }

    [ContextMenu(
        "Print Damage Report"
    )]
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
            $"Permanent hero damage: " +
            $"{HeroPermanentDamage:F2} | " +
            $"Run damage bonus: " +
            $"{HeroRunDamageBonusPercent:F2}% | " +
            $"Final hero damage: " +
            $"{HeroDamage:F2} | " +
            $"Tower damage: " +
            $"{TowerDamage:F2}",
            this
        );
    }

    [ContextMenu(
        "Print Attack Speed Report"
    )]
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
            $"Run hero speed bonus: " +
            $"{HeroRunAttackSpeedBonusPercent:F2}% | " +
            $"Hero speed multiplier: " +
            $"{HeroAttackSpeedMultiplier:F2} | " +
            $"Hero attack interval: " +
            $"{HeroAttackInterval:F3} s | " +
            $"Tower speed multiplier: " +
            $"{TowerAttackSpeedMultiplier:F2} | " +
            $"Tower attack interval: " +
            $"{TowerAttackInterval:F3} s | " +
            $"Hero projectile speed: " +
            $"{HeroProjectileSpeed:F2} | " +
            $"Tower projectile speed: " +
            $"{TowerProjectileSpeed:F2}",
            this
        );
    }

    [ContextMenu(
        "Print Move Speed Report"
    )]
    private void PrintMoveSpeedReport()
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
            $"Base hero move speed: {balanceConfig.HeroBaseMoveSpeed:F2} | " +
            $"Run move speed bonus: {HeroRunMoveSpeedBonusPercent:F2}% | " +
            $"Final hero move speed: {HeroMoveSpeed:F2}",
            this
        );
    }

    [ContextMenu(
        "Print Crit Report"
    )]
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
            $"Permanent crit chance: " +
            $"{HeroPermanentCritChancePercent:F2}% | " +
            $"Run crit bonus: " +
            $"{HeroRunCritChanceBonusPercent:F2}% | " +
            $"Final crit chance: " +
            $"{HeroCritChancePercent:F2}% | " +
            $"Crit damage multiplier: " +
            $"{HeroCritDamageMultiplier:F2}",
            this
        );
    }
}
