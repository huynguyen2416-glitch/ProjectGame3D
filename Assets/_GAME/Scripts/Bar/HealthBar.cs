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
<<<<<<< HEAD
        // 1. Bảo vệ lỗi rỗng để game không bị đứng nếu nhân vật lỡ bị xóa
        if (PlayerState.Instance == null) return;

        // 2. Lấy dữ liệu trực tiếp từ Singleton (Siêu nhẹ, không dùng GetComponent)
        float currentHealth = PlayerState.Instance.currentHealth;
        float maxHealth = PlayerState.Instance.maxHealth;
        // 3. Cập nhật UI
        float fillValue = currentHealth / maxHealth;
        slider.value = fillValue;

        if (healthCounter != null)
        {
            healthCounter.text = Mathf.CeilToInt(currentHealth) + "/" + Mathf.RoundToInt(maxHealth);
        }
=======
        currentHealth = playerState.GetComponent<PlayerState>().currentHealth;
        maxHealth = playerState.GetComponent<PlayerState>().maxHealth;

        float fillValue = currentHealth / maxHealth;
        slider.value = fillValue;

        healthCounter.text = currentHealth + "/" + maxHealth;
>>>>>>> parent of a57ad63 (tạm thời như v)
    }
}