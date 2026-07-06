using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// THÊM: IBeginDragHandler, IDragHandler, IEndDragHandler để xử lý kéo thả
public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // --- Is this item trashable --- //
    public bool isTrashable;

    // --- Item Info UI --- //
    private GameObject itemInfoUI;
    private Text itemInfoUI_itemName;
    private Text itemInfoUI_itemDescription;
    private Text itemInfoUI_itemFunctionality;

    public string thisName, thisDescription, thisFunctionality;

    // --- Consumption --- //
    public bool isConsumable;
    public float healthEffect;
    public float caloriesEffect;
    public float hydrationEffect;

    // --- Bàn phím & Vứt đồ --- //
    private bool isHovering = false;
    public GameObject item3DPrefab;

    // --- Biến hỗ trợ Kéo Thả --- //
    private Transform originalParent;
    private int originalSiblingIndex;
    private CanvasGroup canvasGroup; // Dùng để quản lý va chạm UI khi kéo chuột

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    private void Start()
    {
        if (InventorySystem.Instance != null && InventorySystem.Instance.itemInfoUI != null)
        {
            itemInfoUI = InventorySystem.Instance.itemInfoUI;

            itemInfoUI_itemName = itemInfoUI.transform.Find("itemName")?.GetComponent<Text>();
            itemInfoUI_itemDescription = itemInfoUI.transform.Find("itemDescription")?.GetComponent<Text>();
            itemInfoUI_itemFunctionality = itemInfoUI.transform.Find("itemFunctionality")?.GetComponent<Text>();
        }

      
    }

    private void Update()
    {
        if (isHovering && Input.GetKeyDown(KeyCode.G))
        {
            DropItem();
        }
    }

    // --logic hover chuột --//
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (itemInfoUI != null)
        {
            itemInfoUI.SetActive(true);
            if (itemInfoUI_itemName != null) itemInfoUI_itemName.text = thisName;
            if (itemInfoUI_itemDescription != null) itemInfoUI_itemDescription.text = thisDescription;
            if (itemInfoUI_itemFunctionality != null) itemInfoUI_itemFunctionality.text = thisFunctionality;
        }
    }



    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (itemInfoUI != null) itemInfoUI.SetActive(false);
    }



    // --logic tiêu thụ đồ ăn--//
    public void OnPointerDown(PointerEventData eventData)
    {
        // Bỏ trống để tránh bug giữ chuột cộng máu vô hạn
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Click chuột phải để tiêu thụ
        if (eventData.button == PointerEventData.InputButton.Right && isConsumable)
        {
            ConsumeItemWithKey();
        }
    }




    // --logic kéo thả để drop đồ--//
    // 1. Khi vừa bấm giữ chuột trái và bắt đầu kéo
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return; // Chỉ cho phép kéo bằng chuột trái

        if (item3DPrefab == null)
        {
            return; 
        }

        // Lưu lại vị trí ban đầu trong Balo lỡ kéo sai thì trả về
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Đưa icon nhảy ra ngoài cùng (Canvas) để nó nổi lên trên mọi giao diện khác
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Tắt chặn tia Raycast để lúc thả chuột ra, game soi được vùng bên dưới là UI Balo hay là Môi trường ngoài
        canvasGroup.blocksRaycasts = false;
    }

    // 2. Khi đang giữ chuột di chuyển
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (item3DPrefab == null)
        {
            return;
        }
        // Cho icon chạy theo vị trí con trỏ chuột
        transform.position = Input.mousePosition;
    }

    // 3. Khi nhả chuột trái ra
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (item3DPrefab == null)
        {
            return;
        }
        // Bật lại tương tác
        canvasGroup.blocksRaycasts = true;

        // KIỂM TRA ĐIỀU KIỆN VỨT:
        // pointerCurrentRaycast.gameObject == null nghĩa là bên dưới con trỏ chuột KHÔNG CÓ BẤT KỲ GIAO DIỆN UI NÀO
        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            DropItem(); // Thực hiện vứt đồ ra đất
        }
        else
        {
            // Nếu thả nhầm vào trong túi đồ, hoặc thả lên UI khác -> Trả đồ về vị trí cũ
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }








    // --- HÀM ĂN ĐỒ CHUNG --- //
    private void ConsumeItemWithKey()
    {
        consumingFunction(healthEffect, caloriesEffect, hydrationEffect);
        RemoveFromInventoryList();

        // Dùng Destroy thay cho DestroyImmediate để game không bị Crash
        Destroy(gameObject);
    }

    // --- HÀM VỨT ĐỒ RA THẾ GIỚI 3D --- //
    public void DropItem()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 dropPosition = mainCam.transform.position + mainCam.transform.forward * 2f;

            if (item3DPrefab != null)
            {
                GameObject droppedItem = Instantiate(item3DPrefab, dropPosition, Quaternion.identity);
                droppedItem.name = item3DPrefab.name; // Đổi tên cho sạch Hierarchy
                droppedItem.transform.SetParent(null);
            }
            else
            {
                Debug.LogWarning("CHƯA GẮN PREFAB 3D cho vật phẩm: " + thisName);
            }
        }
        else
        {
            Debug.LogError("Không tìm thấy Camera! Hãy chắc chắn Camera của bạn có gắn tag 'MainCamera' ngoài Unity.");
        }

        if (itemInfoUI != null) itemInfoUI.SetActive(false);
        isHovering = false;

        RemoveFromInventoryList();
        Destroy(gameObject);
    }

    // Hàm hỗ trợ xóa dữ liệu Inventory
    private void RemoveFromInventoryList()
    {
        string cleanName = gameObject.name.Replace("(Clone)", "");
        if (InventorySystem.Instance.itemList.Contains(cleanName))
        {
            InventorySystem.Instance.itemList.Remove(cleanName);
        }
    }

    // Các hàm tính toán chỉ số giữ nguyên
    private void consumingFunction(float healthEffect, float caloriesEffect, float hydrationEffect)
    {
        if (itemInfoUI != null) itemInfoUI.SetActive(false);
        healthEffectCalculation(healthEffect);
        caloriesEffectCalculation(caloriesEffect);
        hydrationEffectCalculation(hydrationEffect);
    }

    private static void healthEffectCalculation(float healthEffect)
    {
        float healthBeforeConsumption = PlayerState.Instance.currentHealth;
        float maxHealth = PlayerState.Instance.maxHealth;

        if (healthEffect != 0)
        {
            if ((healthBeforeConsumption + healthEffect) > maxHealth)
                PlayerState.Instance.setHealth(maxHealth);
            else
                PlayerState.Instance.setHealth(healthBeforeConsumption + healthEffect);
        }
    }

    private static void caloriesEffectCalculation(float caloriesEffect)
    {
        float caloriesBeforeConsumption = PlayerState.Instance.currentCalories;
        float maxCalories = PlayerState.Instance.maxCalories;

        if (caloriesEffect != 0)
        {
            if ((caloriesBeforeConsumption + caloriesEffect) > maxCalories)
                PlayerState.Instance.setCalories(maxCalories);
            else
                PlayerState.Instance.setCalories(caloriesBeforeConsumption + caloriesEffect);
        }
    }

    private static void hydrationEffectCalculation(float hydrationEffect)
    {
        float hydrationBeforeConsumption = PlayerState.Instance.currentHydrationPercent;
        float maxHydration = PlayerState.Instance.maxHydrationPercent;

        if (hydrationEffect != 0)
        {
            if ((hydrationBeforeConsumption + hydrationEffect) > maxHydration)
                PlayerState.Instance.setHydration(maxHydration);
            else
                PlayerState.Instance.setHydration(hydrationBeforeConsumption + hydrationEffect);
        }
    }
}