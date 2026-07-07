using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
	public float maxHealth = 50f;
	public float currentHealth;
	public HealthBar healthBarUI;

	[Header("Cài đặt Animation")]
	[Tooltip("Kéo Animator của con gấu vào đây")]
	public Animator bearAnimator;

	// Biến này để đảm bảo gấu chỉ chết 1 lần (tránh chém bồi vào xác gấu bị lỗi)
	private bool isDead = false;

	void Start()
	{
		currentHealth = maxHealth;
		if (healthBarUI != null)
			healthBarUI.UpdateHealth(currentHealth, maxHealth);

		// Tự động tìm Animator nếu bạn quên kéo thả
		if (bearAnimator == null)
			bearAnimator = GetComponent<Animator>();
	}

	public void TakeDamage(float amount)
	{
		// Nếu gấu đã chết rồi thì không trừ máu hay chạy hiệu ứng nữa
		if (isDead) return;

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
		isDead = true;
		Debug.Log("Gấu đã bị tiêu diệt!");

		// 1. Ẩn thanh máu
		if (healthBarUI != null)
		{
			healthBarUI.gameObject.SetActive(false);
		}

		// 2. CHẠY ANIMATION CHẾT
		if (bearAnimator != null)
		{
			bearAnimator.SetTrigger("Die");
		}

		// 3. Xóa gấu sau 2.5 giây để khớp với thời gian ngã xuống
		Destroy(gameObject, 2.5f);
	}
}