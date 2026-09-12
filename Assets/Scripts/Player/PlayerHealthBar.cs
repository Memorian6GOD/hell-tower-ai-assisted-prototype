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
            Debug.LogError("PlayerHealthBar: PlayerHealth не назначен.");
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogError("PlayerHealthBar: Health Slider не назначен.");
            return;
        }

        healthSlider.minValue = 0;
        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.CurrentHealth;
    }

    private void Update()
    {
        if (playerHealth == null || healthSlider == null)
        {
            return;
        }

        healthSlider.value = playerHealth.CurrentHealth;
    }
}