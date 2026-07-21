using UnityEngine;

// Gắn script này lên 1 GameObject có Collider đánh dấu "vùng lấy nước" (ao, giếng, sông...).
// Không dùng chung luồng InteractableObject.cs/SelectionManager vì luồng đó Destroy() vật thể sau
// đúng 1 lần nhặt - còn vùng lấy nước cần TỒN TẠI MÃI và cho lấy NHIỀU LẦN (có giới hạn theo ngày).
[RequireComponent(typeof(Collider))]
public class WaterSource : MonoBehaviour
{
    [Header("--- Vùng lấy nước ---")]
    [Tooltip("Tên item nước thô sẽ thêm vào balo (phải trùng tên Prefab UI trong thư mục Resources)")]
    public string waterItemName = "water";

    [Tooltip("Số lần được lấy nước tối đa trong 1 ngày - tự động làm mới mỗi khi trời sáng " +
             "(LightingManager.OnDawn)")]
    public int maxUsesPerDay = 3;

    private int usesRemainingToday;

    // Public để SelectionManager.cs đọc được (giống playerInRange của ChoppableTree/MineableRock) -
    // đây là điều kiện để SelectionManager hiện chữ nhắc "Nhấn F để lấy nước" lên UI khi ngắm trúng.
    public bool playerInRange;

    // Cho UI khác đọc ra để hiển thị "còn x/y lượt hôm nay"
    public int UsesRemainingToday => usesRemainingToday;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null) triggerCollider.isTrigger = true;

        usesRemainingToday = maxUsesPerDay;
    }

    private void OnEnable()
    {
        LightingManager.OnDawn += ResetDailyUses;
    }

    private void OnDisable()
    {
        LightingManager.OnDawn -= ResetDailyUses;
        if (playerInRange && SelectionManager.Instance != null)
        {
            SelectionManager.Instance.UnregisterWaterSource(this);
        }
    }

    private void ResetDailyUses()
    {
        usesRemainingToday = maxUsesPerDay;
        Debug.Log($"[WaterSource]: Trời đã sáng, làm mới lượt lấy nước hôm nay ({maxUsesPerDay} lượt).");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (SelectionManager.Instance != null) SelectionManager.Instance.RegisterWaterSource(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (SelectionManager.Instance != null) SelectionManager.Instance.UnregisterWaterSource(this);
    }

    // Được SelectionManager.cs gọi khi người chơi đang ngắm trúng vùng nước (đã hiện chữ nhắc lên
    // UI) và bấm phím F. Trả về true/false để nơi gọi biết có lấy thành công hay không (VD để phát
    // thêm hiệu ứng/âm thanh khác nếu muốn).
    public bool TryCollectWater()
    {
        if (usesRemainingToday <= 0)
        {
            Debug.Log("[WaterSource]: Đã hết lượt lấy nước hôm nay, quay lại vào ngày mai!");
            return false;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("[WaterSource]: Không tìm thấy InventorySystem.Instance trong scene!");
            return false;
        }

        InventorySystem.Instance.AddToInventory(waterItemName);
        usesRemainingToday--;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.pickupItemSound);
        }

        Debug.Log($"[WaterSource]: Đã lấy 1 phần nước thô. Còn lại {usesRemainingToday}/{maxUsesPerDay} lượt hôm nay.");
        return true;
    }
}