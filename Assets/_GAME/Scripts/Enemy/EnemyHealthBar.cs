using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public EnemyHealth enemyHealth;

    void Awake()
    {
        // Tự động tìm nếu quên kéo-thả trong Inspector
        if (healthSlider == null) healthSlider = GetComponentInChildren<Slider>();
        if (enemyHealth == null) enemyHealth = GetComponentInParent<EnemyHealth>();

        if (healthSlider == null)
            Debug.LogWarning(gameObject.name + ": Chưa gán Health Slider!", this);
        if (enemyHealth == null)
            Debug.LogWarning(gameObject.name + ": Chưa gán Enemy Health (không tìm thấy component EnemyHealth ở object cha)!", this);
    }

    void Update()
    {
        // Liên tục cập nhật thanh Slider dựa trên máu thực tế của con gấu
        if (enemyHealth != null && healthSlider != null)
        {
            healthSlider.value = enemyHealth.currentHealth / enemyHealth.maxHealth;
        }
    }

    // Ép thanh máu luôn xoay mặt về phía Camera (Người chơi)
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}