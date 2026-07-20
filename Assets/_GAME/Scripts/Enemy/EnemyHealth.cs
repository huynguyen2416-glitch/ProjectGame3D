using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{

    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Độ khó leo thang")]
    [Tooltip("Cứ mỗi ngày sống sót, máu tối đa của quái tăng thêm % này. VD 0.1 = +10%/ngày. (Không áp dụng nếu Is Boss = true)")]
    public float healthScalePerDay = 0.1f;

    [Header("Chế độ Boss (tick nếu đây là Boss cuối game)")]
    [Tooltip("Nếu bật: quái này KHÔNG scale máu theo ngày, chết sẽ kích hoạt THẮNG GAME thay vì rớt thịt/biến mất bình thường.")]
    public bool isBoss = false;
    [Range(0f, 1f)]
    [Tooltip("Chỉ dùng khi Is Boss = true. Ngưỡng %% máu để chuyển sang Phase 2 (BossAI lắng nghe qua OnPhase2Entered).")]
    public float phase2Threshold = 0.5f;
    [HideInInspector]
    public bool isCorrupted = false;// gấu điên
    public bool IsPhase2 { get; private set; }
    // BossAI đăng ký lắng nghe sự kiện này để đổi tốc độ/kiểu tấn công khi máu xuống thấp
    public event Action OnPhase2Entered;

    private EnemyAI enemyAI;
    private BossAI bossAI;

    void Start()
    {
        if (!isBoss)
        {
            float dayScale = 1f;
            if (LightingManager.Instance != null)
            {
                dayScale = 1f + LightingManager.Instance.daysSurvived * healthScalePerDay;
            }
            maxHealth *= dayScale;
        }

        currentHealth = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
        bossAI = GetComponent<BossAI>();
    }

    // Vũ khí của người chơi sẽ gọi hàm này khi đánh trúng
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " bị chém trúng! Máu còn: " + currentHealth);

        if (isBoss && !IsPhase2 && currentHealth <= maxHealth * phase2Threshold)
        {
            IsPhase2 = true;
            Debug.Log("[Boss] BƯỚC VÀO PHASE 2!");
            OnPhase2Entered?.Invoke();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã gục ngã!");

        PlayDieAnimationAndDisableAI();

        if (isBoss)
        {
            DieAsBoss();
            return;
        }

        // PERSONA: tiêu diệt thành công 1 quái -> +1 Điểm Sinh Tồn
        if (PersonaManager.Instance != null)
        {
            PersonaManager.Instance.AwardPoint(1, "Tiêu diệt quái");
        }

        // Hẹn giờ 3 giây (chờ animation ngã xuống) rồi gọi hàm rớt thịt và xóa sổ gấu
        Invoke("DropMeatAndDestroy", 3f);
    }

    private void DieAsBoss()
    {
        Debug.Log("[Boss] ĐÃ BỊ ĐÁNH BẠI! THỜI GIAN ĐƯỢC GIẢI PHÓNG!");

        if (PersonaManager.Instance != null)
        {
            PersonaManager.Instance.AwardPoint(5, "Đánh bại Boss");
        }

        // Gọi LightingManager để mở lại thời gian, để người chơi sống nốt chờ bình minh lên
        if (LightingManager.Instance != null)
        {
            LightingManager.Instance.OnBossDefeated();
        }
        else
        {
            Debug.LogError("[EnemyHealth]: Không tìm thấy LightingManager.Instance!");
        }
    }

    // Dùng chung cho cả quái thường (EnemyAI) và Boss (BossAI) - tắt AI, bật animation "die"
    private void PlayDieAnimationAndDisableAI()
    {
        Animator animRef = null;

        if (enemyAI != null)
        {
            enemyAI.enabled = false;
            animRef = enemyAI.anim;
        }

        if (bossAI != null)
        {
            bossAI.enabled = false;
            animRef = bossAI.anim;
        }

        if (animRef != null)
        {
            animRef.SetBool("atk", false);
            animRef.SetBool("run", false);
            animRef.SetBool("walk", false);
            animRef.SetTrigger("die");
        }
    }

    // Rớt thịt và xóa sổ con gấu (chỉ dùng cho quái thường, KHÔNG gọi cho Boss)
    public void DropMeatAndDestroy()
    {
        // Chỉ rớt thịt nếu KHÔNG bị Boss taunt
        if (!isCorrupted)
        {
            GameObject bearMeatPrefab = ResourceCache.Load("bearmeat");
            if (bearMeatPrefab != null)
            {
                Vector3 spawnPosition = transform.position + new Vector3(0, 0.5f, 0);
                Instantiate(bearMeatPrefab, spawnPosition, Quaternion.identity);
                Debug.Log("Đã rớt thịt gấu ra sàn!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy prefab 'bearmeat' trong thư mục Resources.");
            }
        }
        else
        {
            Debug.Log(gameObject.name + " là gấu đang chiến đấu boss, không rớt thịt!");
        }

        // Xóa sổ gấu
        Destroy(gameObject);
    }
}