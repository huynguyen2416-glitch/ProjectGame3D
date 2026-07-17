using UnityEngine;


[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    //tùy dụng cụ
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


    public void StartSwing()
    {
        isSwinging = true;
        hasHitThisSwing = false;
    }

 
    public void EndSwing()
    {
        isSwinging = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // mỗi lần vung dính 1 collider
        if (!isSwinging || hasHitThisSwing) return;

        // 2 vũ khí đều có thể gây sát thương
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            hasHitThisSwing = true;
            return;
        }

        // rìu thì chặt cây
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

        // cuốc thì đập đá
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