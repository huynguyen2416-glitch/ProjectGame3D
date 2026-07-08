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
    
    // ĐỔI THÀNH PUBLIC GAMEOBJECT ĐỂ CÁC SCRIPT KHÁC GỌI ĐƯỢC
    public GameObject selectedObject; 
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
            
            // GÁN VẬT THỂ MÀ CAMERA ĐANG NHÌN TRÚNG VÀO BIẾN NÀY
            selectedObject = selectionTransform.gameObject; 

            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();
            ChoppableTree choppableTree = selectionTransform.GetComponent<ChoppableTree>();

            // --- XỬ LÝ CHỌN CÂY ĐỂ CHẶT ---
            bool isUIOpen = false;
            if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) isUIOpen = true;
            if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen) isUIOpen = true;

            if (choppableTree && choppableTree.playerInRange && isUIOpen == false)
            {
                choppableTree.canBeChopped = true;
                selectedTree = choppableTree.gameObject;
                if (chopHolder != null) chopHolder.gameObject.SetActive(true);
            }
            else
            {
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
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlaySound(SoundManager.Instance.pickupItemSound);
                    }
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
            // NẾU KHÔNG NHÌN TRÚNG GÌ CẢ -> RESET BIẾN
            selectedObject = null; 
            
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
    public void RotatePlayerTowardsTree()
    {
        if (selectedTree != null)
        {
            // Tìm GameObject gốc của Người Chơi (thường là Player hoặc transform cha của Camera)
            // Thay "Player" bằng Tag của nhân vật chính nếu bác đặt khác nhé
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                // Lấy vị trí của cây nhưng giữ nguyên chiều cao Y của người chơi để tránh bị ngửa mặt lên trời
                Vector3 targetPosition = new Vector3(selectedTree.transform.position.x, player.transform.position.y, selectedTree.transform.position.z);

                // Ép nhân vật xoay mặt về hướng đó mượt mà
                player.transform.LookAt(targetPosition);
            }
        }
    }
}
