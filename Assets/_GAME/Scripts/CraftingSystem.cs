using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//TẠO "KHUÔN" CÔNG THỨC CHẾ TẠO
[System.Serializable]
public class CraftingRecipe
{
    public string uiName;           // Tên object trên Hierarchy (VD: "Axe", "Poison")
    public string resultItemName;   // Tên item sinh ra đưa vào balo (VD: "axe", "poison")
    public GameObject targetScreen; // UI Tab chứa món này (ToolsScreenUI, MedScreenUI...)
    public int resultAmount = 1;
    [Header("Nguyên liệu 1")]
    public string req1;
    public int req1Amount;

    [Header("Nguyên liệu 2 (Có hay không đều được)")]
    public string req2;
    public int req2Amount;
}

public class CraftingSystem : MonoBehaviour
{
    [Header("UI Screens")]
    public GameObject craftingScreenUI;
    public GameObject toolsScreenUI;
    public GameObject survivalScreenUI;
    public GameObject medScreenUI;


    // DANH SÁCH CÔNG THỨC (CÓ THỂ CHỈNH SỬA TRỰC TIẾP TRONG UNITY)
    [Header("Danh sách Công Thức Chế Tạo")]
    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    public List<string> inventoryItemList = new List<string>();

    Button toolsBTN;
    Button survivalBTN;
    Button MedBTN;

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
        toolsBTN.onClick.AddListener(delegate { OpenCategory(toolsScreenUI); });

        survivalBTN = craftingScreenUI.transform.Find("SurvivalButton").GetComponent<Button>();
        survivalBTN.onClick.AddListener(delegate { OpenCategory(survivalScreenUI); });

        MedBTN = craftingScreenUI.transform.Find("MedButton").GetComponent<Button>();
        MedBTN.onClick.AddListener(delegate { OpenCategory(medScreenUI); });

        //  Tự động cài đặt TẤT CẢ các nút chế tạo dựa theo danh sách Recipes
        foreach (var recipe in recipes)
        {
            SetupCraftButton(recipe);
        }
    }

    void SetupCraftButton(CraftingRecipe recipe)
    {
        if (recipe.targetScreen == null) return;

        Transform itemUI = recipe.targetScreen.transform.Find(recipe.uiName);
        if (itemUI != null)
        {
            Button btn = itemUI.Find("Button").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            // Truyền dữ liệu từ recipe vào hàm Craft
            btn.onClick.AddListener(delegate { CraftItem(recipe); });
        }
    }

    void OpenCategory(GameObject screenToOpen)
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(false);
        survivalScreenUI.SetActive(false);
        medScreenUI.SetActive(false);
        screenToOpen.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isOpen = !isOpen;
            craftingScreenUI.SetActive(isOpen);

            if (!isOpen)
            {
                toolsScreenUI.SetActive(false);
                survivalScreenUI.SetActive(false);
                medScreenUI.SetActive(false);
            }

            Cursor.lockState = isOpen ? CursorLockMode.None : (InventorySystem.Instance.isOpen ? CursorLockMode.None : CursorLockMode.Locked);
            Cursor.visible = isOpen;
        }

        if (isOpen) RefreshRequirementsUI();
    }


    //QUÉT QUA DANH SÁCH ĐỂ UPDATE UI 

    void RefreshRequirementsUI()
    {
        inventoryItemList = InventorySystem.Instance.itemList;

        foreach (var recipe in recipes)
        {
            UpdateRecipeUI(recipe);
        }
    }

    void UpdateRecipeUI(CraftingRecipe recipe)
    {
        if (recipe.targetScreen == null) return;

        Transform itemUI = recipe.targetScreen.transform.Find(recipe.uiName);
        if (itemUI == null) return;

        // Xử lý Nguyên liệu 1
        Text req1Text = itemUI.Find("req1").GetComponent<Text>();
        int count1 = CountItem(recipe.req1);
        string vnName1 = GetVNName(recipe.req1);
        req1Text.text = $"{vnName1}: {count1} / {recipe.req1Amount}";
        req1Text.color = (count1 >= recipe.req1Amount) ? Color.green : Color.red;

        // Xử lý Nguyên liệu 2
        Transform req2Transform = itemUI.Find("req2");
        if (req2Transform != null)
        {
            Text req2Text = req2Transform.GetComponent<Text>();
            if (!string.IsNullOrEmpty(recipe.req2) && recipe.req2Amount > 0)
            {
                req2Text.gameObject.SetActive(true);
                int count2 = CountItem(recipe.req2);
                string vnName2 = GetVNName(recipe.req2);
                req2Text.text = $"{vnName2}: {count2} / {recipe.req2Amount}";
                req2Text.color = (count2 >= recipe.req2Amount) ? Color.green : Color.red;
            }
            else
            {
                req2Text.gameObject.SetActive(false);
            }
        }
    }

    string GetVNName(string engName)
    {
        if (engName == "stone") return "Đá";
        if (engName == "wood") return "Gỗ";
        if (engName == "flower") return "Hoa";
        return engName; // Thêm từ điển ở đây nếu muốn
    }

    
    //HÀM CRAFT MỚI NHẬN VÀO TRỰC TIẾP CLASS RECIPE
    
    void CraftItem(CraftingRecipe recipe)
    {
        inventoryItemList = InventorySystem.Instance.itemList;

        bool hasReq1 = CountItem(recipe.req1) >= recipe.req1Amount;
        bool hasReq2 = string.IsNullOrEmpty(recipe.req2) || CountItem(recipe.req2) >= recipe.req2Amount;

        if (hasReq1 && hasReq2)
        {
            RemoveItem(recipe.req1, recipe.req1Amount);

            if (!string.IsNullOrEmpty(recipe.req2) && recipe.req2Amount > 0)
            {
                RemoveItem(recipe.req2, recipe.req2Amount);
            }

            // logic nhận thêm số lượng đồ crafft
            for (int i = 0; i < recipe.resultAmount; i++)
            {
                InventorySystem.Instance.AddToInventory(recipe.resultItemName);
            }

            Debug.Log($"Chế tạo thành công: {recipe.resultAmount} {recipe.resultItemName}");
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.craftingSound);
            }
        }
        else
        {
            Debug.Log("Không đủ nguyên liệu cho " + recipe.resultItemName);
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