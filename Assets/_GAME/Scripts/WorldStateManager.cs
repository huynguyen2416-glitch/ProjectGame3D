using System.Collections.Generic;
using UnityEngine;

// Theo dõi các WorldObjectID đã bị phá huỷ/nhặt trong PHIÊN CHƠI HIỆN TẠI (chặt cây xong,
// đập đá xong, nhặt item ngoài map xong...), lưu vào SaveData lúc autosave, và áp dụng lại
// (xoá ngay lập tức, im lặng) lúc Continue/RestartFromLastSave - tránh cây/đá/item hiện lại
// y như mới dù người chơi đã xử lý xong từ trước khi save.
public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    private readonly HashSet<string> destroyedIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ApplyDestroyedFromSave(GameController.PendingLoad);
    }

    // Gọi hàm này NGAY khi 1 vật thể có WorldObjectID bị phá huỷ/nhặt xong (cây gãy, đá vỡ, item nhặt...)
    public void MarkDestroyed(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        destroyedIds.Add(id);
    }

    // Được GameController.PerformAutosave() gọi để điền danh sách vào SaveData
    public void FillSaveData(SaveData data)
    {
        data.destroyedWorldObjectIds = new List<string>(destroyedIds);
    }

    // Xoá NGAY LẬP TỨC, im lặng (không hiệu ứng chặt/vỡ, không rớt đồ lần 2) mọi vật thể trong
    // scene có WorldObjectID khớp với danh sách đã lưu - chạy sớm trong Start() trước khi
    // người chơi kịp nhìn thấy chúng "còn nguyên" một cách sai lệch.
    private void ApplyDestroyedFromSave(SaveData data)
    {
        if (data == null || data.destroyedWorldObjectIds == null || data.destroyedWorldObjectIds.Count == 0) return;

        HashSet<string> toDestroy = new HashSet<string>(data.destroyedWorldObjectIds);
        WorldObjectID[] allWorldObjects = FindObjectsOfType<WorldObjectID>();

        int destroyedCount = 0;
        foreach (WorldObjectID obj in allWorldObjects)
        {
            if (toDestroy.Contains(obj.id))
            {
                destroyedIds.Add(obj.id); // Ghi nhớ lại luôn, để lần autosave TIẾP THEO không bị mất khỏi danh sách
                Destroy(obj.gameObject);
                destroyedCount++;
            }
        }

        Debug.Log($"[WorldStateManager]: Đã dọn {destroyedCount}/{toDestroy.Count} vật thể đã bị phá huỷ/nhặt từ lần chơi trước.");
    }
}