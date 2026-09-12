using UnityEngine;
using UnityEngine.UI;

public class DefenseTowerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.rotation =
                mainCamera.transform.rotation;
        }
    }

    public void SetHealth(
        int currentHealth,
        int maxHealth
    )
    {
        if (healthSlider == null)
        {
            return;
        }

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