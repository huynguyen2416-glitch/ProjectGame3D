using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EquipableItem : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        bool axeActive = WeaponHolder.Instance != null && WeaponHolder.Instance.realAxeInHand != null && WeaponHolder.Instance.realAxeInHand.activeSelf;
        bool pickaxeActive = WeaponHolder.Instance != null && WeaponHolder.Instance.realPickaxeInHand != null && WeaponHolder.Instance.realPickaxeInHand.activeSelf;

        if (!axeActive && !pickaxeActive)
        {
            return; // Không cầm rìu cũng không cầm cuốc -> bấm chuột trái sẽ vô tác dụng
        }

        // Chỉ khi có rìu hoặc cuốc trên tay, đoạn code bấm chuột này mới được chạy:
        if (Input.GetMouseButtonDown(0) && // Click chuột trái
            InventorySystem.Instance.isOpen == false &&
            CraftingSystem.Instance.isOpen == false &&
            SelectionManager.Instance.handIsVisible == false)
        {
            // Bắt đầu vung tay -> Bật animation
            animator.SetTrigger("hit");

            bool didHitSomething = false; // Biến cờ đánh dấu xem có chém/đập trúng gì không

            // ==========================================
            // CHỈ KHI CẦM RÌU: CHẶT CÂY + CHÉM GẤU
            // ==========================================
            if (axeActive)
            {
                // 1. KIỂM TRA CHẶT CÂY (Dùng selectedTree)
                GameObject tree = SelectionManager.Instance.selectedTree;
                if (tree != null)
                {
                    tree.GetComponent<ChoppableTree>().GetHit();
                    didHitSomething = true;
                }

                // 2. KIỂM TRA CHÉM GẤU (Dùng selectedObject)
                GameObject hitObject = SelectionManager.Instance.selectedObject;
                if (hitObject != null)
                {
                    EnemyHealth enemy = hitObject.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(25f);
                        didHitSomething = true;
                    }
                }
            }

            // ==========================================
            // CHỈ KHI CẦM CUỐC: ĐẬP ĐÁ (Dùng selectedRock)
            // ==========================================
            if (pickaxeActive)
            {
                GameObject rock = SelectionManager.Instance.selectedRock;
                if (rock != null)
                {
                    rock.GetComponent<MineableRock>().GetHit();
                    didHitSomething = true;
                }
            }

            // ==========================================
            // PHÁT ÂM THANH DỰA TRÊN KẾT QUẢ CHÉM/ĐẬP
            // ==========================================
            if (SoundManager.Instance != null)
            {
                if (didHitSomething)
                {
                    // Trúng (Cây, Gấu hoặc Đá) -> Kêu tiếng Cộp / Phập
                    SoundManager.Instance.PlaySound(SoundManager.Instance.chopSound);
                }
                else
                {
                    // Hụt (Vào không khí) -> Kêu tiếng Vút
                    SoundManager.Instance.PlaySound(SoundManager.Instance.toolSwingSound);
                }
            }
        }
    }
}