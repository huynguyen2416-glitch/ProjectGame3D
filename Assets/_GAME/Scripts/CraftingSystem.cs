using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingScreenUI;
    public GameObject toolsScreenUI;

    // Danh sách này dùng để đồng bộ dữ liệu từ Inventory sang để kiểm tra
    public List<string> inventoryItemList = new List<string>();

    //Category Buttons
    Button toolsBTN;

    //Craft Buttons
    Button craftAxeBTN;

    //Requirement Text
    Text AxeReq1, AxeReq2;

    public bool isOpen;

    public static CraftingSystem Instance { get; set; }

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

    // Start is called before the first frame update
    void Start()
    {
        isOpen = false;

        toolsBTN = craftingScreenUI.transform.Find("ToolsButton").GetComponent<Button>();
        toolsBTN.onClick.AddListener(delegate { OpenToolsCategory(); });

        // Tìm các Text yêu cầu nguyên liệu của Rìu (Axe)
        AxeReq1 = toolsScreenUI.transform.Find("Axe").transform.Find("req1").GetComponent<Text>();
        AxeReq2 = toolsScreenUI.transform.Find("Axe").transform.Find("req2").GetComponent<Text>();

        // Tìm nút bấm Craft của Rìu
        craftAxeBTN = toolsScreenUI.transform.Find("Axe").transform.Find("Button").GetComponent<Button>();

        // SỬA ĐỔI: Khi bấm nút sẽ gọi hàm chế tạo Rìu
        craftAxeBTN.onClick.AddListener(delegate { CraftItem("axe", "stone", 2, "wood", 1); });
    }

    void OpenToolsCategory()
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        // Nhấn phím C để Bật/Tắt Menu chế tạo
        if (Input.GetKeyDown(KeyCode.C) && !isOpen)
        {
            craftingScreenUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; // Hiện con trỏ chuột để click
            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.C) && isOpen)
        {
            craftingScreenUI.SetActive(false);
            toolsScreenUI.SetActive(false);

            // Nếu túi đồ (Inventory) cũng đang đóng thì mới khóa chuột lại giữa màn hình
            if (!InventorySystem.Instance.isOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            isOpen = false;
        }

        // Nếu menu đang mở, liên tục cập nhật chữ hiển thị số lượng nguyên liệu
        if (isOpen)
        {
            RefreshRequirementsUI();
        }
    }

    // =========================================================
    // LOGIC XỬ LÝ CHẾ TẠO VÀ ĐỒNG BỘ INVENTORY
    // =========================================================

    // Hàm cập nhật chữ hiển thị (Ví dụ: "Đá: 1/2") ngoài giao diện UI
    void RefreshRequirementsUI()
    {
        // Cập nhật danh sách nguyên liệu từ Inventory thực tế sang hệ thống Craft
        inventoryItemList = InventorySystem.Instance.itemList;

        int stoneCount = CountItem("stone");
        int woodCount = CountItem("wood");

        // Gán chữ hiển thị lên giao diện theo định dạng: "Số lượng đang có / Số lượng cần"
        AxeReq1.text = "Đá: " + stoneCount + " / 2";
        AxeReq2.text = "Gỗ: " + woodCount + " / 1";

        // Đổi màu chữ: Đủ đồ thì màu Xanh, Thiếu đồ thì màu Đỏ cho trực quan
        AxeReq1.color = (stoneCount >= 2) ? Color.green : Color.red;
        AxeReq2.color = (woodCount >= 1) ? Color.green : Color.red;
    }

    // Hàm chế tạo tổng quát (Có thể dùng chung cho nhiều món đồ về sau)
    void CraftItem(string itemToCraft, string req1Name, int req1Amount, string req2Name, int req2Amount)
    {
        // Lấy dữ liệu mới nhất từ balo
        inventoryItemList = InventorySystem.Instance.itemList;

        // 1. KIỂM TRA ĐIỀU KIỆN: Có đủ cả 2 loại nguyên liệu không?
        if (CountItem(req1Name) >= req1Amount && CountItem(req2Name) >= req2Amount)
        {
            // 2. TRỪ NGUYÊN LIỆU (Xóa tên trong danh sách và Phá hủy ảnh UI của nguyên liệu đó)
            RemoveItem(req1Name, req1Amount);
            RemoveItem(req2Name, req2Amount);

            // 3. THÊM VẬT PHẨM MỚI VÀO BALO (Gọi hàm có sẵn từ InventorySystem của bạn)
            InventorySystem.Instance.AddToInventory(itemToCraft);

            Debug.Log("Chế tạo thành công món đồ: " + itemToCraft);
        }
        else
        {
            Debug.Log("Không đủ nguyên liệu để chế tạo món này!");
        }
    }

    // Hàm phụ trợ: Đếm số lượng một item cụ thể đang có trong danh sách nhặt được
    int CountItem(string itemName)
    {
        int count = 0;
        foreach (string item in inventoryItemList)
        {
            if (item == itemName) count++;
        }
        return count;
    }

    // Hàm phụ trợ: Tiến hành trừ dữ liệu và xóa Icon của vật phẩm trên giao diện Slot UI
    void RemoveItem(string itemName, int amountToRemove)
    {
        // Bước A: Xóa tên vật phẩm khỏi danh sách backend (InventorySystem.Instance.itemList)
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

        // Bước B: Quét qua các ô ô vuông UI (Slot), tìm Icon hình ảnh của nguyên liệu đó để xóa đi
        removedCount = 0;
        foreach (GameObject slot in InventorySystem.Instance.slotList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject itemInSlot = slot.transform.GetChild(0).gameObject;

                // Kiểm tra đúng tên file hoặc tên file kèm đuôi (Clone) do hàm Instantiate sinh ra
                if (itemInSlot.name == itemName || itemInSlot.name == itemName + "(Clone)")
                {
                    Destroy(itemInSlot); // Xóa bức ảnh đó khỏi ô vuông
                    removedCount++;
                    if (removedCount >= amountToRemove) break;
                }
            }
        }
    }
}