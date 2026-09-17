using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Slider healthSlider;

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealthBar: PlayerHealth не назначен.", this);
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogError("PlayerHealthBar: Health Slider не назначен.", this);
            return;
        }

        healthSlider.minValue = 0;
        RefreshHealth();
    }

    private void Update()
    {
        RefreshHealth();
    }

    private void RefreshHealth()
    {
        if (playerHealth == null || healthSlider == null)
        {
            return;
        }

        // Сначала обновляем предел, затем заполняем полоску текущим HP.
        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.CurrentHealth;
    }
}
