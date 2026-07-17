using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    private Slider slider;
    public Text healthCounter;
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        // The player can be unavailable while scenes are changing.
        if (PlayerState.Instance == null) return;
        float currentHealth = PlayerState.Instance.currentHealth;
        float maxHealth = PlayerState.Instance.maxHealth;
        float fillValue = currentHealth / maxHealth;
        slider.value = fillValue;

        if (healthCounter != null)
        {
            healthCounter.text = Mathf.CeilToInt(currentHealth) + "/" + Mathf.RoundToInt(maxHealth);
        }
    }
}