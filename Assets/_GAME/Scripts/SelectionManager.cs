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

    public GameObject selectedObject;
    public GameObject selectedTree;
    public GameObject chopHolder;
    public GameObject selectedRock;
    public GameObject mineHolder;

    [Header("--- Hiệu năng ---")]
    [Tooltip("Khoảng cách tối đa cho phép tương tác/nhắm. TRƯỚC ĐÂY là Mathf.Infinity (bắn tia vô hạn " +
             "mỗi frame, rất tốn) - giờ giới hạn lại để giảm tải Physics.Raycast chạy liên tục mỗi frame.")]
    public float maxInteractDistance = 8f;

    [Tooltip("Chỉ raycast vào các layer thực sự cần thiết (cây, đá, vật phẩm, quái, mặt đất...). " +
             "Để mặc định ~0 (mọi layer) vẫn chạy được, nhưng thu hẹp lại trong Inspector sẽ giảm tải " +
             "đáng kể vì Physics không cần kiểm tra va chạm với các layer không liên quan (UI, VFX...).")]
    public LayerMask interactionMask = ~0;

    // --- PROPERTY HỖ TRỢ ---
    public bool handIsVisible => handIcon != null && handIcon.gameObject.activeSelf;

    // Cache Camera.main 1 lần thay vì gọi lại mỗi frame (Camera.main phải tìm object có tag
    // "MainCamera" trong scene nếu chưa cache nội bộ - gọi lặp lại mỗi frame là lãng phí không cần thiết).
    private Camera mainCam;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        interaction_text = interaction_Info_UI.GetComponent<Text>();
        mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam == null) mainCam = Camera.main; // fallback phòng trường hợp camera đổi/scene reload
        if (mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, maxInteractDistance, interactionMask, QueryTriggerInteraction.Ignore))
        {
            var selectionTransform = hit.transform;

            // GÁN VẬT THỂ MÀ CAMERA ĐANG NHÌN TRÚNG VÀO BIẾN NÀY
            selectedObject = selectionTransform.gameObject;


            InteractableObject interactable = selectionTransform.GetComponentInParent<InteractableObject>();
            ChoppableTree choppableTree = selectionTransform.GetComponentInParent<ChoppableTree>();
            MineableRock mineableRock = selectionTransform.GetComponentInParent<MineableRock>();

            // --- XỬ LÝ CHỌN CÂY ĐỂ CHẶT ---
            bool isUIOpen = false;
            if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) isUIOpen = true;
            if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen) isUIOpen = true;

            if (choppableTree && choppableTree.playerInRange && isUIOpen == false)
            {
                // Nếu trước đó đang nhìn cây khác, phải tắt cây cũ đi trước khi bật cây mới
                if (selectedTree != null && selectedTree != choppableTree.gameObject)
                {
                    ChoppableTree oldTree = selectedTree.GetComponent<ChoppableTree>();
                    if (oldTree != null) oldTree.canBeChopped = false;
                }

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

            // --- XỬ LÝ CHỌN ĐÁ ĐỂ ĐẬP ---
            if (mineableRock && mineableRock.playerInRange && isUIOpen == false)
            {
                // Nếu trước đó đang nhìn đá khác, phải tắt đá cũ đi trước khi bật đá mới
                if (selectedRock != null && selectedRock != mineableRock.gameObject)
                {
                    MineableRock oldRock = selectedRock.GetComponent<MineableRock>();
                    if (oldRock != null) oldRock.canBeMined = false;
                }

                mineableRock.canBeMined = true;
                selectedRock = mineableRock.gameObject;
                if (mineHolder != null) mineHolder.gameObject.SetActive(true);
            }
            else
            {
                if (selectedRock != null)
                {
                    selectedRock.gameObject.GetComponent<MineableRock>().canBeMined = false;
                    selectedRock = null;
                    if (mineHolder != null) mineHolder.gameObject.SetActive(false);
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

                if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButton(0))
                {
                    InventorySystem.Instance.AddToInventory(interactable.GetItemName());
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlaySound(SoundManager.Instance.pickupItemSound);
                    }
                    Destroy(interactable.gameObject);
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

            if (selectedRock != null)
            {
                selectedRock.gameObject.GetComponent<MineableRock>().canBeMined = false;
                selectedRock = null;
                if (mineHolder != null) mineHolder.gameObject.SetActive(false);
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
