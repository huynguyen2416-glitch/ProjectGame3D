using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string itemName;
    public Sprite itemIcon; // Khai báo thêm biến chứa hình ảnh
    public bool playerInRange;

    public string GetItemName()
    {
        return itemName;
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