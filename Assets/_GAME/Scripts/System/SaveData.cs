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

    // ---- Chỉ số người chơi ---- //
    public float currentHealth, maxHealth;
    public float currentCalories, maxCalories;
    public float currentHydrationPercent, maxHydrationPercent;
    public float currentStamina, maxStamina;
    // Persona (StaminaDrainReduction/StaminaRegenBonus) sửa thẳng 2 giá trị này lúc mở khoá -
    // phải lưu lại, nếu không thì Continue/hồi sinh sẽ mất hiệu ứng, quay về mặc định Inspector.
    public float staminaDrainPerSecond, staminaRegenPerSecond;

    // ---- Persona (nhánh nâng cấp) ---- //
    // Dùng upgradeName (đã có sẵn, là tên duy nhất từng nhánh) làm khoá thay vì Dictionary,
    // vì JsonUtility không serialize được Dictionary trực tiếp - tách thành 2 list song song.
    public List<string> personaUpgradeNames = new List<string>();
    public List<int> personaUpgradeLevels = new List<int>();

    // % bonus cộng dồn hiện tại của Persona, lưu THẲNG giá trị cuối cùng (không lưu lại bằng
    // cách replay từng effect) để lúc load chỉ cần gán lại, không cần tính toán gì thêm.
    public float personaDropRateBonus;
    public float personaHarvestSpeedBonus;
    public float personaCalorieBurnRateReduction;
    public float personaMoveSpeedBonus;
    public float personaHealthBurnRateReduction;

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