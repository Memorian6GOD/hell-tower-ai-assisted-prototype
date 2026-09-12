using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    public event Action<PlayerHealth> Died;
    public event Action<PlayerHealth> Revived;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        currentHealth = maxHealth;
        IsDead = false;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log(
            $"Player takes {damage} damage. " +
            $"HP: {currentHealth}/{maxHealth}"
        );

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public bool TryRevive(int healthPercent)
    {
        if (!IsDead)
        {
            Debug.LogWarning(
                "PlayerHealth: Player cannot be revived " +
                "because Player is not dead."
            );

            return false;
        }

        int clampedHealthPercent =
            Mathf.Clamp(
                healthPercent,
                1,
                100
            );

        currentHealth =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    maxHealth *
                    clampedHealthPercent /
                    100f
                )
            );

        IsDead = false;

        Debug.Log(
            "PlayerHealth: Player revived with " +
            currentHealth +
            "/" +
            maxHealth +
            " HP."
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

        Debug.Log("Player died.");

        Died?.Invoke(this);
    }
}