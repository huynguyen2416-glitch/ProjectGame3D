using UnityEngine;

// Gắn lên Prefab mũi tên. Cần có Rigidbody (Use Gravity tuỳ ý, KHÔNG tick Is Kinematic) +
// Collider để bắt va chạm. Nếu Collider để Is Trigger thì mục tiêu (quái) cần có Rigidbody
// để Unity nhận sự kiện OnTriggerEnter.
[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    public float damage = 20f;
    [Tooltip("Tự huỷ mũi tên sau chừng này giây nếu bay mãi không trúng gì, tránh rác trong Scene")]
    public float lifeTime = 8f;

    private Rigidbody rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Gọi từ BowShooter ngay sau khi Instantiate
    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity; // Unity cũ hơn 6 dùng rb.velocity, đổi lại nếu Editor báo lỗi dòng này
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider other)
    {
        if (hasHit) return; // Chỉ tính 1 lần trúng duy nhất, tránh gây damage 2 lần cùng 1 mũi tên
        hasHit = true;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Trúng bất kỳ thứ gì (quái, tường, đất...) đều dừng bay và biến mất sau va chạm
        if (rb != null) rb.linearVelocity = Vector3.zero;
        Destroy(gameObject, 0.05f);
    }
}