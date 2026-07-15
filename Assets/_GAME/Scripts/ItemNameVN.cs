using System.Collections.Generic;

public static class ItemNameVN
{
    private static readonly Dictionary<string, string> nameMap = new Dictionary<string, string>
    {
        { "stone", "Đá" },
        { "wood", "Gỗ" },
        { "flower", "Hoa" },
        { "mushroom", "Nấm" },
        { "axe", "Rìu" },
        { "pickaxe", "Cuốc" },
        { "potion", "Độc dược" },
        {"meat", "Thịt" }

    };

    public static string Get(string englishName)
    {
        if (string.IsNullOrEmpty(englishName)) return englishName;
        return nameMap.TryGetValue(englishName, out string vnName) ? vnName : englishName;
    }
}