using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; }

    public GameObject inventoryScreenUI;

    // ĐÂY LÀ KHỐI PREFAB CHỨA UI IMAGE ĐỂ SINH RA (ví dụ file 'silver' hoặc 'stone' dạng Prefab UI)
    [Header("Item Prefab Config")]
    public GameObject itemIconPrefab;

    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();
    public GameObject itemToAdd;
    public GameObject whatSlotToEquip;
    public bool isOpen;
    public bool isFull;

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
        if (Input.GetKeyDown(KeyCode.I) && !isOpen)
        {
            inventoryScreenUI.SetActive(true);
            isOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.I) && isOpen)
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
                }
                else
                {
                    Debug.LogError("KHÔNG TÌM THẤY Prefab UI nào tên là '" + itemName + "' trong thư mục Resources!");
                }
            }
        }
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
}