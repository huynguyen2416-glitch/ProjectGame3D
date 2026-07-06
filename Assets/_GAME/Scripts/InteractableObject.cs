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
        if (string.IsNullOrEmpty(displayItemName))
        {
            return itemName;
        }
        return displayItemName;
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