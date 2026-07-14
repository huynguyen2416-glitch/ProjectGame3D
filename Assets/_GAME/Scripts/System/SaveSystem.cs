using System;
using System.IO;
using UnityEngine;

// Lớp tĩnh (static) chịu trách nhiệm đọc/ghi file save dạng JSON.
// Chỉ dùng 1 file save duy nhất (1 "slot") theo đúng mô hình bạn chọn:
// autosave ngầm lúc sang ngày mới, ghi đè lên save cũ mỗi lần.
public static class SaveSystem
{
    private const string SaveFileName = "savegame.json";

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static void SaveGame(SaveData data)
    {
        try
        {
            data.savedAtIso = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem]: Đã autosave ngày {data.daysSurvived} vào '{SavePath}'");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem]: Lỗi khi ghi save file: {e.Message}");
        }
    }

    public static SaveData LoadGame()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("[SaveSystem]: Không tìm thấy save file để load!");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem]: Lỗi khi đọc save file: {e.Message}");
            return null;
        }
    }

    public static bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    // Dùng khi bấm "New Game" và muốn xoá tiến trình cũ hẳn (tuỳ bạn có muốn dùng hay không)
    public static void DeleteSave()
    {
        if (HasSaveFile())
        {
            File.Delete(SavePath);
            Debug.Log("[SaveSystem]: Đã xoá save file.");
        }
    }
}