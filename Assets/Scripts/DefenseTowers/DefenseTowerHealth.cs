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

        // Начальный максимум уже включает все усиления до постройки.
        // Подписка нужна только для следующих усилений.
        combatStats.DefenseTowerRunHealthBonusAdded += HandleRunHealthBonusAdded;

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

    private void HandleRunHealthBonusAdded(int amount)
    {
        if (!isInitialized || isDestroyed || amount <= 0)
        {
            return;
        }

        if (amount > int.MaxValue - maxHealth)
        {
            Debug.LogError(
                "DefenseTowerHealth: HP overflow. " +
                "Configure permanent health before Play.",
                this
            );
            return;
        }

        // Полученный ранее урон сохраняется: 200/300 + 50 = 250/350.
        // Обрабатываем только новую прибавку, не весь накопленный бонус.
        maxHealth += amount;
        currentHealth += amount;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        Debug.Log(
            $"DefenseTowerHealth: {gameObject.name} run HP upgrade +{amount}. " +
            $"HP: {currentHealth}/{maxHealth}.",
            this
        );
    }

    private void UnsubscribeFromHealthBonuses()
    {
        if (combatStats != null)
        {
            combatStats.DefenseTowerRunHealthBonusAdded -= HandleRunHealthBonusAdded;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealthBonuses();
    }

    private void DestroyTower()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;
        UnsubscribeFromHealthBonuses();

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

    [ContextMenu("Test Health/Print Health")]
    private void TestPrintHealth()
    {
        if (!Application.isPlaying)
            return;

        Debug.Log(
            $"DefenseTowerHealth: {gameObject.name} HP = {currentHealth}/{maxHealth}. " +
            $"IsDestroyed = {isDestroyed}.",
            this
        );
    }
}
