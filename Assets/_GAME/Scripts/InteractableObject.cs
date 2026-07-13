using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string itemName;
    public string displayItemName;

    public Sprite itemIcon;
    public bool playerInRange;

    // Trả về ID không dấu cho hệ thống Balo
    public string GetItemName()
    {
        return itemName;
    }

    // Trả về Tên có dấu để in ra UI
    public string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(displayItemName))
        {
            return displayItemName; // Ưu tiên tên đã điền tay riêng cho object này
        }
        return ItemNameVN.Get(itemName); // Không có thì tự tra bảng tên chung
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}