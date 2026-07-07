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

    private void PopulateSlotList()
    {
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
        // 1. Tìm ô trống tiếp theo
        GameObject availableSlot = FindNextEmptySlot();

        // 2. CHỈ THÊM NẾU TÌM THẤY Ô TRỐNG
        if (availableSlot != null)
        {
            itemToEquip.transform.SetParent(availableSlot.transform, false);

            // Lấy tên gốc của vật phẩm (ví dụ: "Axe(Clone)" -> "Axe")
            string cleanName = itemToEquip.name.Replace("(Clone)", "");
            itemList.Add(cleanName);


            // Gọi WeaponHolder và truyền biến 'cleanName' vào thay vì 'thisName'
            if (WeaponHolder.Instance != null)
            {
                WeaponHolder.Instance.EquipWeapon(cleanName);
            }
            else
            {
                Debug.LogWarning("Không tìm thấy WeaponHolder.Instance trong Scene!");
            }

            // Cập nhật lại UI Balo 
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
            if (slot.transform.childCount > 0)
            {
                counter += 1;
            }
        }

        if (counter >= quickSlotsList.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}