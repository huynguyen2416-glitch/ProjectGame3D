using System.Collections;
using UnityEngine;

// Gắn lên Player (cùng chỗ EquipableItem, CampfireBuilder). Xử lý bắn cung: bấm chuột trái bắn ngay
// (không tốn đạn), có cooldown giữa 2 phát, ngắm theo đúng điểm crosshair (giống cách SelectionManager
// đang chọn mục tiêu để cây/đá/quái được chọn khớp với những gì người chơi đang nhắm).
public class BowShooter : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Để trống thì tự GetComponent<Animator> trên chính object này")]
    public Animator animator;
    [Tooltip("Tên Trigger trong Animator Controller cho animation kéo dây/bắn cung")]
    public string shootTrigger = "shoot";
    [Tooltip("Thời gian chờ từ lúc bắt đầu animation đến lúc buông dây (mũi tên thực sự xuất hiện)")]
    public float shootDelay = 0.25f;

    [Header("Prefab mũi tên")]
    public GameObject arrowPrefab;

    [Tooltip("Điểm bắn ra - kéo 1 Transform đặt ở đầu cung (hoặc tay cầm cung) vào đây")]
    public Transform shootPoint;

    [Header("Thông số bắn")]
    public float arrowSpeed = 30f;
    public float attackCooldown = 0.6f;
    [Tooltip("Tầm xa tối đa dùng để tính điểm ngắm khi phía trước không có gì cản")]
    public float maxAimDistance = 100f;

    private float lastShootTime = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        bool bowActive = WeaponHolder.Instance != null && WeaponHolder.Instance.realBowInHand != null && WeaponHolder.Instance.realBowInHand.activeSelf;
        if (!bowActive) return;

        if (Input.GetMouseButtonDown(0) &&
            Time.time >= lastShootTime + attackCooldown &&
            InventorySystem.Instance.isOpen == false &&
            CraftingSystem.Instance.isOpen == false &&
            SelectionManager.Instance.handIsVisible == false)
        {
            lastShootTime = Time.time;

            // 1. Chỉ gọi Animation kéo dây/bắn ngay lập tức
            if (animator != null && !string.IsNullOrEmpty(shootTrigger))
            {
                animator.SetTrigger(shootTrigger);
            }

            // 2. Ghi nhận HƯỚNG NGẮM ngay lúc bấm chuột (không phải lúc mũi tên bay ra) - tránh
            // trường hợp người chơi xoay camera đi chỗ khác trong lúc animation đang chạy khiến
            // mũi tên bắn lệch hướng so với lúc bấm.
            Vector3 aimPointAtClickTime = GetAimPoint();

            // 3. Bắn tên thật sau khi animation kéo tới đúng khung hình buông dây
            StartCoroutine(SpawnArrowAfterDelay(shootDelay, aimPointAtClickTime));
        }
    }

    private IEnumerator SpawnArrowAfterDelay(float delay, Vector3 aimPoint)
    {
        yield return new WaitForSeconds(delay);

        if (arrowPrefab == null || shootPoint == null)
        {
            Debug.LogWarning("[BowShooter]: Chưa gán arrowPrefab hoặc shootPoint trong Inspector!");
            yield break;
        }

        Vector3 direction = (aimPoint - shootPoint.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(direction);


        Quaternion rotationOffset = Quaternion.Euler(90f, 0f, 0f);

        // Áp dụng góc xoay mới
        GameObject arrowGO = Instantiate(arrowPrefab, shootPoint.position, targetRotation * rotationOffset);
        Arrow arrow = arrowGO.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.Launch(direction * arrowSpeed);
        }
        else
        {
            Debug.LogWarning("[BowShooter]: Prefab mũi tên không có component Arrow!");
        }

        if (SoundManager.Instance != null)
        {
            // Tái dùng tạm âm thanh vung tay - đổi field khác nếu SoundManager có sẵn âm bắn cung riêng
            SoundManager.Instance.PlaySound(SoundManager.Instance.toolSwingSound);
        }
    }

    // Ngắm theo đúng điểm màn hình con trỏ đang đứng (giống hệt cách SelectionManager chọn mục tiêu)
    Vector3 GetAimPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        return ray.origin + ray.direction * maxAimDistance;
    }
}