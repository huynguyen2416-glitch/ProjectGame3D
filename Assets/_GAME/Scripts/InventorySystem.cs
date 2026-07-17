using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; }

    public GameObject inventoryScreenUI;

    // Prefab used to render an item icon in an inventory slot.
    public GameObject itemIconPrefab;

    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();
    public GameObject itemToAdd;
    public GameObject whatSlotToEquip;
    public bool isOpen;
    public bool isFull;

    // Pickup-notification UI.
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

    }

    private void PopulateSlotList()
    {
        // Populate slots from the inventory UI only when none were assigned.
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
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.isPlacingMode) return;
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
                    // Load the UI prefab using the item's resource identifier.
                GameObject prefabFromResources = ResourceCache.Load(itemName);

                if (prefabFromResources != null)
                {
                    itemToAdd = Instantiate(prefabFromResources, whatSlotToEquip.transform.position, whatSlotToEquip.transform.rotation);
                    itemToAdd.transform.SetParent(whatSlotToEquip.transform);
                    itemToAdd.transform.localScale = Vector3.one;

                    itemList.Add(itemName);
                    Debug.Log("Đã nhặt thành công: " + itemName);
                    InventoryItem itemScript = itemToAdd.GetComponent<InventoryItem>();
                    string displayName = (itemScript != null && !string.IsNullOrEmpty(itemScript.thisName)) ? itemScript.thisName : itemName;

                    // Show the item's display name in the pickup notification.
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

        // Restart the timeout when another item is collected.
        StopAllCoroutines();
        StartCoroutine(HidePopupCoroutine());
    }
    private IEnumerator HidePopupCoroutine()
    {
        yield return new WaitForSeconds(2f);
        pickupAlert.SetActive(false);
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

    public void RemoveItemAmount(string itemName, int amountToRemove)
    {
        int removedCount = 0;

        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject itemInSlot = slot.transform.GetChild(0).gameObject;
                string cleanName = itemInSlot.name.Replace("(Clone)", "");

                if (cleanName == itemName)
                {
                    itemInSlot.transform.SetParent(null);
                    Destroy(itemInSlot);

                    removedCount++;
                    if (removedCount >= amountToRemove) break;
                }
            }
        }
        ReCalculateList();
    }
}