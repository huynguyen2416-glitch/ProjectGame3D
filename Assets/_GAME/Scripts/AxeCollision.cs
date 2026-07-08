using UnityEngine;

public class AxeCollision : MonoBehaviour
{
	[Header("Cài đặt Nhân vật")]
	[Tooltip("Kéo Animator của nhân vật vào đây, hoặc script sẽ tự tìm")]
	public Animator playerAnimator;

	[Header("Cài đặt Hiệu ứng / Vật thể")]
	public GameObject hitPrefab;

	[Header("Cài đặt Sát thương")]
	public float damage = 20f;

	private bool canDealDamage = true;

	void Start()
	{
		// Tự động tìm Animator ở các Object cha nếu bạn quên chưa kéo thả trong Inspector
		if (playerAnimator == null)
		{
			playerAnimator = GetComponentInParent<Animator>();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		// 1. Bỏ qua nếu rìu chạm trúng chính mình
		if (other.gameObject.name == "zombieApocalypseMan" || other.CompareTag("Player"))
			return;

		// 2. KIỂM TRA TRẠNG THÁI VUNG RÌU
		if (playerAnimator != null)
		{
			// "slash" là tên của KHỐI TRẠNG THÁI (chữ thường) trong bảng Animator của bạn
			if (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("slash"))
			{
				return; // Nếu KHÔNG PHẢI đang chạy animation chém -> Thoát ngay, không trừ máu
			}
		}

		// 3. Xử lý trừ máu khi đã thỏa mãn điều kiện vung rìu
		if (other.CompareTag("Enemy") && canDealDamage)
		{
			//EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

			//if (enemyHealth != null)
			{
				//enemyHealth.TakeDamage(damage);
			}

			if (hitPrefab != null)
			{
				Vector3 hitPoint = other.ClosestPoint(transform.position);
				GameObject spawnedObject = Instantiate(hitPrefab, hitPoint, Quaternion.identity);
				Destroy(spawnedObject, 2f);
			}

			canDealDamage = false;
			Invoke(nameof(ResetDamage), 0.5f);
		}
	}

	private void ResetDamage()
	{
		canDealDamage = true;
	}
}