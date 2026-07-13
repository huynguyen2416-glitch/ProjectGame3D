using System.Collections.Generic;
using UnityEngine;

public enum PersonaBranch
{
    LyTri,    // Lý trí: đồ nghề, tỉ lệ rớt vật phẩm, tốc độ cày map
    SinhTon   // Sinh tồn: máu, nước, calo, thể lực
}

public enum PersonaEffectType
{
    MaxHealth,
    MaxStamina,
    MaxHydration,
    MaxCalories,
    StaminaDrainReduction,  // giảm staminaDrainPerSecond khi sprint
    DropRateBonus,          // % tăng tỉ lệ rớt thêm vật phẩm (cộng dồn, đọc ở PersonaManager.dropRateBonus)
    HarvestSpeedBonus,      // % giảm thời gian giữa mỗi nhát chặt/đập (cộng dồn, đọc ở PersonaManager.harvestSpeedBonus)
    CalorieBurnRateReduction, // % giảm tốc độ đốt calo khi di chuyển (cộng dồn, đọc ở PersonaManager.calorieBurnRateReduction)
    MoveSpeedBonus          // % tăng tốc độ di chuyển (đi bộ + chạy), cộng dồn, đọc ở PersonaManager.moveSpeedBonus
}

[System.Serializable]
public class PersonaRequirement
{
    [Tooltip("Phải khớp CHÍNH XÁC với tên item trong InventorySystem.itemList (vd: flower, mushroom, wood)")]
    public string itemName;
    public int amount = 1;
}

[System.Serializable]
public class PersonaEffect
{
    public PersonaEffectType type;
    [Tooltip("Giá trị CỘNG THÊM khi đạt tới level này (không phải tổng dồn). Với các loại %/tỉ lệ (DropRateBonus, HarvestSpeedBonus, CalorieBurnRateReduction, MoveSpeedBonus) thì nhập dạng thập phân, VD 0.05 = +5%.")]
    public float value;
}

[System.Serializable]
public class PersonaLevelData
{
    [Tooltip("Số thứ tự level, bắt đầu từ 1")]
    public int level = 1;

    [Tooltip("Mô tả hiệu ứng khi lên level này (Vd: +10 Máu Tối Đa)")]
    public string effectDescription;

    public List<PersonaRequirement> requirements = new List<PersonaRequirement>();
    public List<PersonaEffect> effects = new List<PersonaEffect>();
}

[CreateAssetMenu(fileName = "NewPersonaUpgrade", menuName = "Persona/Upgrade")]
public class PersonaUpgradeSO : ScriptableObject
{
    public PersonaBranch branch;
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Tooltip("Danh sách level theo thứ tự tăng dần, thường tối đa 5 level")]
    public List<PersonaLevelData> levels = new List<PersonaLevelData>();
}