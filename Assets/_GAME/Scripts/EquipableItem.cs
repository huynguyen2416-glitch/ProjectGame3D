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
    public float hitDelay = 0.3f; // THÊM BIẾN NÀY ĐỂ CANH CHUẨN THỜI GIAN

    private float lastAttackTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (WeaponHolder.Instance == null || WeaponHolder.Instance.realAxeInHand == null || !WeaponHolder.Instance.realAxeInHand.activeSelf)
        {
<<<<<<< HEAD
            return;
        }

        if (Input.GetMouseButtonDown(0) &&
            Time.time >= lastAttackTime + attackCooldown &&
=======
            return; // Thoát hàm Update luôn, bấm chuột trái sẽ vô tác dụng
        }

        // Chỉ khi có rìu trên tay, đoạn code bấm chuột này mới được chạy:
        if (Input.GetMouseButtonDown(0) && // Click chuột trái
>>>>>>> parent of a57ad63 (tạm thời như v)
            InventorySystem.Instance.isOpen == false &&
            CraftingSystem.Instance.isOpen == false &&
            SelectionManager.Instance.handIsVisible == false)
        {
<<<<<<< HEAD
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
=======
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
>>>>>>> parent of a57ad63 (tạm thời như v)
            }
        }
    }
}