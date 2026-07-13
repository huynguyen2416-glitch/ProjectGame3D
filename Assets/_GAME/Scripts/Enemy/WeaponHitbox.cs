using UnityEngine;

// Gắn script này lên CHÍNH model 3D của rìu/cuốc (object con nằm trong tay nhân vật),
// không gắn lên tay hay lên Player. Object này cần có Collider (BoxCollider) — script tự bật Is Trigger.
[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
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

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            hasHitThisSwing = true;
            return;
        }

        ChoppableTree tree = other.GetComponentInParent<ChoppableTree>();
        if (tree != null)
        {
            tree.GetHit();
            hasHitThisSwing = true;
            return;
        }

        MineableRock rock = other.GetComponentInParent<MineableRock>();
        if (rock != null)
        {
            rock.GetHit();
            hasHitThisSwing = true;
            return;
        }
    }
}