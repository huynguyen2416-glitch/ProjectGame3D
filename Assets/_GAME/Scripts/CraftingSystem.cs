using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingScreenUI;
    public GameObject toolsScreenUI;
    public List<string> inventoryItemList = new List<string>();
    Button toolsBTN;
    public bool isOpen;

    public static CraftingSystem Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        isOpen = false;

        toolsBTN = craftingScreenUI.transform.Find("ToolsButton").GetComponent<Button>();
        toolsBTN.onClick.AddListener(delegate { OpenToolsCategory(); });


        // ==========================================================
        SetupCraftButton("Axe", "axe", "stone", 2, "wood", 1);
        SetupCraftButton("Pickaxe", "pickaxe", "stone", 3, "wood", 2); //thêm Cuốc
        // ==========================================================
    }

    // Hàm phụ trợ tự động tìm Button trên UI và gán sự kiện CraftItem
    void SetupCraftButton(string uiName, string itemToCraft, string req1, int amt1, string req2, int amt2)
    {
        Transform itemUI = toolsScreenUI.transform.Find(uiName);
        if (itemUI != null)
        {
            Button btn = itemUI.Find("Button").GetComponent<Button>();
            btn.onClick.AddListener(delegate { CraftItem(itemToCraft, req1, amt1, req2, amt2); });
        }
    }

    void OpenToolsCategory() { craftingScreenUI.SetActive(false); toolsScreenUI.SetActive(true); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isOpen = !isOpen;
            craftingScreenUI.SetActive(isOpen);
            if (!isOpen) toolsScreenUI.SetActive(false);

            Cursor.lockState = isOpen ? CursorLockMode.None : (InventorySystem.Instance.isOpen ? CursorLockMode.None : CursorLockMode.Locked);
            Cursor.visible = isOpen;
        }

        if (isOpen) RefreshRequirementsUI();
    }

//case ui text
    void RefreshRequirementsUI()
    {
        inventoryItemList = InventorySystem.Instance.itemList;

        // Quét qua danh sách các item cần quản lý hiển thị
        string[] allCraftableItems = { "Axe", "Pickaxe" };

        foreach (string itemName in allCraftableItems)
        {
            switch (itemName)
            {
                case "Axe":
                    UpdateRecipeUI("Axe", "stone", 2, "wood", 1);
                    break;

                case "Pickaxe":
                    UpdateRecipeUI("Pickaxe", "stone", 3, "wood", 2);
                    break;

                    // Thêm món mới thì chỉ cần thêm 'case "Tên_Món":' 
                    //case "   ":
                    //UpdateRecipeUI("Pickaxe", "stone", 3, "wood", 2);
                    //break;
            }
        }
    }

    // Hàm phụ trợ tự động tìm Text "req1", "req2" của món đó và update số lượng, màu sắc
    void UpdateRecipeUI(string uiName, string req1Name, int req1Amount, string req2Name, int req2Amount)
    {
        Transform itemUI = toolsScreenUI.transform.Find(uiName);
        if (itemUI == null) return;

        Text req1Text = itemUI.Find("req1").GetComponent<Text>();
        Text req2Text = itemUI.Find("req2").GetComponent<Text>();

        int count1 = CountItem(req1Name);
        int count2 = CountItem(req2Name);

        // Đổi ngôn ngữ hiển thị tùy ý bằng tiếng Việt
        string vnName1 = (req1Name == "stone") ? "Đá" : (req1Name == "wood") ? "Gỗ" : req1Name;
        string vnName2 = (req2Name == "stone") ? "Đá" : (req2Name == "wood") ? "Gỗ" : req2Name;

        req1Text.text = $"{vnName1}: {count1} / {req1Amount}";
        req2Text.text = $"{vnName2}: {count2} / {req2Amount}";

        req1Text.color = (count1 >= req1Amount) ? Color.green : Color.red;
        req2Text.color = (count2 >= req2Amount) ? Color.green : Color.red;
    }


    // --- CÁC LOGIC CHẾ TẠO, ĐẾM, XÓA ĐỒ CỦA BẠN ĐƯỢC GIỮ NGUYÊN HOÀN TOÀN ---

    void CraftItem(string itemToCraft, string req1Name, int req1Amount, string req2Name, int req2Amount)
    {
        inventoryItemList = InventorySystem.Instance.itemList;

        if (CountItem(req1Name) >= req1Amount && CountItem(req2Name) >= req2Amount)
        {
            RemoveItem(req1Name, req1Amount);
            RemoveItem(req2Name, req2Amount);
            InventorySystem.Instance.AddToInventory(itemToCraft);
            Debug.Log("Chế tạo thành công: " + itemToCraft);
        }
        else
        {
            Debug.Log("Không đủ nguyên liệu!");
        }
    }

    int CountItem(string itemName)
    {
        int count = 0;
        foreach (string item in inventoryItemList) { if (item == itemName) count++; }
        return count;
    }

    void RemoveItem(string itemName, int amountToRemove)
    {
        int removedCount = 0;
        for (int i = InventorySystem.Instance.itemList.Count - 1; i >= 0; i--)
        {
            if (InventorySystem.Instance.itemList[i] == itemName)
            {
                InventorySystem.Instance.itemList.RemoveAt(i);
                removedCount++;
                if (removedCount >= amountToRemove) break;
            }
        }

        removedCount = 0;
        foreach (GameObject slot in InventorySystem.Instance.slotList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject itemInSlot = slot.transform.GetChild(0).gameObject;
                if (itemInSlot.name == itemName || itemInSlot.name == itemName + "(Clone)")
                {
                    Destroy(itemInSlot);
                    removedCount++;
                    if (removedCount >= amountToRemove) break;
                }
            }
        }
    }
}