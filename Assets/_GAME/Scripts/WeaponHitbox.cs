using UnityEngine;


[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    //Tạo danh sách phân loại để chọn trong Inspector
    public enum ToolType { Axe, Pickaxe }

    [Header("Phân loại dụng cụ")]
    [Tooltip("Chọn đúng loại cho prefab vũ khí (Rìu chọn Axe, Cuốc chọn Pickaxe)")]
    public ToolType toolType;

    [Tooltip("Sát thương gây cho quái khi lưỡi rìu/cuốc chạm trúng")]
    public float damage = 25f;

    private bool isSwinging;
    private bool hasHitThisSwing;

    public bool HasHitThisSwing => hasHitThisSwing;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Gọi lúc bắt đầu 1 nhát vung — mở "cửa sổ" cho phép gây damage
    public void StartSwing()
    {
        isSwinging = true;
        hasHitThisSwing = false;
    }

    // Gọi lúc kết thúc nhát vung — đóng lại, tránh 1 nhát chạm liên tục nhiều lần
    public void EndSwing()
    {
        isSwinging = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chưa trong lúc vung, hoặc nhát này đã tính 1 lần chạm rồi -> bỏ qua
        if (!isSwinging || hasHitThisSwing) return;

        // 1. CẢ RÌU VÀ CUỐC ĐỀU ĐÁNH ĐƯỢC QUÁI (Giữ nguyên logic của bác)
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            hasHitThisSwing = true;
            return;
        }

        // 2. CHỈ RÌU (AXE) MỚI ĐƯỢC PHÉP CHẶT CÂY
        if (toolType == ToolType.Axe)
        {
            ChoppableTree tree = other.GetComponentInParent<ChoppableTree>(); 
            if (tree != null)
            {
                tree.GetHit(); 
                hasHitThisSwing = true;
                return;
            }
        }

        // 3. CHỈ CUỐC (PICKAXE) MỚI ĐƯỢC PHÉP ĐẬP ĐÁ
        if (toolType == ToolType.Pickaxe)
        {
            MineableRock rock = other.GetComponentInParent<MineableRock>();
            if (rock != null)
            {
                rock.GetHit();
                hasHitThisSwing = true;
                return;
            }
        }
    }
}