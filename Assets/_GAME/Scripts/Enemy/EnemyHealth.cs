using UnityEngine;
public class EnemyHealth : MonoBehaviour
{

    public float maxHealth = 100f;
    public float currentHealth;

    private EnemyAI enemyAI;

    void Start()
    {
        currentHealth = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
    }

    // Vũ khí của người chơi sẽ gọi hàm này khi đánh trúng
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " bị chém trúng! Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã gục ngã!");

        // PERSONA: tiêu diệt thành công 1 quái -> +1 Điểm Sinh Tồn
        if (PersonaManager.Instance != null)
        {
            PersonaManager.Instance.AwardPoint(1, "Tiêu diệt quái");
        }

        // Tắt AI để gấu ngừng đuổi đánh
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }



        // Bật animation Die của gấu lên nên sẽ tắt mấy animation kia đi
        if (enemyAI != null && enemyAI.anim != null)
        {
            enemyAI.anim.SetBool("atk", false);
            enemyAI.anim.SetBool("run", false);
            enemyAI.anim.SetBool("walk", false);
            enemyAI.anim.SetTrigger("die");
        }

        // Hẹn giờ 3 giây (chờ animation ngã xuống) rồi gọi hàm rớt thịt và xóa sổ gấu
        Invoke("DropMeatAndDestroy", 3f);
    }

    // Rớt thịt và xóa sổ con gấu
    public void DropMeatAndDestroy()
    {
        // 1. Tải prefab từ thư mục Resources
        GameObject bearMeatPrefab = ResourceCache.Load("bearmeat");

        if (bearMeatPrefab != null)
        {
            // Nhích tọa độ Y lên một chút (VD: 0.5f) để miếng thịt không bị lún xuống đất
            Vector3 spawnPosition = transform.position + new Vector3(0, 0.5f, 0);

            // Sinh ra miếng thịt
            Instantiate(bearMeatPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("Đã rớt thịt gấu ra sàn!");
        }
        else
        {
            Debug.LogError("Lỗi: Không tìm thấy file 'bearmeat' trong thư mục Resources. Bác kiểm tra lại tên file nhé!");
        }

        // 2. Xóa sổ con gấu ngay lập tức khỏi Scene
        Destroy(gameObject);
    }
}