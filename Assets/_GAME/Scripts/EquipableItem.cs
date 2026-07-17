using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EquipableItem : MonoBehaviour
{
    public Animator animator;

    [Header("Combat Settings")]
    public float attackCooldown = 0.8f;

    [Tooltip("Thời gian chờ từ lúc bấm chuột đến lúc lưỡi rìu chạm mục tiêu (giây)")]
    public float hitDelay = 0.3f;

    private float lastAttackTime = 0f;

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
            return;
        }

        if (Input.GetMouseButtonDown(0) &&
            Time.time >= lastAttackTime + attackCooldown &&
            InventorySystem.Instance.isOpen == false &&
            CraftingSystem.Instance.isOpen == false &&
            SelectionManager.Instance.handIsVisible == false)
        {
            lastAttackTime = Time.time;

            // TỰ ĐỘNG XOAY NGƯỜI HƯỚNG VÀO MỤC TIÊU ---
            GameObject targetToLook = null;
            if (axeActive && SelectionManager.Instance.selectedTree != null)
            {
                targetToLook = SelectionManager.Instance.selectedTree;
            }
            else if (pickaxeActive && SelectionManager.Instance.selectedRock != null)
            {
                targetToLook = SelectionManager.Instance.selectedRock;
            }

            if (targetToLook != null)
            {
                // Tìm Root Player (Object chứa PlayerMovement) để xoay toàn bộ cơ thể
                PlayerMovement playerMove = GetComponentInParent<PlayerMovement>();
                if (playerMove != null)
                {
                    Vector3 targetPos = targetToLook.transform.position;
                    // Giữ nguyên trục Y của người chơi để không bị ngửa mặt lên trời hay cúi gằm xuống đất
                    Vector3 lookPos = new Vector3(targetPos.x, playerMove.transform.position.y, targetPos.z);
                    playerMove.transform.LookAt(lookPos);
                }
            }
            // ----------------------------------------------------------------

            // 1. Chỉ gọi Animation vung tay
            animator.SetTrigger("hit");

            // 2. Bắt đầu bộ đếm ngược để nổ sát thương (Hàm Coroutine bác đang dùng)
            StartCoroutine(ExecuteHit(hitDelay, axeActive, pickaxeActive));
        }
    }

    // HÀM HẸN GIỜ ĐỂ CHỜ LƯỠI RÌU CHẠM ĐÍCH
    private IEnumerator ExecuteHit(float delay, bool isAxe, bool isPickaxe)
    {
        // Chờ đúng khoảng thời gian rìu vung từ trên cao xuống
        yield return new WaitForSeconds(delay);

        GameObject activeWeapon = isAxe ? WeaponHolder.Instance.realAxeInHand : WeaponHolder.Instance.realPickaxeInHand;
        WeaponHitbox hitbox = activeWeapon != null ? activeWeapon.GetComponentInChildren<WeaponHitbox>() : null;

        if (hitbox == null)
        {
            Debug.LogWarning("[EquipableItem]: Không tìm thấy WeaponHitbox trên vũ khí đang cầm — cần gắn script này + BoxCollider lên model rìu/cuốc.");
            yield break;
        }

        hitbox.StartSwing();

        // Giữ "cửa sổ" va chạm mở trong khoảng thời gian ngắn tương ứng lúc lưỡi rìu thực sự quét qua mục tiêu
        yield return new WaitForSeconds(0.15f);

        hitbox.EndSwing();

        // Phát âm thanh dựa trên việc WeaponHitbox có thực sự chạm trúng gì không
        if (SoundManager.Instance != null)
        {
            if (hitbox.HasHitThisSwing)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.chopSound);
            }
            else
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.toolSwingSound);
            }
        }
    }
}