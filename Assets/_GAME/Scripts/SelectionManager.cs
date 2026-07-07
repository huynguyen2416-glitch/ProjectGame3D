using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    // --- KHỞI TẠO SINGLETON  ---
    public static SelectionManager Instance { get; private set; }

    public GameObject interaction_Info_UI;
    Text interaction_text;
    public bool onTarget;
    public Image centerDotImage;
    public Image handIcon;
    private Transform selectedObject;

    public GameObject selectedTree;
    public GameObject chopHolder;

    // --- PROPERTY HỖ TRỢ ---
    public bool handIsVisible => handIcon != null && handIcon.gameObject.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

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
            ChoppableTree choppableTree = selectionTransform.GetComponent<ChoppableTree>();

            // --- XỬ LÝ CHỌN CÂY ĐỂ CHẶT ---
            if (choppableTree && choppableTree.playerInRange)
            {
                choppableTree.canBeChopped = true;
                selectedTree = choppableTree.gameObject;
                if (chopHolder != null) chopHolder.gameObject.SetActive(true);
            }
            else
            {
                // Nếu đổi hướng nhìn sang vật thể khác, tắt trạng thái chặt cây cũ
                if (selectedTree != null)
                {
                    selectedTree.gameObject.GetComponent<ChoppableTree>().canBeChopped = false;
                    selectedTree = null;
                    if (chopHolder != null) chopHolder.gameObject.SetActive(false);
                }
            }

            // --- XỬ LÝ NHẶT VẬT PHẨM ---
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

                if (Input.GetKeyDown(KeyCode.F))
                {
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

            if (selectedTree != null)
            {
                selectedTree.gameObject.GetComponent<ChoppableTree>().canBeChopped = false;
                selectedTree = null;
                if (chopHolder != null) chopHolder.gameObject.SetActive(false);
            }
        }
    }

    public void DisableSelection()
    {
        handIcon.enabled = false;
        centerDotImage.enabled = false;
        interaction_Info_UI.SetActive(false);
        selectedObject = null;
    }

    public void EnableSelection()
    {
        handIcon.enabled = true;
        centerDotImage.enabled = true;
        interaction_Info_UI.SetActive(true);
    }
}