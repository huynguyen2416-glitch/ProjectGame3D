using System.Collections.Generic;
using UnityEngine;

public class PersonaManager : MonoBehaviour
{
    public static PersonaManager Instance { get; private set; }

    [Tooltip("Kéo tất cả PersonaUpgradeSO đã tạo (Assets > Create > Persona > Upgrade) vào đây")]
    public List<PersonaUpgradeSO> allUpgrades = new List<PersonaUpgradeSO>();

    // Level hiện tại của từng nâng cấp, 0 = chưa mở khoá level nào. Chỉ tồn tại trong runtime (không save).
    private Dictionary<PersonaUpgradeSO, int> currentLevels = new Dictionary<PersonaUpgradeSO, int>();

    // Hiệu ứng không map thẳng vào PlayerState, đọc giá trị này ở nơi khác (vd ChoppableTree/MineableRock)
    public float dropRateBonus { get; private set; }
    public float harvestSpeedBonus { get; private set; } 
    public float calorieBurnRateReduction { get; private set; } // 0.2 = -20% tốc độ đốt calo
    public float moveSpeedBonus { get; private set; } // 0.1 = +10% tốc độ di chuyển (đi bộ + chạy), đọc ở PlayerMovement

    public float healthBurnRateReduction { get; private set; }// 0.3 -20% tốc độ đốt HP
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null && !currentLevels.ContainsKey(upgrade))
                currentLevels[upgrade] = 0;
        }
    }

    public int GetCurrentLevel(PersonaUpgradeSO upgrade)
    {
        return currentLevels.TryGetValue(upgrade, out int lvl) ? lvl : 0;
    }

    // Trả về dữ liệu level kế tiếp cần mở khoá, null nếu đã đạt level tối đa
    public PersonaLevelData GetNextLevelData(PersonaUpgradeSO upgrade)
    {
        int current = GetCurrentLevel(upgrade);
        foreach (var lvlData in upgrade.levels)
        {
            if (lvlData.level == current + 1) return lvlData;
        }
        return null;
    }

    private int CountItem(string itemName)
    {
        if (InventorySystem.Instance == null) return 0;
        int count = 0;
        foreach (string item in InventorySystem.Instance.itemList)
        {
            if (item == itemName) count++;
        }
        return count;
    }

    public bool CanUnlockNextLevel(PersonaUpgradeSO upgrade)
    {
        PersonaLevelData nextLevel = GetNextLevelData(upgrade);
        if (nextLevel == null) return false;

        foreach (var req in nextLevel.requirements)
        {
            if (CountItem(req.itemName) < req.amount) return false;
        }
        return true;
    }

    // Trừ nguyên liệu khỏi cả danh sách logic (itemList) lẫn UI (slotList), cùng cách CraftingSystem đang làm
    private void ConsumeRequirements(PersonaLevelData levelData)
    {
        foreach (var req in levelData.requirements)
        {
            // 1. Trừ trong danh sách Logic (Giữ nguyên)
            int removed = 0;
            for (int i = InventorySystem.Instance.itemList.Count - 1; i >= 0; i--)
            {
                if (InventorySystem.Instance.itemList[i] == req.itemName)
                {
                    InventorySystem.Instance.itemList.RemoveAt(i);
                    removed++;
                    if (removed >= req.amount) break;
                }
            }

            // 2. Xóa Object trên UI 
            removed = 0;
            foreach (GameObject slot in InventorySystem.Instance.slotList)
            {
                if (slot.transform.childCount == 0) continue;

                GameObject itemInSlot = slot.transform.GetChild(0).gameObject;
                if (itemInSlot.name == req.itemName || itemInSlot.name == req.itemName + "(Clone)")
                {
                    //Cắt đứt quan hệ cha-con ngay lập tức để ReCalculateList không quét trúng nữa
                    itemInSlot.transform.SetParent(null);

                    Destroy(itemInSlot);
                    removed++;
                    if (removed >= req.amount) break;
                }
            }
        }
    }

    private void ApplyEffects(PersonaLevelData levelData)
    {
        foreach (var effect in levelData.effects)
        {
            switch (effect.type)
            {
                case PersonaEffectType.MaxHealth:
                    if (PlayerState.Instance != null)
                    {
                        PlayerState.Instance.maxHealth += effect.value;
                        PlayerState.Instance.setHealth(PlayerState.Instance.currentHealth + effect.value);
                    }
                    break;

                case PersonaEffectType.MaxStamina:
                    if (PlayerState.Instance != null)
                        PlayerState.Instance.maxStamina += effect.value;
                    break;

                case PersonaEffectType.MaxHydration:
                    if (PlayerState.Instance != null)
                        PlayerState.Instance.maxHydrationPercent += effect.value;
                    break;

                case PersonaEffectType.MaxCalories:
                    if (PlayerState.Instance != null)
                        PlayerState.Instance.maxCalories += effect.value;
                    break;

                case PersonaEffectType.StaminaDrainReduction:
                    if (PlayerState.Instance != null)
                        PlayerState.Instance.staminaDrainPerSecond = Mathf.Max(0f, PlayerState.Instance.staminaDrainPerSecond - effect.value);
                    break;

                case PersonaEffectType.StaminaRegenBonus:
                    if (PlayerState.Instance != null)
                        PlayerState.Instance.staminaRegenPerSecond += effect.value;
                    break;

                case PersonaEffectType.DropRateBonus:
                    dropRateBonus += effect.value;
                    break;

                case PersonaEffectType.HarvestSpeedBonus:
                    harvestSpeedBonus += effect.value;
                    break;

                case PersonaEffectType.CalorieBurnRateReduction:
                    calorieBurnRateReduction = Mathf.Clamp01(calorieBurnRateReduction + effect.value);
                    break;

                case PersonaEffectType.MoveSpeedBonus:
                    moveSpeedBonus += effect.value;
                    break;
                case PersonaEffectType.HealthBurnRateReduction:
                    healthBurnRateReduction = Mathf.Clamp01(healthBurnRateReduction + effect.value);
                    break;
            }
        }
    }

    // Gọi hàm này từ nút "Mở khoá" trên UI
    public bool TryUnlockNextLevel(PersonaUpgradeSO upgrade)
    {
        if (!CanUnlockNextLevel(upgrade)) return false;

        PersonaLevelData nextLevel = GetNextLevelData(upgrade);
        ConsumeRequirements(nextLevel);
        ApplyEffects(nextLevel);

        currentLevels[upgrade] = nextLevel.level;

        if (InventorySystem.Instance != null) InventorySystem.Instance.ReCalculateList();

        return true;
    }
}