using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI;
    Text interaction_text;
    public bool onTarget;
    public Image centerDotImage;
    public Image handIcon;

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

                interaction_text.text = interactable.GetDisplayName();
                interaction_Info_UI.SetActive(true);
                if (interactable.CompareTag("pickable"))
                {
                    centerDotImage.gameObject.SetActive(false);
                    handIcon.gameObject.SetActive(true);
                }
                else
                {
                    handIcon.gameObject.SetActive(false);
                    centerDotImage.gameObject.SetActive(true);
                }

                // Nhặt đồ bằng phím F
                if ( Input.GetKeyDown(KeyCode.F))
                {
                    //  Gọi balo để thêm vật phẩm bằng Tên trước khi xóa
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
                handIcon.gameObject.SetActive(false);
                centerDotImage.gameObject.SetActive(true);
            }
        }
        else
        {
            onTarget = false;
            interaction_Info_UI.SetActive(false);
            handIcon.gameObject.SetActive(false);
            centerDotImage.gameObject.SetActive(true);
        }
    }
}