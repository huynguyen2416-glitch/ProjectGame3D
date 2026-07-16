using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; }

    public GameObject inventoryScreenUI;

    // ĐÂY LÀ KHỐI PREFAB CHỨA UI IMAGE ĐỂ SINH RA (ví dụ file 'silver' hoặc 'stone' dạng Prefab UI)
    public GameObject itemIconPrefab;

    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();
    public GameObject itemToAdd;
    public GameObject whatSlotToEquip;
    public bool isOpen;
    public bool isFull;

    //popup
    public GameObject pickupAlert;
    public Text pickupName;
    public Image pickupImage;
    public GameObject itemInfoUI;
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

    void Start()
    {
        isOpen = false;
        isFull = false;
        PopulateSlotList();
        Cursor.visible = false;

        // Khôi phục lại balo từ save (nếu Continue/RestartFromLastSave có PendingLoad).
        RestoreFromSave(GameController.PendingLoad);
    }

    // ================= LƯU / KHÔI PHỤC BALO ================= //

    // Gọi từ GameController.PerformAutosave() lúc tự động lưu game
    public void FillSaveData(SaveData data)
    {
        data.inventoryItems = new List<string>(itemList);
    }

    private void RestoreFromSave(SaveData data)
    {
        if (data == null || data.inventoryItems == null) return;

        Dictionary<string, int> reservedInQuickSlots = new Dictionary<string, int>();
        if (data.quickSlotItems != null)
        {
            foreach (string quickItem in data.quickSlotItems)
            {
                if (string.IsNullOrEmpty(quickItem)) continue;
                reservedInQuickSlots.TryGetValue(quickItem, out int count);
                reservedInQuickSlots[quickItem] = count + 1;
            }
        }

        foreach (string itemName in data.inventoryItems)
        {
            if (string.IsNullOrEmpty(itemName)) continue;

            if (reservedInQuickSlots.TryGetValue(itemName, out int remaining) && remaining > 0)
            {
                reservedInQuickSlots[itemName] = remaining - 1;
                Debug.LogWarning($"[InventorySystem]: Bỏ qua khôi phục '{itemName}' vào Balo vì item này đang được tính là đang cầm trên tay (Quick Slot) - tránh hiện trùng 2 bản.");
                continue;
            }

            AddToInventory(itemName);
        }
    }

    private void PopulateSlotList()
    {
        // Nếu bạn đã tự kéo tay Slot List ngoài Inspector, hãy ẩn hàm này đi.
        // Còn nếu muốn tự động tìm ô vuông có Tag "Slot", hãy giữ nguyên.
        if (slotList.Count == 0)
        {
            foreach (Transform child in inventoryScreenUI.transform)
            {
                if (child.CompareTag("Slot"))
                {
                    slotList.Add(child.gameObject);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isOpen)
        {
            inventoryScreenUI.SetActive(true);
            isOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.C) && isOpen)
        {
            inventoryScreenUI.SetActive(false);
            isOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Luồng nhặt đồ dựa vào tên Prefab UI trong thư mục Resources
    public void AddToInventory(string itemName)
    {
        if (CheckIfFull())
        {
            Debug.Log("The inventory is full");
        }
        else
        {
            whatSlotToEquip = FindNextEmptySlot();

            if (whatSlotToEquip != null)
            {
                // Tìm phôi UI GameObject trong thư mục Resources theo đúng tên itemName được truyền vào
                GameObject prefabFromResources = Resources.Load<GameObject>(itemName);

                if (prefabFromResources != null)
                {
                    // Tạo ra Icon vật phẩm tại vị trí ô trống
                    itemToAdd = Instantiate(prefabFromResources, whatSlotToEquip.transform.position, whatSlotToEquip.transform.rotation);

                    // Đưa Icon làm con của ô Slot để nó nằm gọn bên trong
                    itemToAdd.transform.SetParent(whatSlotToEquip.transform);

                    // Reset lại tỷ lệ kích thước tránh bị phình to/nhỏ quá cỡ
                    itemToAdd.transform.localScale = Vector3.one;

                    itemList.Add(itemName);
                    Debug.Log("Đã nhặt thành công: " + itemName);
                    InventoryItem itemScript = itemToAdd.GetComponent<InventoryItem>();
                    string displayName = (itemScript != null && !string.IsNullOrEmpty(itemScript.thisName)) ? itemScript.thisName : itemName;

                    // Truyền displayName (Tiếng Việt) vào Popup thay vì itemName (Tiếng Anh)
                    TriggerPickupPopup(displayName, itemToAdd.GetComponent<Image>().sprite);
                    ReCalculateList();
                }
                else
                {
                    Debug.LogError("KHÔNG TÌM THẤY Prefab UI nào tên là '" + itemName + "' trong thư mục Resources!");
                }
            }
        }
    }

    void TriggerPickupPopup(string itemName, Sprite itemSprite)
    {
        pickupAlert.SetActive(true);
        pickupName.text = itemName;
        pickupImage.sprite = itemSprite;

        // Dừng các hiệu ứng tắt cũ (nếu có) và bắt đầu đếm ngược tắt Popup
        StopAllCoroutines();
        StartCoroutine(HidePopupCoroutine());
    }
    private IEnumerator HidePopupCoroutine()
    {
        yield return new WaitForSeconds(2f); // Đợi 2 giây
        pickupAlert.SetActive(false);        // Tắt Popup
    }
    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return null;
    }

    private bool CheckIfFull()
    {
        int counter = 0;
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                counter += 1;
            }
        }

        if (counter >= slotList.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void ReCalculateList()
    {
        itemList.Clear();
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                string originalName = slot.transform.GetChild(0).name;
                string cleanName = originalName.Replace("(Clone)", "");
                itemList.Add(cleanName);
            }
        }
    }
}