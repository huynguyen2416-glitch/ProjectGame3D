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
        { "potion1", "Lọ an thần" },
        { "potion2", "Tinh dầu sưởi ấm" },
        { "potion3", "Tiên thảo dược" },
        { "potion", "Tinh dầu thanh lọc" },
        {"meat", "Thịt" },
        {"cuctrang", "Cúc trắng" },
        {"boconganh", "Bồ công anh" },
        {"dalanhuong", "Dạ lan hương" },
        {"hongden", "Hồng đen" },
        {"hongdo", "Hồng đỏ" },
        {"oaihuong", "Oải hương" },
        {"campfire","Lửa trại" },
        {"drygrass","Cỏ khô" },
        {"rope","Dây thừng" },
        {"sword","Kiếm Thánh" },
        {"gem1","Khởi Nguyên" },
        {"gem2","Giao Ước" },
        {"gem3","Khải Huyền" },
    };

    public static string Get(string englishName)
    {
        if (string.IsNullOrEmpty(englishName)) return englishName;
        return nameMap.TryGetValue(englishName, out string vnName) ? vnName : englishName;
    }
}