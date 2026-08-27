using UnityEngine;
using UnityEngine.UI;

public class CoreTowerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;

    public void SetHealth(int currentHealth, int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}