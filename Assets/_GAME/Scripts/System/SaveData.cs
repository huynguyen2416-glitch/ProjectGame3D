using System;
using System.Collections.Generic;
using UnityEngine;

// Class dữ liệu thuần (không phải MonoBehaviour) - đây là "hình dạng" của file JSON save.
// [Serializable] để JsonUtility.ToJson/FromJson đọc/ghi được.
[Serializable]
public class SaveData
{
    // ---- Ngày ---- //
    public int daysSurvived;
    public float timeOfDay; // Giờ trong ngày lúc save, để load lại đúng thời điểm (nếu muốn khôi phục chính xác)

    // ---- Vị trí người chơi ---- //
    public float posX, posY, posZ;
    public Quaternion playerRotation = Quaternion.identity;

    // ---- Chỉ số người chơi ---- //
    public float currentHealth, maxHealth;
    public float currentCalories, maxCalories;
    public float currentHydrationPercent, maxHydrationPercent;
    public float currentStamina, maxStamina;
    public float staminaDrainPerSecond, staminaRegenPerSecond;

    // ---- Persona (nhánh nâng cấp) ---- //
    // Dùng upgradeName (đã có sẵn, là tên duy nhất từng nhánh) làm khoá thay vì Dictionary,
    // vì JsonUtility không serialize được Dictionary trực tiếp - tách thành 2 list song song.
    public List<string> personaUpgradeNames = new List<string>();
    public List<int> personaUpgradeLevels = new List<int>();
    public float personaDropRateBonus;
    public float personaHarvestSpeedBonus;
    public float personaCalorieBurnRateReduction;
    public float personaMoveSpeedBonus;
    public float personaHealthBurnRateReduction;

    // ---- Balo (Inventory) ---- //

    public List<string> inventoryItems = new List<string>();

    // ---- Quick Slot (vũ khí/công cụ đang trang bị) ---- //
    // 1 phần tử / 1 ô quick slot, chuỗi rỗng "" nghĩa là ô đó đang trống.
    public List<string> quickSlotItems = new List<string>();
    // Ô quick slot đang được CẦM TRÊN TAY lúc save (-1 = không cầm gì cả)
    public int activeQuickSlotIndex = -1;

    // ---- World State (cây/đá/item đã bị chặt/đập/nhặt) ---- //

    public List<string> destroyedWorldObjectIds = new List<string>();

    // ---- Thời điểm save (để debug / hiển thị nếu cần) ---- //
    public string savedAtIso;

    public Vector3 GetPosition()
    {
        return new Vector3(posX, posY, posZ);
    }

    public void SetPosition(Vector3 pos)
    {
        posX = pos.x;
        posY = pos.y;
        posZ = pos.z;
    }
}