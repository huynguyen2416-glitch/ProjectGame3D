using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Campfire : MonoBehaviour
{
    [Header("--- Chỉ số Lửa ---")]
    public float fireDamagePerSecond = 15f; // Sát thương khi dẫm thẳng vào lửa
    public float burnRadius = 1.2f;         // Khoảng cách bị bỏng 

    [Header("--- Persona ---")]
    public bool isPlayerBuilt = false;

    [Header("--- Đun nước tinh khiết ---")]
    [Tooltip("Tên item nước THÔ cần tiêu thụ (phải khớp tên item trong balo, VD lấy từ WaterSource.cs)")]
    public string rawWaterItemName = "water";

    [Tooltip("Tên item nước TINH KHIẾT sẽ nhận được sau khi đun xong (cấu hình prefab này với " +
             "isConsumable = true, hydrationEffect > 0 để uống được)")]
    public string purifiedWaterItemName = "purewater";

    [Tooltip("Phím tương tác để đun nước khi đang đứng cạnh lửa trại")]
    public KeyCode boilWaterKey = KeyCode.E;

    [Tooltip("Thời gian chờ giữa 2 lần đun liên tiếp (giây), tránh spam phím ra nước ồ ạt")]
    public float boilCooldown = 1f;

    private float boilTimer;

    private bool hasAwardedPoint = false;
    private bool isPlayerNearby = false;

    // khoảng cách khi bị đốt
    private float burnRadiusSqr;
    private Transform cachedPlayerTransform;
    private PlayerState cachedPlayerState;
    private Collider triggerCollider;

    public float lifetime = 20f;
    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        burnRadiusSqr = burnRadius * burnRadius;
        if (isPlayerBuilt && !hasAwardedPoint && PersonaManager.Instance != null)
        {
            hasAwardedPoint = true;
            PersonaManager.Instance.AwardPoint(1, "Xây lửa trại");// kích hoạt nhận điểm
        }
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (PlayerState.Instance != null)
            {
                cachedPlayerState = PlayerState.Instance;
                if (cachedPlayerState.playerBody != null)
                {
                    cachedPlayerTransform = cachedPlayerState.playerBody.transform;
                }

                cachedPlayerState.SetNearCampfire(true);
            }

            if (SelectionManager.Instance != null) SelectionManager.Instance.RegisterCampfire(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (cachedPlayerState != null)
            {
                cachedPlayerState.SetNearCampfire(false);
            }

            cachedPlayerTransform = null;
            cachedPlayerState = null;

            if (SelectionManager.Instance != null) SelectionManager.Instance.UnregisterCampfire(this);
        }
    }

    private void Update()
    {
        if (isPlayerNearby && cachedPlayerState != null && cachedPlayerTransform != null)
        {
            Vector3 offset = transform.position - cachedPlayerTransform.position;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance <= burnRadiusSqr)
            {
                float currentHp = cachedPlayerState.currentHealth;
                cachedPlayerState.setHealth(currentHp - fireDamagePerSecond * Time.deltaTime);
            }
        }

        // --- Đun nước tinh khiết: chỉ hoạt động khi người chơi đang đứng cạnh lửa trại ---
        if (isPlayerNearby)
        {
            if (boilTimer > 0f)
            {
                boilTimer -= Time.deltaTime;
            }
            else if (Input.GetKeyDown(boilWaterKey))
            {
                TryBoilWater();
            }
        }
    }

    private void TryBoilWater()
    {
        if (InventorySystem.Instance == null) return;

        bool isUIOpen = InventorySystem.Instance.isOpen
                      || (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen);
        if (isUIOpen) return;

        if (CountItemInInventory(rawWaterItemName) <= 0)
        {
            Debug.Log("[Campfire]: Không có nước thô trong balo để đun!");
            return;
        }

        // Trừ đúng 1 nước thô, nhận lại đúng 1 nước tinh khiết
        InventorySystem.Instance.RemoveItemAmount(rawWaterItemName, 1);
        InventorySystem.Instance.AddToInventory(purifiedWaterItemName);

        boilTimer = boilCooldown;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.craftingSound);
        }

        Debug.Log("[Campfire]: Đã đun 1 phần nước thô thành nước tinh khiết!");
    }

    private int CountItemInInventory(string itemName)
    {
        int count = 0;
        foreach (string item in InventorySystem.Instance.itemList)
        {
            if (item == itemName) count++;
        }
        return count;
    }

    // Cho SelectionManager.cs đọc ra để hiển thị đúng chữ nhắc UI (có nước để đun hay chỉ đơn
    // thuần đang sưởi ấm) mà không cần lặp lại logic đếm inventory ở nơi khác.
    public int RawWaterCount => InventorySystem.Instance != null ? CountItemInInventory(rawWaterItemName) : 0;

    private void OnDestroy()
    {
        if (isPlayerNearby && cachedPlayerState != null)
        {
            cachedPlayerState.SetNearCampfire(false);
        }

        if (SelectionManager.Instance != null) SelectionManager.Instance.UnregisterCampfire(this);
    }
}