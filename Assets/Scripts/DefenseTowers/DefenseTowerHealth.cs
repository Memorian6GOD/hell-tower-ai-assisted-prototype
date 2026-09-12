using System;
using UnityEngine;

public class DefenseTowerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 300;

    private int currentHealth;
    private bool isDestroyed;

    private DefenseTowerHealthBar healthBar;

    public event Action<DefenseTowerHealth> Destroyed;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDestroyed => isDestroyed;

    private void Start()
    {
        currentHealth = maxHealth;
        isDestroyed = false;

        healthBar =
            GetComponentInChildren<DefenseTowerHealthBar>(
                true
            );

        if (healthBar != null)
        {
            healthBar.SetHealth(
                currentHealth,
                maxHealth
            );

            healthBar.Show();
        }

        Debug.Log(
            gameObject.name +
            " health: " +
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
            gameObject.name +
            " health: " +
            currentHealth
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
            gameObject.name +
            " destroyed!"
        );

        Destroyed?.Invoke(this);

        Destroy(gameObject);
    }

    [ContextMenu("Test Damage 50")]
    private void TestDamage()
    {
        TakeDamage(50);
    }
}