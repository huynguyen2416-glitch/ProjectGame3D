using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
	public float maxHealth = 50f; // Máu của gấu
	public float currentHealth;
	public HealthBar healthBarUI; // Tham chiếu đến thanh máu lơ lửng của gấu

	void Start()
	{
		currentHealth = maxHealth;
		if (healthBarUI != null)
			healthBarUI.UpdateHealth(currentHealth, maxHealth);
	}

	public void TakeDamage(float amount)
	{
		currentHealth -= amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

		if (healthBarUI != null)
			healthBarUI.UpdateHealth(currentHealth, maxHealth);

		if (currentHealth <= 0)
		{
			Die();
		}
	}

	void Die()
	{
		Debug.Log("Gấu đã bị tiêu diệt!");
		// Chạy animation chết hoặc xóa gấu khỏi Scene
		// Destroy(gameObject, 2f); 
	}
}