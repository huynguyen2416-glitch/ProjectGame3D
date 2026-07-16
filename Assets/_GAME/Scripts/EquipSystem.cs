using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipSystem : MonoBehaviour
{
    public static EquipSystem Instance { get; set; }

    // -- UI -- //
    public GameObject quickSlotsPanel;

    public List<GameObject> quickSlotsList = new List<GameObject>();
    public List<string> itemList = new List<string>();

    // --- CẬP NHẬT: BIẾN THEO DÕI Ô ĐANG CHỌN TRÊN TAY ---
    public int activeSlotIndex = -1; // -1 nghĩa là chưa chọn ô nào cả

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        PopulateSlotList();
    }


    
    public void SetQuickSlotItem(int slotIndex, string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return;
        if (slotIndex < 0 || slotIndex >= quickSlotsList.Count) return;

        GameObject prefab = Resources.Load<GameObject>(itemName);
        if (prefab == null)
        {
            Debug.LogError($"[EquipSystem]: Không tìm thấy prefab '{itemName}' trong Resources để khôi phục Quick Slot {slotIndex + 1}!");
            return;
        }

        GameObject slot = quickSlotsList[slotIndex];
        GameObject instance = Instantiate(prefab, slot.transform);
        instance.name = prefab.name;

        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one; // Trước đây THIẾU dòng này -> icon dễ bị sai kích thước
        }
        else
        {
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
        }

        itemList.Add(itemName);
    }

    private void Update()
    {
        if (quickSlotsList == null) return;

        for (int i = 0; i < quickSlotsList.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectQuickSlot(i);
                break;
            }
        }
    }

    public void SelectQuickSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlotsList.Count) return;

        GameObject selectedSlot = quickSlotsList[slotIndex];

        if (selectedSlot == null)
        {
            Debug.LogError($"[EquipSystem]: Ô Quick Slot ở vị trí số {slotIndex + 1} đang bị NULL trong Inspector!");
            return;
        }

        // Ghi nhớ ô này đang được kích hoạt
        activeSlotIndex = slotIndex;

        if (selectedSlot.transform.childCount > 0)
        {
            Transform itemInSlot = selectedSlot.transform.GetChild(0);
            string cleanName = itemInSlot.gameObject.name.Replace("(Clone)", "");

            if (WeaponHolder.Instance != null)
            {
                WeaponHolder.Instance.EquipWeapon(cleanName);
                Debug.Log($"[EquipSystem]: Đã bấm phím số {slotIndex + 1} -> Trang bị: {cleanName}");
            }
            else
            {
                Debug.LogWarning($"[EquipSystem]: WeaponHolder.Instance đang NULL - không thể hiện vũ khí 3D '{cleanName}' trên tay. Kiểm tra WeaponHolder đã có trong scene chưa.");
            }
        }
        else
        {
            if (WeaponHolder.Instance != null)
            {
                WeaponHolder.Instance.UnquipAllWeapons();
                Debug.Log($"[EquipSystem]: Đã bấm phím số {slotIndex + 1} -> Ô trống, cất toàn bộ vũ khí!");
            }
        }
    }

    // =========================================================================
    // CHỐT CHẶN BẢO VỆ: Được gọi từ script Drop trước khi hủy vật phẩm UI
    // =========================================================================
    public void UnquipIfDropped(GameObject itemUI)
    {
        for (int i = 0; i < quickSlotsList.Count; i++)
        {
            // Tìm xem cái UI item bị drop có đang nằm trong ô Quickslot nào không
            if (quickSlotsList[i].transform.childCount > 0 && quickSlotsList[i].transform.GetChild(0).gameObject == itemUI)
            {
                // Nếu ô bị drop TRÙNG KHỚP với ô nhân vật đang cầm trên tay
                if (i == activeSlotIndex)
                {
                    if (WeaponHolder.Instance != null)
                    {
                        WeaponHolder.Instance.UnquipAllWeapons(); // Cất vũ khí 3D ngay lập tức
                        Debug.LogWarning("[EquipSystem]: Vật phẩm đang cầm trên tay bị Drop! Đã tự động ẩn vũ khí 3D để tránh lỗi.");
                    }
                    activeSlotIndex = -1; // Reset trạng thái ô chọn
                }
                break;
            }
        }
    }

    private void PopulateSlotList()
    {
        quickSlotsList.Clear();

        if (quickSlotsPanel == null)
        {
            Debug.LogError("[EquipSystem]: Bạn chưa kéo thả 'Quick Slots Panel' vào script!");
            return;
        }

        foreach (Transform child in quickSlotsPanel.transform)
        {
            if (child.CompareTag("QuickSlot"))
            {
                quickSlotsList.Add(child.gameObject);
            }
        }
    }

    public void AddToQuickSlots(GameObject itemToEquip)
    {
        GameObject availableSlot = FindNextEmptySlot();

        if (availableSlot != null)
        {
            itemToEquip.transform.SetParent(availableSlot.transform, false);

            string cleanName = itemToEquip.name.Replace("(Clone)", "");
            itemList.Add(cleanName);

            // Khi tự động nhặt thẳng vào Quickslot, cập nhật luôn ô active
            activeSlotIndex = quickSlotsList.IndexOf(availableSlot);

            if (WeaponHolder.Instance != null)
            {
                WeaponHolder.Instance.EquipWeapon(cleanName);
            }

            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.ReCalculateList();
            }
        }
        else
        {
            Debug.LogWarning("Thanh trang bị nhanh đã đầy, không thể thêm!");
        }
    }

    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in quickSlotsList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return null;
    }

    public bool CheckIfFull()
    {
        int counter = 0;
        foreach (GameObject slot in quickSlotsList)
        {
            if (slot.transform.childCount > 0) counter += 1;
        }
        return counter >= quickSlotsList.Count;
    }
}