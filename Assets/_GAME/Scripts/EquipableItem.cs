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
        if (WeaponHolder.Instance == null || WeaponHolder.Instance.realAxeInHand == null || !WeaponHolder.Instance.realAxeInHand.activeSelf)
        {
            return; // Thoát hàm Update luôn, bấm chuột trái sẽ vô tác dụng
        }

        // Chỉ khi có rìu trên tay, đoạn code bấm chuột này mới được chạy:
        if (Input.GetMouseButtonDown(0) && // Click chuột trái
            InventorySystem.Instance.isOpen == false &&
            CraftingSystem.Instance.isOpen == false &&
            SelectionManager.Instance.handIsVisible == false)
        {
            SelectionManager.Instance.RotatePlayerTowardsTree();
            // Bắt đầu vung rìu -> Bật animation
            animator.SetTrigger("hit");

            bool didHitSomething = false; // Biến cờ đánh dấu xem có chém trúng gì không

            // ==========================================
            // 1. KIỂM TRA CHẶT CÂY (Dùng selectedTree)
            // ==========================================
            GameObject tree = SelectionManager.Instance.selectedTree;
            if (tree != null)
            {
                tree.GetComponent<ChoppableTree>().GetHit();
                didHitSomething = true;
            }

            // ==========================================
            // 2. KIỂM TRA CHÉM GẤU (Dùng selectedObject)
            // ==========================================
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

            // ==========================================
            // 3. PHÁT ÂM THANH DỰA TRÊN KẾT QUẢ CHÉM
            // ==========================================
            if (SoundManager.Instance != null)
            {
                if (didHitSomething)
                {
                    // Chém TRÚNG (Cây hoặc Gấu) -> Kêu tiếng Cộp / Phập
                    SoundManager.Instance.PlaySound(SoundManager.Instance.chopSound);
                }
                else
                {
                    // Chém HỤT (Vào không khí) -> Kêu tiếng Vút
                    SoundManager.Instance.PlaySound(SoundManager.Instance.toolSwingSound);
                }
            }
        }
    }
}