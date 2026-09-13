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
}