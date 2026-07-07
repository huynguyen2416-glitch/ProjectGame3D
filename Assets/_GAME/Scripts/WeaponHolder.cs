using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public static WeaponHolder Instance { get; private set; }

    public GameObject realAxeInHand;
    public GameObject realPickaxeInHand;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        UnquipAllWeapons();
    }

    public void EquipWeapon(string itemName)
    {
        // 🔍 DÒ LỖI TRỰC TIẾP: Xem hệ thống đang đọc ra tên gì
        Debug.LogWarning("LOG HỆ THỐNG: Tên vật phẩm nhận được là: [" + itemName + "]");

        UnquipAllWeapons();

        // Chuyển hết về chữ thường để so sánh cho dễ
        string fixedName = itemName.ToLower().Trim();

        // FIX LOGIC: Thay vì dùng == "axe", ta dùng .Contains("axe") 
        // Chỉ cần tên có chứa chữ axe (ví dụ: "axe", "axe_ui", "iron_axe") là đều đúng!
        if (fixedName.Contains("axe"))
        {
            if (realAxeInHand != null)
            {
                realAxeInHand.SetActive(true);
                Debug.Log("🎉 THÀNH CÔNG: Đã kích hoạt rìu trên tay!");
            }
        }
        else if (fixedName.Contains("pickaxe") || fixedName.Contains("cuoc"))
        {
            if (realPickaxeInHand != null) realPickaxeInHand.SetActive(true);
        }
    }

    public void UnquipAllWeapons()
    {
        if (realAxeInHand != null) realAxeInHand.SetActive(false);
        if (realPickaxeInHand != null) realPickaxeInHand.SetActive(false);
    }
}