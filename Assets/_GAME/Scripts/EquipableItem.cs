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
        // --- ĐOẠN CODE THÊM MỚI BẢO VỆ ĐẦU GAME ---
        // Kiểm tra nếu hệ thống quản lý vũ khí chưa sẵn sàng, 
        // hoặc cái Rìu Thật trên tay nhân vật ĐANG BỊ ẨN (chưa nhặt/chưa trang bị)
        // thì KHÔNG CHO CHẠY tiếp code chặt cây phía dưới.
        if (WeaponHolder.Instance == null || WeaponHolder.Instance.realAxeInHand == null || !WeaponHolder.Instance.realAxeInHand.activeSelf)
        {
            return; // Thoát hàm Update luôn, bấm chuột trái sẽ vô tác dụng
        }
        // ------------------------------------------

        // Chỉ khi có rìu trên tay, đoạn code bấm chuột này mới được chạy:
        if (Input.GetMouseButtonDown(0) && // Click chuột trái
            InventorySystem.Instance.isOpen == false &&
            CraftingSystem.Instance.isOpen == false &&
            SelectionManager.Instance.handIsVisible == false)
        {
            GameObject selectedTree = SelectionManager.Instance.selectedTree;

            if (selectedTree != null)
            {
                selectedTree.GetComponent<ChoppableTree>().GetHit();
            }

            // Kích hoạt animation vung rìu
            animator.SetTrigger("hit");
        }
    }
}