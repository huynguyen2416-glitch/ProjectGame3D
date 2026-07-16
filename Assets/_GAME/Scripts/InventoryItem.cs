using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    // --- Consumption (Hồi trực tiếp) --- //
    [Header("Immediate Consumption Effects")]
    public bool isConsumable;
    public float healthEffect;
    public float caloriesEffect;
    public float hydrationEffect;

    // ==========================================
    // CÀI ĐẶT BUFF (HIỆU ỨNG KÉO DÀI)
    // ==========================================
    public enum BuffType { None, ColdImmunity, InfiniteStamina, HealOverTime }

    [Header("Over-Time Buff Effects")]
    public BuffType itemBuffType = BuffType.None; // Chọn loại hiệu ứng
    public float buffDuration = 120f;             // Thời gian tác dụng (giây)
    public float buffValue = 2f;                  // Giá trị (Ví dụ: Hồi 2 máu mỗi giây nếu chọn HealOverTime)

    // --- Bàn phím & Vứt đồ --- //
    private bool isHovering = false;
    public GameObject item3DPrefab;

    // --- Biến hỗ trợ Kéo Thả --- //
    private Transform originalParent;
    private int originalSiblingIndex;
    private CanvasGroup canvasGroup;

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
        if (isHovering && Input.GetKeyDown(KeyCode.G)) DropItem();
    }

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

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && isConsumable)
        {
            ConsumeItemWithKey();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (item3DPrefab == null) return;
        DragDrop.itemBeingDragged = gameObject;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (item3DPrefab == null) return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (item3DPrefab == null) return;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            DropItem();
        }
        else
        {
            if (transform.parent == transform.root)
            {
                transform.SetParent(originalParent);
                transform.SetSiblingIndex(originalSiblingIndex);
            }
            else
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySound(SoundManager.Instance.pickupItemSound);
            }
        }
        DragDrop.itemBeingDragged = null;
        if (InventorySystem.Instance != null) InventorySystem.Instance.ReCalculateList();
    }

    // --- HÀM ĂN ĐỒ/UỐNG THUỐC --- //
    private void ConsumeItemWithKey()
    {
        // 1. Hồi các chỉ số gốc trực tiếp (Nếu có)
        consumingFunction(healthEffect, caloriesEffect, hydrationEffect);

        // 2. KÍCH HOẠT BUFF (Nếu vật phẩm có chứa Buff)
        if (itemBuffType != BuffType.None && PlayerState.Instance != null)
        {
            PlayerState.Instance.ApplyBuff(itemBuffType.ToString(), buffDuration, buffValue);
        }

        // Báo cho EquipSystem cất vũ khí/đồ vật
        if (EquipSystem.Instance != null)
        {
            EquipSystem.Instance.UnquipIfDropped(gameObject);
        }

        RemoveFromInventoryList();
        Destroy(gameObject);

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.Invoke("ReCalculateList", 0.1f);
        }
    }

    public void DropItem()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 dropPosition = mainCam.transform.position + mainCam.transform.forward * 2f;
            if (item3DPrefab != null)
            {
                GameObject droppedItem = Instantiate(item3DPrefab, dropPosition, Quaternion.identity);
                droppedItem.name = item3DPrefab.name;
                droppedItem.transform.SetParent(null);
            }
        }

        if (itemInfoUI != null) itemInfoUI.SetActive(false);
        isHovering = false;
        if (EquipSystem.Instance != null) EquipSystem.Instance.UnquipIfDropped(gameObject);

        RemoveFromInventoryList();
        Destroy(gameObject);

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.Invoke("ReCalculateList", 0.1f);
        }
    }

    private void RemoveFromInventoryList()
    {
        string cleanName = gameObject.name.Replace("(Clone)", "");
        if (InventorySystem.Instance.itemList.Contains(cleanName))
        {
            InventorySystem.Instance.itemList.Remove(cleanName);
        }
    }

    private void consumingFunction(float healthEffect, float caloriesEffect, float hydrationEffect)
    {
        if (itemInfoUI != null) itemInfoUI.SetActive(false);
        healthEffectCalculation(healthEffect);
        caloriesEffectCalculation(caloriesEffect);
        hydrationEffectCalculation(hydrationEffect);
    }

    private static void healthEffectCalculation(float healthEffect)
    {
        if (healthEffect == 0) return;
        float healthBefore = PlayerState.Instance.currentHealth;
        float maxHealth = PlayerState.Instance.maxHealth;
        PlayerState.Instance.setHealth(Mathf.Min(healthBefore + healthEffect, maxHealth));
    }

    private static void caloriesEffectCalculation(float caloriesEffect)
    {
        if (caloriesEffect == 0) return;
        float caloriesBefore = PlayerState.Instance.currentCalories;
        float maxCalories = PlayerState.Instance.maxCalories;
        PlayerState.Instance.setCalories(Mathf.Min(caloriesBefore + caloriesEffect, maxCalories));
    }

    private static void hydrationEffectCalculation(float hydrationEffect)
    {
        if (hydrationEffect == 0) return;
        float hydrationBefore = PlayerState.Instance.currentHydrationPercent;
        float maxHydration = PlayerState.Instance.maxHydrationPercent;
        PlayerState.Instance.setHydration(Mathf.Min(hydrationBefore + hydrationEffect, maxHydration));
    }
}