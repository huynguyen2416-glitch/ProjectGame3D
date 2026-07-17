using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Info")]
    public string thisName;
    public string thisDescription;
    public string thisFunctionality;
    public bool isTrashable;
    public GameObject item3DPrefab;

    [Header("Immediate Consumption Effects")]
    public bool isConsumable;
    public float healthEffect;
    public float caloriesEffect;
    public float hydrationEffect;

    public enum BuffType { None, ColdImmunity, InfiniteStamina, HealOverTime }

    [Header("Over-Time Buff Effects")]
    public BuffType itemBuffType = BuffType.None;
    public float buffDuration = 120f;
    public float buffValue = 2f;

    // Tooltip UI and pointer state.
    private GameObject itemInfoUI;
    private Text itemInfoUI_itemName;
    private Text itemInfoUI_itemDescription;
    private Text itemInfoUI_itemFunctionality;
    private bool isHovering = false;

    // State needed to restore an interrupted drag operation.
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
        if (eventData.button != PointerEventData.InputButton.Left || item3DPrefab == null) return;

        DragDrop.itemBeingDragged = gameObject;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || item3DPrefab == null) return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || item3DPrefab == null) return;
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
            else if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.pickupItemSound);
            }
        }

        DragDrop.itemBeingDragged = null;
        if (InventorySystem.Instance != null) InventorySystem.Instance.ReCalculateList();
    }

    private void ConsumeItemWithKey()
    {
        ApplyConsumptionEffects(healthEffect, caloriesEffect, hydrationEffect);

        // Apply an optional timed effect.
        if (itemBuffType != BuffType.None && PlayerState.Instance != null)
        {
            PlayerState.Instance.ApplyBuff(itemBuffType.ToString(), buffDuration, buffValue);
        }

        if (EquipSystem.Instance != null) EquipSystem.Instance.UnquipIfDropped(gameObject);

        RemoveFromInventoryList();
        Destroy(gameObject);

        if (InventorySystem.Instance != null) InventorySystem.Instance.Invoke(nameof(InventorySystem.ReCalculateList), 0.1f);
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
            }
        }

        if (itemInfoUI != null) itemInfoUI.SetActive(false);
        isHovering = false;

        if (EquipSystem.Instance != null) EquipSystem.Instance.UnquipIfDropped(gameObject);

        RemoveFromInventoryList();
        Destroy(gameObject);

        if (InventorySystem.Instance != null) InventorySystem.Instance.Invoke(nameof(InventorySystem.ReCalculateList), 0.1f);
    }

    private void RemoveFromInventoryList()
    {
        string cleanName = gameObject.name.Replace("(Clone)", "");
        if (InventorySystem.Instance.itemList.Contains(cleanName))
        {
            InventorySystem.Instance.itemList.Remove(cleanName);
        }
    }

    private void ApplyConsumptionEffects(float health, float calories, float hydration)
    {
        if (itemInfoUI != null) itemInfoUI.SetActive(false);
        CalculateHealthEffect(health);
        CalculateCaloriesEffect(calories);
        CalculateHydrationEffect(hydration);
    }

    private static void CalculateHealthEffect(float healthEffect)
    {
        if (healthEffect == 0) return;
        float healthBefore = PlayerState.Instance.currentHealth;
        float maxHealth = PlayerState.Instance.maxHealth;
        PlayerState.Instance.setHealth(Mathf.Min(healthBefore + healthEffect, maxHealth));
    }

    private static void CalculateCaloriesEffect(float caloriesEffect)
    {
        if (caloriesEffect == 0) return;
        float caloriesBefore = PlayerState.Instance.currentCalories;
        float maxCalories = PlayerState.Instance.maxCalories;
        PlayerState.Instance.setCalories(Mathf.Min(caloriesBefore + caloriesEffect, maxCalories));
    }

    private static void CalculateHydrationEffect(float hydrationEffect)
    {
        if (hydrationEffect == 0) return;
        float hydrationBefore = PlayerState.Instance.currentHydrationPercent;
        float maxHydration = PlayerState.Instance.maxHydrationPercent;
        PlayerState.Instance.setHydration(Mathf.Min(hydrationBefore + hydrationEffect, maxHydration));
    }
}