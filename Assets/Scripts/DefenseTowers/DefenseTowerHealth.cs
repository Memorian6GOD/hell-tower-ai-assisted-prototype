using System;
using UnityEngine;

public class DefenseTowerHealth : MonoBehaviour
{
    private CombatStats combatStats;
    private DefenseTowerHealthBar healthBar;

    private int maxHealth;
    private int currentHealth;
    private bool isDestroyed;
    private bool isInitialized;

    public event Action<DefenseTowerHealth> Destroyed;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDestroyed => isDestroyed;

    private void Start()
    {
        // Башня создаётся из префаба и находит настройки на сцене.
        combatStats = FindAnyObjectByType<CombatStats>();

        if (combatStats == null)
        {
            Debug.LogError(
                "DefenseTowerHealth: CombatStats was not found. " +
                "Check GameProgression in the scene.",
                this
            );
            return;
        }

        int configuredMaxHealth = combatStats.DefenseTowerMaxHealth;

        if (configuredMaxHealth <= 0)
        {
            Debug.LogError(
                "DefenseTowerHealth: Check GameBalanceConfig on CombatStats.",
                this
            );
            return;
        }

        maxHealth = configuredMaxHealth;
        currentHealth = maxHealth;
        isDestroyed = false;
        isInitialized = true;

        healthBar =
            GetComponentInChildren<DefenseTowerHealthBar>(true);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
            healthBar.Show();
        }

        Debug.Log(
            $"DefenseTowerHealth: {gameObject.name} " +
            $"initial health = {currentHealth}/{maxHealth}.",
            this
        );
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isInitialized || isDestroyed || damageAmount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        Debug.Log(
            $"{gameObject.name} health: {currentHealth}/{maxHealth}",
            this
        );

        if (currentHealth == 0)
        {
            DestroyTower();
        }
    }

    private void DestroyTower()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;

        if (healthBar != null)
        {
            healthBar.Hide();
        }

        Debug.Log(
            $"{gameObject.name} destroyed!",
            this
        );

        Destroyed?.Invoke(this);

        Destroy(gameObject);
    }

    [ContextMenu("Test Damage 50")]
    private void TestDamage()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TakeDamage(50);
    }
}