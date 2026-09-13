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
}