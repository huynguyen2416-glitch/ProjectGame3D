using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
	public float maxHealth = 100f;
	public float currentHealth;
	public HealthBar healthBarUI;

	void Start()
	{
		currentHealth = maxHealth;
		// Gửi giá trị ban đầu cho UI
		healthBarUI.UpdateHealth(currentHealth, maxHealth);
	}

	public void TakeDamage(float amount)
	{
		currentHealth -= amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

		// Cập nhật lại UI mỗi khi nhân vật Elaina bị thương
		healthBarUI.UpdateHealth(currentHealth, maxHealth);
	}
}