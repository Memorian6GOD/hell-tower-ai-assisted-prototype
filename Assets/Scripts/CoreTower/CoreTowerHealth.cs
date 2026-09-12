using System;
using UnityEngine;

public class CoreTowerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 1000;

    [Header("UI")]
    [SerializeField] private CoreTowerHealthBar healthBar;

    private int currentHealth;
    private bool isDestroyed;

    private GameManager gameManager;

    public event Action<CoreTowerHealth> Destroyed;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDestroyed => isDestroyed;

    private void Start()
    {
        currentHealth = maxHealth;

        gameManager =
            FindAnyObjectByType<GameManager>();

        if (healthBar != null)
        {
            healthBar.Show();
            healthBar.SetHealth(
                currentHealth,
                maxHealth
            );
        }

        Debug.Log(
            "Core Tower health: " +
            currentHealth
        );
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDestroyed)
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

        if (healthBar != null)
        {
            healthBar.SetHealth(
                currentHealth,
                maxHealth
            );
        }

        Debug.Log(
            "Core Tower health: " +
            currentHealth
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

        Debug.Log("CORE TOWER DESTROYED!");

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
        TakeDamage(100);
    }
}