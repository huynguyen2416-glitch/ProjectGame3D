using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 1. TẠO CLASS NGUYÊN LIỆU ĐỘNG
[System.Serializable]
public class CraftingIngredient
{
    public string itemName;  // Tên nguyên liệu (VD: "sm_rose_red")
    public int amount;       // Số lượng cần thiết
}

// 2. CẬP NHẬT LẠI KHUÔN CÔNG THỨC CHẾ TẠO
[System.Serializable]
public class CraftingRecipe
{
    public string uiName;
    public string resultItemName;
    public GameObject targetScreen;
    public int resultAmount = 1;

    [Header("Danh sách Nguyên liệu cần thiết")]
    public List<CraftingIngredient> ingredients = new List<CraftingIngredient>();
}

// 3. CACHE LẠI UI ĐỂ KHÔNG BỊ GIỚI HẠN SỐ LƯỢNG TEXT
public class CraftingRecipeUIRefs
{
    public List<Text> reqTexts = new List<Text>();
}

public class CraftingSystem : MonoBehaviour
{
    [Header("UI Screens")]
    public GameObject craftingScreenUI;
    public GameObject toolsScreenUI;
    public GameObject survivalScreenUI;
    public GameObject medScreenUI;

    [Header("Danh sách Công Thức Chế Tạo")]
    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    public List<string> inventoryItemList = new List<string>();

    Button toolsBTN;
    Button survivalBTN;
    Button MedBTN;

    public bool isOpen;
    public static CraftingSystem Instance { get; set; }

    [Tooltip("Số lần cập nhật UI mỗi giây")]
    public float refreshRatePerSecond = 5f;

    private readonly Dictionary<CraftingRecipe, CraftingRecipeUIRefs> uiRefsCache = new Dictionary<CraftingRecipe, CraftingRecipeUIRefs>();
    private float refreshTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        isOpen = false;

        // Setup các nút chuyển tab bằng UI hierarchy[cite: 6]
        toolsBTN = craftingScreenUI.transform.Find("ToolsButton").GetComponent<Button>();
        toolsBTN.onClick.AddListener(delegate { OpenCategory(toolsScreenUI); });

        survivalBTN = craftingScreenUI.transform.Find("SurvivalButton").GetComponent<Button>();
        survivalBTN.onClick.AddListener(delegate { OpenCategory(survivalScreenUI); });

        MedBTN = craftingScreenUI.transform.Find("MedButton").GetComponent<Button>();
        MedBTN.onClick.AddListener(delegate { OpenCategory(medScreenUI); });

        foreach (var recipe in recipes)
        {
            SetupCraftButton(recipe);
            CacheRecipeUIRefs(recipe);
        }
    }

    void CacheRecipeUIRefs(CraftingRecipe recipe)
    {
        if (recipe.targetScreen == null) return;

        Transform itemUI = recipe.targetScreen.transform.Find(recipe.uiName);
        if (itemUI == null) return;

        CraftingRecipeUIRefs refs = new CraftingRecipeUIRefs();

        // Tự động tìm tất cả các Text có tên "req1", "req2", "req3"... theo số lượng nguyên liệu
        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            Transform reqTransform = itemUI.Find("req" + (i + 1));
            if (reqTransform != null)
            {
                refs.reqTexts.Add(reqTransform.GetComponent<Text>());
            }
        }
        uiRefsCache[recipe] = refs;
    }

    void SetupCraftButton(CraftingRecipe recipe)
    {
        if (recipe.targetScreen == null) return;

        Transform itemUI = recipe.targetScreen.transform.Find(recipe.uiName);
        if (itemUI != null)
        {
            Button btn = itemUI.Find("Button").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
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

            if (isOpen)
            {
                RefreshRequirementsUI();
                refreshTimer = 0f;
            }
        }

        if (isOpen)
        {
            refreshTimer += Time.deltaTime;
            float interval = 1f / Mathf.Max(1f, refreshRatePerSecond);
            if (refreshTimer >= interval)
            {
                RefreshRequirementsUI();
                refreshTimer = 0f;
            }
        }
    }

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
        if (!uiRefsCache.TryGetValue(recipe, out CraftingRecipeUIRefs refs)) return;

        // Quét qua danh sách nguyên liệu và cập nhật màu sắc/số lượng
        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            if (i < refs.reqTexts.Count && refs.reqTexts[i] != null)
            {
                refs.reqTexts[i].gameObject.SetActive(true);
                CraftingIngredient ingredient = recipe.ingredients[i];

                int count = CountItem(ingredient.itemName);
                string vnName = GetVNName(ingredient.itemName);

                refs.reqTexts[i].text = $"{vnName}: {count} / {ingredient.amount}";
                refs.reqTexts[i].color = (count >= ingredient.amount) ? Color.green : Color.red;
            }
        }
    }

    string GetVNName(string engName)
    {
        return ItemNameVN.Get(engName); // Giữ nguyên hàm dịch thuật
    }

    void CraftItem(CraftingRecipe recipe)
    {
        inventoryItemList = InventorySystem.Instance.itemList;

        // 1. Kiểm tra xem có đủ TOÀN BỘ nguyên liệu không
        bool canCraft = true;
        foreach (var req in recipe.ingredients)
        {
            if (CountItem(req.itemName) < req.amount)
            {
                canCraft = false;
                break;
            }
        }

        // 2. Tiến hành chế tạo nếu đủ đồ
        if (canCraft)
        {
            // Trừ toàn bộ nguyên liệu
            foreach (var req in recipe.ingredients)
            {
                RemoveItem(req.itemName, req.amount);
            }

            // Thêm vật phẩm thành phẩm vào balo
            for (int i = 0; i < recipe.resultAmount; i++)
            {
                InventorySystem.Instance.AddToInventory(recipe.resultItemName);
            }

            Debug.Log($"Chế tạo thành công: {recipe.resultAmount} {recipe.resultItemName}");
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.craftingSound); // âm thanh chế tạo
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

        // Trừ logic trong mảng List trước
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

        // Hủy object UI trong balo
        foreach (GameObject slot in InventorySystem.Instance.slotList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject itemInSlot = slot.transform.GetChild(0).gameObject;
                if (itemInSlot.name == itemName || itemInSlot.name == itemName + "(Clone)")
                {
                    itemInSlot.transform.SetParent(null);
                    Destroy(itemInSlot);
                    removedCount++;
                    if (removedCount >= amountToRemove) break;
                }
            }
        }
    }
}