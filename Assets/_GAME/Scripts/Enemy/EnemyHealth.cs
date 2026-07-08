using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
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

        // Tắt AI để gấu ngừng đuổi đánh
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        // Tắt Collider để không cản đường người chơi nữa
        Collider coll = GetComponent<Collider>();
        if (coll != null) coll.enabled = false;

        // Bật animation chết
        if (enemyAI != null && enemyAI.anim != null)
        {
            enemyAI.anim.SetBool("atk", false);
            enemyAI.anim.SetBool("run", false);
            enemyAI.anim.SetBool("walk", false);

            // DÒNG NÀY ĐÃ ĐƯỢC MỞ KHÓA ĐỂ GỌI HOẠT ẢNH CHẾT
            enemyAI.anim.SetTrigger("die");
        }

        // Dọn dẹp xác gấu sau 4 giây (chờ animation chết chạy xong)
        Destroy(gameObject, 4f);
    }
}