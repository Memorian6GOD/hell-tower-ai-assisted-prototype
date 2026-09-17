using System;
using UnityEngine;

public class CoreTowerHealth : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] private CombatStats combatStats;

    [Header("UI")]
    [SerializeField] private CoreTowerHealthBar healthBar;

    private int maxHealth;
    private int currentHealth;
    private bool isDestroyed;
    private bool isInitialized;

    private GameManager gameManager;

    public event Action<CoreTowerHealth> Destroyed;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDestroyed => isDestroyed;

    private void Start()
    {
        if (combatStats == null)
        {
            Debug.LogError(
                "CoreTowerHealth: Assign CombatStats from GameProgression.",
                this
            );
            return;
        }

        int configuredMaxHealth = combatStats.CoreTowerMaxHealth;

        if (configuredMaxHealth <= 0)
        {
            Debug.LogError(
                "CoreTowerHealth: Check GameBalanceConfig on CombatStats.",
                this
            );
            return;
        }

        maxHealth = configuredMaxHealth;
        currentHealth = maxHealth;
        isDestroyed = false;
        isInitialized = true;

        gameManager = FindAnyObjectByType<GameManager>();

        if (healthBar != null)
        {
            healthBar.Show();
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        Debug.Log(
            $"CoreTowerHealth: Initial health = {currentHealth}/{maxHealth}.",
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
            $"Core Tower health: {currentHealth}/{maxHealth}",
            this
        );

        if (currentHealth == 0)
        {
            DestroyCoreTower();
        }
    }

    // Увеличивает максимальное и текущее HP на одинаковую величину.
    public bool TryAddRunMaxHealth(int amount)
    {
        if (!Application.isPlaying || !isInitialized || amount <= 0)
        {
            return false;
        }

        if (isDestroyed)
        {
            Debug.Log(
                "CoreTowerHealth: HP upgrade skipped because CoreTower is destroyed.",
                this
            );
            return false;
        }

        if (combatStats == null ||
            !combatStats.TryAddCoreTowerRunMaxHealthBonus(this, amount))
        {
            return false;
        }

        maxHealth = combatStats.CoreTowerMaxHealth;
        currentHealth += amount;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        Debug.Log(
            $"CoreTowerHealth: Run HP upgrade +{amount}. " +
            $"HP: {currentHealth}/{maxHealth}.",
            this
        );

        return true;
    }

    internal bool CanReceiveHealthUpgradeFrom(CombatStats source)
    {
        return isInitialized && !isDestroyed && combatStats == source;
    }

    private void DestroyCoreTower()
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

        Debug.Log("CORE TOWER DESTROYED!", this);

        Destroyed?.Invoke(this);

        if (gameManager != null)
        {
            gameManager.GameOver();
        }

        Destroy(gameObject);
    }

    [ContextMenu("Test Damage 100")]
    private void TestDamage()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TakeDamage(100);
    }

    [ContextMenu("Test Health/Add 50 Run Max HP")]
    private void TestAdd50RunMaxHP()
    {
        if (!Application.isPlaying)
            return;

        TryAddRunMaxHealth(50);
    }

    [ContextMenu("Test Health/Print Health")]
    private void TestPrintHealth()
    {
        if (!Application.isPlaying)
            return;

        Debug.Log(
            $"CoreTowerHealth: HP = {currentHealth}/{maxHealth}. " +
            $"IsDestroyed = {isDestroyed}.",
            this
        );
    }
}
