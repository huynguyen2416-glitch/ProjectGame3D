using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI;
    Text interaction_text;
    public bool onTarget;

    private void Start()
    {
        interaction_text = interaction_Info_UI.GetComponent<Text>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            if (interactable && interactable.playerInRange)
            {
                onTarget = true;
                interaction_text.text = interactable.GetItemName();
                interaction_Info_UI.SetActive(true);

                // Nhặt đồ bằng chuột trái hoặc phím F
                if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.F))
                {
                    // LUỒNG QUAN TRỌNG: Gọi balo để thêm vật phẩm bằng Tên trước khi xóa
                    InventorySystem.Instance.AddToInventory(interactable.GetItemName());

                    Destroy(selectionTransform.gameObject);
                    interaction_Info_UI.SetActive(false);
                    onTarget = false;
                }
            }
            else
            {
                onTarget = false;
                interaction_Info_UI.SetActive(false);
            }
        }
        else
        {
            onTarget = false;
            interaction_Info_UI.SetActive(false);
        }
    }
}