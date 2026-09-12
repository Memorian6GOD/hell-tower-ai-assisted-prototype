using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("Health Bar Settings")]
    [SerializeField]
    private bool showHealthBarOnStart;

    private int currentHealth;
    private EnemyHealthBar healthBar;
    private bool healthMultiplierApplied;

    public event Action<EnemyHealth> Died;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;

        healthBar =
            GetComponentInChildren<EnemyHealthBar>(
                true
            );

        if (healthBar != null)
        {
            healthBar.SetHealth(
                currentHealth,
                maxHealth
            );

            if (showHealthBarOnStart)
            {
                healthBar.Show();
            }
            else
            {
                healthBar.Hide();
            }
        }
    }

    public void ApplyHealthMultiplier(float healthMultiplier)
    {
        if (healthMultiplierApplied)
        {
            Debug.LogWarning(
                "EnemyHealth: Health multiplier was already applied."
            );

            return;
        }

        if (healthMultiplier <= 0f)
        {
            Debug.LogWarning(
                "EnemyHealth: Health multiplier must be greater than 0."
            );

            return;
        }

        maxHealth = Mathf.Max(
            1,
            Mathf.CeilToInt(
                maxHealth * healthMultiplier
            )
        );

        healthMultiplierApplied = true;
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead)
        {
            return;
        }

        if (damageAmount <= 0)
        {
            return;
        }

        currentHealth -= damageAmount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log(
            "Enemy health: " +
            currentHealth
        );

        if (healthBar != null)
        {
            healthBar.SetHealth(
                currentHealth,
                maxHealth
            );
        }

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void ShowHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.Show();
        }
    }

    public void HideHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.Hide();
        }
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        Died?.Invoke(this);

        Destroy(gameObject);
    }
}
