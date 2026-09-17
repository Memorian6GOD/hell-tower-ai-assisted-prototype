using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] private CombatStats combatStats;

    private int maxHealth;
    private int currentHealth;
    private bool isInitialized;

    public event Action<PlayerHealth> Died;
    public event Action<PlayerHealth> Revived;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        if (combatStats == null)
        {
            Debug.LogError(
                "PlayerHealth: Assign CombatStats from GameProgression.",
                this
            );

            return;
        }

        int configuredMaxHealth = combatStats.HeroMaxHealth;

        if (configuredMaxHealth <= 0)
        {
            Debug.LogError(
                "PlayerHealth: Check GameBalanceConfig on CombatStats.",
                this
            );

            return;
        }

        maxHealth = configuredMaxHealth;
        currentHealth = maxHealth;

        IsDead = false;
        isInitialized = true;

        Debug.Log(
            $"PlayerHealth: Initial health = {currentHealth}/{maxHealth}.",
            this
        );
    }

    public void TakeDamage(int damage)
    {
        if (!isInitialized || IsDead || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log(
            $"Player takes {damage} damage. " +
            $"HP: {currentHealth}/{maxHealth}",
            this
        );

        if (currentHealth == 0)
        {
            Die();
        }
    }

    //  арточки и тесты выдают здоровье через этот метод.
    // true означает, что усиление действительно получено.
    public bool TryAddRunMaxHealth(int amount)
    {
        if (!Application.isPlaying || !isInitialized || amount <= 0)
        {
            return false;
        }

        if (IsDead)
        {
            Debug.Log(
                $"PlayerHealth: HP upgrade skipped because Player is dead. " +
                $"HP: {currentHealth}/{maxHealth}.",
                this
            );
            return false;
        }

        if (combatStats == null ||
            !combatStats.TryAddHeroRunMaxHealthBonus(this, amount))
        {
            return false;
        }

        // ѕрибавл€ем одинаковое количество к максимуму и текущему HP.
        // Ќапример: 60/100 + 20 превращаетс€ в 80/120.
        maxHealth = combatStats.HeroMaxHealth;
        currentHealth += amount;

        Debug.Log(
            $"PlayerHealth: Run HP upgrade +{amount}. " +
            $"HP: {currentHealth}/{maxHealth}.",
            this
        );

        return true;
    }

    // CombatStats провер€ет, что усиление получает живой герой,
    // который использует именно этот экземпл€р расчЄта характеристик.
    internal bool CanReceiveHealthUpgradeFrom(CombatStats source)
    {
        return isInitialized && !IsDead && combatStats == source;
    }

    public bool TryRevive(int healthPercent)
    {
        if (!isInitialized)
        {
            Debug.LogError(
                "PlayerHealth: Cannot revive before health is initialized.",
                this
            );

            return false;
        }

        if (!IsDead)
        {
            Debug.LogWarning(
                "PlayerHealth: Player cannot be revived " +
                "because Player is not dead.",
                this
            );

            return false;
        }

        int clampedHealthPercent =
            Mathf.Clamp(healthPercent, 1, 100);

        currentHealth = Mathf.Max(
            1,
            Mathf.CeilToInt(
                maxHealth * (clampedHealthPercent / 100f)
            )
        );

        IsDead = false;

        Debug.Log(
            $"PlayerHealth: Player revived with " +
            $"{currentHealth}/{maxHealth} HP.",
            this
        );

        Revived?.Invoke(this);

        return true;
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        Debug.Log("Player died.", this);

        Died?.Invoke(this);
    }
    [ContextMenu("Test Health/Take 40 Damage")]
    private void TestTake40Damage()
    {
        if (!Application.isPlaying)
            return;

        TakeDamage(40);
    }

    [ContextMenu("Test Health/Add 20 Run Max HP")]
    private void TestAdd20RunMaxHP()
    {
        if (!Application.isPlaying)
            return;

        TryAddRunMaxHealth(20);
    }

    [ContextMenu("Test Health/Kill Player")]
    private void TestKillPlayer()
    {
        if (!Application.isPlaying)
            return;

        TakeDamage(currentHealth);
    }

    [ContextMenu("Test Health/Print Health")]
    private void TestPrintHealth()
    {
        if (!Application.isPlaying)
            return;

        Debug.Log(
            $"PlayerHealth: HP = {currentHealth}/{maxHealth}. " +
            $"IsDead = {IsDead}.",
            this
        );
    }
}
