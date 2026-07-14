using System;
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