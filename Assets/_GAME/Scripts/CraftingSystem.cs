using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CraftingIngredient
{
    public string itemName;
    public int amount;
}

[System.Serializable]
public class CraftingRecipe
{
    public string uiName;
    public string resultItemName;
    public GameObject targetScreen;
    public int resultAmount = 1;

    [Header("--- Cấu Hình Cho Công Trình Xây Dựng ---")]
    public bool isStructure;           // Tích chọn nếu đây là công trình đặt xuống đất (như Lửa Trại)
    public GameObject structurePrefab; // Kéo Prefab 3D của đống lửa thực tế vào đây

    [Header("Danh sách Nguyên liệu cần thiết")]
    public List<CraftingIngredient> ingredients = new List<CraftingIngredient>();
}

public class CraftingRecipeUIRefs
{
    public List<Text> reqTexts = new List<Text>();
}

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("UI Screens")]
    public GameObject craftingScreenUI;
    public GameObject toolsScreenUI;
    public GameObject survivalScreenUI;
    public GameObject medScreenUI;

    [Header("--- Hệ Thống Nhắm Đặt Công Trình ---")]
    public GameObject placementCrosshair; // Kéo UI Tâm ngắm (như dấu cộng nhỏ giữa màn hình) vào đây
    public LayerMask groundLayer;         // Chọn Layer "Ground" của mặt đất 
    public float maxBuildDistance = 10f;  // Khoảng cách tối đa có thể đặt công trình

    [Header("Dữ Liệu")]
    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();
    public List<string> inventoryItemList = new List<string>();

    // Trạng thái hệ thống
    public bool isOpen;
    public bool isPlacingMode = false; // Đã chuyển thành public để InventorySystem truy cập được

    private CraftingRecipe pendingRecipe;
    private Button toolsBTN, survivalBTN, MedBTN;
    private readonly Dictionary<CraftingRecipe, CraftingRecipeUIRefs> uiRefsCache = new Dictionary<CraftingRecipe, CraftingRecipeUIRefs>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        isOpen = false;
        isPlacingMode = false;

        if (placementCrosshair != null) placementCrosshair.SetActive(false);

        // Khởi tạo các nút chuyển Tab danh mục
        toolsBTN = craftingScreenUI.transform.Find("ToolsButton").GetComponent<Button>();
        toolsBTN.onClick.AddListener(() => OpenCategory(toolsScreenUI));

        survivalBTN = craftingScreenUI.transform.Find("SurvivalButton").GetComponent<Button>();
        survivalBTN.onClick.AddListener(() => OpenCategory(survivalScreenUI));

        MedBTN = craftingScreenUI.transform.Find("MedButton").GetComponent<Button>();
        MedBTN.onClick.AddListener(() => OpenCategory(medScreenUI));

        // Cài đặt nút bấm cho từng công thức
        foreach (var recipe in recipes)
        {
            SetupCraftButton(recipe);
            CacheRecipeUIRefs(recipe);
        }
    }

    private void CacheRecipeUIRefs(CraftingRecipe recipe)
    {
        if (recipe.targetScreen == null) return;

        Transform itemUI = recipe.targetScreen.transform.Find(recipe.uiName);
        if (itemUI == null) return;

        CraftingRecipeUIRefs refs = new CraftingRecipeUIRefs();

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

    private void SetupCraftButton(CraftingRecipe recipe)
    {
        if (recipe.targetScreen == null) return;

        Transform itemUI = recipe.targetScreen.transform.Find(recipe.uiName);
        if (itemUI != null)
        {
            Button btn = itemUI.Find("Button").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => CraftItem(recipe));
        }
    }

    private void OpenCategory(GameObject screenToOpen)
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(false);
        survivalScreenUI.SetActive(false);
        medScreenUI.SetActive(false);
        screenToOpen.SetActive(true);
    }

    private void Update()
    {
        if (isPlacingMode)
        {
            // Nhấn Chuột Trái (LMB) để CHỐT vị trí và tiến hành XÂY dựng
            if (Input.GetMouseButtonDown(0)) TryPlaceStructure();

            // Nhấn Chuột Phải (RMB) hoặc phím ESC để HỦY bỏ chế độ đặt
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) CancelPlacement();

            return; // Đang đặt đồ thì khóa hoàn toàn logic đóng/mở UI bên dưới
        }

        // kích panel
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

            if (isOpen) RefreshRequirementsUI();
        }
    }

    private void RefreshRequirementsUI()
    {
        inventoryItemList = InventorySystem.Instance.itemList;
        foreach (var recipe in recipes)
        {
            UpdateRecipeUI(recipe);
        }
    }

    private void UpdateRecipeUI(CraftingRecipe recipe)
    {
        if (!uiRefsCache.TryGetValue(recipe, out CraftingRecipeUIRefs refs)) return;

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            if (i < refs.reqTexts.Count && refs.reqTexts[i] != null)
            {
                refs.reqTexts[i].gameObject.SetActive(true);
                CraftingIngredient ingredient = recipe.ingredients[i];

                int count = CountItem(ingredient.itemName);
                string vnName = ItemNameVN.Get(ingredient.itemName);

                refs.reqTexts[i].text = $"{vnName}: {count} / {ingredient.amount}";
                refs.reqTexts[i].color = (count >= ingredient.amount) ? Color.green : Color.red;
            }
        }
    }

    // Logic chính xử lý khi nhấn nút "Craft" trên giao diện
    private void CraftItem(CraftingRecipe recipe)
    {
        inventoryItemList = InventorySystem.Instance.itemList;

        // Bước 1: Kiểm tra xem người chơi có đủ nguyên liệu không
        foreach (var req in recipe.ingredients)
        {
            if (CountItem(req.itemName) < req.amount)
            {
                Debug.Log("Không đủ nguyên liệu chế tạo!");
                return;
            }
        }

        // Bước 2: Phân loại cơ chế Chế tạo
        if (recipe.isStructure)
        {
            // BẮT ĐẦU CHẾ ĐỘ ĐẶT (Ví dụ: Lửa Trại)
            pendingRecipe = recipe;
            isPlacingMode = true;

            // Ẩn giao diện chế tạo trước khi nhắm hướng đặt 
            isOpen = false;
            if (craftingScreenUI != null) craftingScreenUI.SetActive(false);

            // Ẩn túi đồ để khôi phục lại các phím 
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.isOpen = false;
                if (InventorySystem.Instance.inventoryScreenUI != null)
                {
                    InventorySystem.Instance.inventoryScreenUI.SetActive(false);
                }
            }

            // Khóa con trỏ chuột vào giữa màn hình để xoay Camera nhắm hướng đặt
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Hiển thị tâm ngắm nhắm bắn UI ở giữa màn hình
            if (placementCrosshair != null) placementCrosshair.SetActive(true);

            Debug.Log("Đã vào chế độ nhắm đặt. Nhấn Chuột Trái để xây, Chuột Phải hoặc ESC để Hủy.");
        }
        else
        {
            // CHẾ TẠO ĐỒ VẬT THƯỜNG (Ví dụ: Rìu, Thuốc)
            foreach (var req in recipe.ingredients)
            {
                InventorySystem.Instance.RemoveItemAmount(req.itemName, req.amount);
            }

            for (int i = 0; i < recipe.resultAmount; i++)
            {
                InventorySystem.Instance.AddToInventory(recipe.resultItemName);
            }

            if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(SoundManager.Instance.craftingSound);
            if (PersonaManager.Instance != null) PersonaManager.Instance.AwardPoint(1, $"Chế tạo {recipe.resultItemName}");

            RefreshRequirementsUI();
        }
    }

    // Logic xử lý khi click Chuột Trái để đặt công trình xuống đất
    private void TryPlaceStructure()
    {
        if (pendingRecipe == null) return;

        //Kiểm tra lại nguyên liệu vì túi đồ có thể thay đổi trong lúc đang đặt.
        inventoryItemList = InventorySystem.Instance.itemList;
        foreach (var req in pendingRecipe.ingredients)
        {
            if (CountItem(req.itemName) < req.amount)
            {
                Debug.LogWarning("Không đủ nguyên liệu! Ai cho mài hack vứt đồ ra đất hả!!");
                CancelPlacement();
                return;
            }
        }

        // Bắn tia Raycast từ vị trí giữa camera (tâm màn hình) thẳng về phía trước
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, groundLayer))
        {
            //Chỉ trừ nguyên liệu sau khi đã tìm thấy một vị trí mặt đất hợp lệ.
            foreach (var req in pendingRecipe.ingredients)
            {
                InventorySystem.Instance.RemoveItemAmount(req.itemName, req.amount);
            }

            //Tạo công trình tại điểm va chạm của tia raycast.
            if (pendingRecipe.structurePrefab != null)
            {
                GameObject structure = Instantiate(pendingRecipe.structurePrefab, hit.point, Quaternion.identity);
                structure.name = pendingRecipe.structurePrefab.name;

                Campfire campfireScript = structure.GetComponent<Campfire>();
                if (campfireScript != null)
                {
                    campfireScript.isPlayerBuilt = true;
                }

                Shelter shelterScript = structure.GetComponent<Shelter>();
                if (shelterScript != null)
                {
                    shelterScript.isPlayerBuilt = true;
                }
            }

            // Thông báo cho người chơi và phần thưởng cho công trình đã hoàn thành.
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(SoundManager.Instance.craftingSound);
            if (PersonaManager.Instance != null) PersonaManager.Instance.AwardPoint(1, $"Xây dựng {pendingRecipe.resultItemName}");

            Debug.Log($"Xây dựng thành công: {pendingRecipe.resultItemName}");

            //  Thoát chế độ nhắm đặt đồ
            CancelPlacement();
        }
        else
        {
            Debug.Log("Vị trí ngắm quá xa hoặc không phải mặt đất hợp lệ!");
        }
    }

    // Hủy bỏ chế độ nhắm đặt, reset lại trạng thái
    private void CancelPlacement()
    {
        isPlacingMode = false;
        pendingRecipe = null;

        if (placementCrosshair != null) placementCrosshair.SetActive(false);

        // Đưa chuột về trạng thái khóa ẩn bình thường của góc nhìn thứ nhất
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Đã hủy chế độ đặt công trình.");
    }

    private int CountItem(string itemName)
    {
        int count = 0;
        foreach (string item in inventoryItemList)
        {
            if (item == itemName) count++;
        }
        return count;
    }
}