using System.Collections.Generic;
using UnityEngine;

public class PersonaManager : MonoBehaviour
{
    public static PersonaManager Instance { get; private set; }

    [Tooltip("Kéo tất cả PersonaUpgradeSO đã tạo (Assets > Create > Persona > Upgrade) vào đây")]
    public List<PersonaUpgradeSO> allUpgrades = new List<PersonaUpgradeSO>();

    [Header("Kho Điểm Sinh Tồn (DÙNG CHUNG cho cả 2 nhánh)")]
    [Tooltip("Số điểm hiện có, chưa tiêu vào nhánh nào cả")]
    public int availablePoints = 0;

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

    // ================= KIẾM ĐIỂM SINH TỒN ================= //
    // Gọi hàm này từ bất kỳ đâu người chơi hoàn thành 1 hành động sinh tồn: ghép đồ thành công,
    // giết quái, đập đá, chặt cây, xây lửa trại, sống sót qua 1 đêm...
    public void AwardPoint(int amount = 1, string reason = "")
    {
        if (amount <= 0) return;

        availablePoints += amount;
        Debug.Log($"[PersonaManager]: +{amount} Điểm Sinh Tồn" +
                   (string.IsNullOrEmpty(reason) ? "" : $" ({reason})") +
                   $". Tổng hiện có: {availablePoints}");
    }

    // Chỉ còn kiểm tra ĐỦ ĐIỂM hay không - không còn kiểm tra vật phẩm trong balo nữa
    public bool CanUnlockNextLevel(PersonaUpgradeSO upgrade)
    {
        PersonaLevelData nextLevel = GetNextLevelData(upgrade);
        if (nextLevel == null) return false;

        return availablePoints >= nextLevel.pointCost;
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

    // Gọi hàm này từ nút "Mở khoá" trên UI - giờ chỉ trừ ĐIỂM, không đụng tới balo nữa.
    // Mở khoá xong KHÔNG THỂ hoàn tác riêng lẻ - chỉ có thể ResetAllPersona() để làm lại từ đầu.
    public bool TryUnlockNextLevel(PersonaUpgradeSO upgrade)
    {
        if (!CanUnlockNextLevel(upgrade)) return false;

        PersonaLevelData nextLevel = GetNextLevelData(upgrade);

        availablePoints -= nextLevel.pointCost;
        ApplyEffects(nextLevel);
        currentLevels[upgrade] = nextLevel.level;

        Debug.Log($"[PersonaManager]: Đã mở khoá '{upgrade.upgradeName}' Lv{nextLevel.level} (-{nextLevel.pointCost} điểm). Còn lại: {availablePoints}");

        return true;
    }

    //reset nhánh
    public void ResetAllPersona()
    {
        int totalRefund = 0;

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null) continue;

            int currentLevel = GetCurrentLevel(upgrade);
            for (int lvl = 1; lvl <= currentLevel; lvl++)
            {
                PersonaLevelData levelData = upgrade.levels.Find(l => l.level == lvl);
                if (levelData != null) totalRefund += levelData.pointCost;
            }

            currentLevels[upgrade] = 0;
        }

        availablePoints += totalRefund;

        // Reset toàn bộ % bonus cộng dồn về 0
        dropRateBonus = 0f;
        harvestSpeedBonus = 0f;
        calorieBurnRateReduction = 0f;
        moveSpeedBonus = 0f;
        healthBurnRateReduction = 0f;

        // Đưa PlayerState về đúng chỉ số GỐC (trước khi có bất kỳ Persona nào từng áp dụng)
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.ResetToBaseStats();
        }

        Debug.Log($"[PersonaManager]: ĐÃ RESET TOÀN BỘ PERSONA. Hoàn lại {totalRefund} điểm, tổng điểm hiện có: {availablePoints}");
    }
}
