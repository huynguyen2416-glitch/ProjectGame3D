using System.Globalization;
using System.Text;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public static WeaponHolder Instance { get; private set; }

    public GameObject realAxeInHand;
    public GameObject realPickaxeInHand;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            UnquipAllWeapons();
        }
    }

    public void EquipWeapon(string itemName)
    {
        UnquipAllWeapons();

        //  Nếu cất vũ khí (chọn ô trống) thì chỉ cần cất đi là xong, không làm gì thêm
        if (string.IsNullOrEmpty(itemName)) return;

        string fixedName = RemoveDiacritics(itemName).ToLower().Replace(" ", "").Trim();

        bool isPickaxe = fixedName.Contains("pickaxe") || fixedName.Contains("cuoc") || fixedName.Contains("miner");
        bool isAxe = !isPickaxe && (fixedName.Contains("axe") || fixedName.Contains("riu") || fixedName.Contains("hatchet") || fixedName.Contains("choptree"));

        if (isPickaxe)
        {
            if (realPickaxeInHand != null) realPickaxeInHand.SetActive(true);
            else Debug.LogError("[WeaponHolder]: realPickaxeInHand chưa được gán trong Inspector!");
        }
        else if (isAxe)
        {
            if (realAxeInHand != null) realAxeInHand.SetActive(true);
            else Debug.LogError("[WeaponHolder]: realAxeInHand chưa được gán trong Inspector!");
        }
        else
        {
            Debug.LogWarning($"[WeaponHolder]: Không nhận diện được vũ khí từ tên '{itemName}'.");
        }
    }

    public void UnquipAllWeapons()
    {
        if (realAxeInHand != null) realAxeInHand.SetActive(false);
        if (realPickaxeInHand != null) realPickaxeInHand.SetActive(false);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();
        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }
}