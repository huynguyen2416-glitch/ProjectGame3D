using UnityEngine;
using UnityEngine.SceneManagement;

// GameController: quản lý chuyển scene và dữ liệu save vào mỗi phân đoạn scene

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    [Tooltip("Scene gameplay chinh, vi du: 'Canh1'")]
    public string gameplaySceneName = "Canh1";

    [Tooltip("Scene cutscene tu dong khi thang du so ngay, vi du: 'OutroScene'")]
    public string outroSceneName = "OutroScene";

    [Tooltip("Scene man hinh thua/chet, co nut Quay lai")]
    public string deathSceneName = "DeathScene";

    [Tooltip("Scene man hinh chinh (Start Menu)")]
    public string startSceneName = "StartScene";
    [Tooltip("Scene chạy Credits cuối game")]
    public string creditSceneName = "CreditsScene";
    public static SaveData PendingLoad { get; private set; }
    public static Texture2D LastDeathScreenshot { get; private set; }

    public static void SetLastDeathScreenshot(Texture2D texture)
    {
        // Giai phong anh cu truoc khi gan anh moi, tranh ton bo nho sau nhieu lan chet
        if (LastDeathScreenshot != null)
        {
            Destroy(LastDeathScreenshot);
        }
        LastDeathScreenshot = texture;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ================= GOI TU LightingManager MOI KHI SANG NGAY MOI ================= //
    public void PerformAutosave(int daysSurvived, float timeOfDay)
    {
        if (PlayerState.Instance == null)
        {
            Debug.LogWarning("[GameController]: Khong the autosave vi PlayerState.Instance dang null.");
            return;
        }

        SaveData data = new SaveData
        {
            daysSurvived = daysSurvived,
            timeOfDay = timeOfDay
        };

        // PlayerState tu dien phan du lieu cua no (mau/calo/nuoc/stamina/vi tri) vao data.
        PlayerState.Instance.FillSaveData(data);

        // PersonaManager tu dien level/bonus cua no vao data (neu co gan trong scene)
        if (PersonaManager.Instance != null)
        {
            PersonaManager.Instance.FillSaveData(data);
            Debug.Log($"[GameController]: Autosave co Persona - so nhanh dang luu: {data.personaUpgradeNames.Count}");
        }
        else
        {
            Debug.LogWarning("[GameController]: PersonaManager.Instance dang NULL luc autosave - cac nhanh Persona da mo khoa se KHONG duoc luu lan nay!");
        }

        // InventorySystem tu dien danh sach item trong balo vao data
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.FillSaveData(data);
            Debug.Log($"[GameController]: Autosave co Balo - so item dang luu: {data.inventoryItems.Count}");
        }
        else
        {
            Debug.LogWarning("[GameController]: InventorySystem.Instance dang NULL luc autosave - Balo se KHONG duoc luu lan nay!");
        }

        // EquipSystem tu dien danh sach Quick Slot + o dang cam tren tay vao data
        if (EquipSystem.Instance != null)
        {
            EquipSystem.Instance.FillSaveData(data);
            Debug.Log($"[GameController]: Autosave co Quick Slot - dang cam o so {data.activeQuickSlotIndex + 1}");
        }
        else
        {
            Debug.LogWarning("[GameController]: EquipSystem.Instance dang NULL luc autosave - Quick Slot se KHONG duoc luu lan nay!");
        }

        // WorldStateManager tu dien danh sach vat the (cay/da/item) da bi pha huy/nhat vao data
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.FillSaveData(data);
            Debug.Log($"[GameController]: Autosave co World State - so vat the da xoa: {data.destroyedWorldObjectIds.Count}");
        }
        else
        {
            Debug.LogWarning("[GameController]: WorldStateManager.Instance dang NULL luc autosave - cay/da/item da nhat se KHONG duoc luu, se hien lai luc Continue!");
        }

        SaveSystem.SaveGame(data);
    }

    // ================= GAN VAO NUT "BAT DAU CHOI MOI" TRONG StartScene ================= //
    public void NewGame()
    {
        SaveSystem.DeleteSave(); // Xóa save cũ - tránh Continue sau này vô tình load lại tiến trình của lần chơi trước
        PendingLoad = null; // Khong ap du lieu gi -> PlayerState/LightingManager tu khoi tao mac dinh
        Time.timeScale = 1f;
        LoadSceneSafely(gameplaySceneName);
    }

    // ================= GAN VAO NUT "CONTINUE" TRONG StartScene ================= //
    public void ContinueGame()
    {
        SaveData data = SaveSystem.LoadGame();

        if (data == null)
        {
            Debug.LogWarning("[GameController]: Khong co save de Continue, chuyen sang New Game.");
            NewGame();
            return;
        }

        PendingLoad = data;
        Time.timeScale = 1f;
        LoadSceneSafely(gameplaySceneName);
    }

    // ================= GAN VAO NUT "QUAY LAI" TRONG DeathScene ================= //
    // Load lai save gan nhat (tuc la dau ngay hom truoc, vi autosave chay ngam moi khi sang ngay moi).
    public void RestartFromLastSave()
    {
        // Anh chup luc chet khong can dung nua khi da quay lai gameplay, giai phong ngay cho gon bo nho.
        SetLastDeathScreenshot(null);
        ContinueGame();
    }

    // ================= DUOC GOI TU LightingManager KHI SONG DU SO DEM ================= //
    public void TriggerWin()
    {
        PrepareForUIScene();
        LoadSceneSafely(outroSceneName);
    }

    // ================= DUOC GOI TU PlayerState KHI HET MAU ================= //
    public void TriggerDeath()
    {
        PrepareForUIScene();
        LoadSceneSafely(deathSceneName);
    }

    private void PrepareForUIScene()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ================= GAN VAO NUT "VE MAN HINH CHINH" (VD sau Outro/DeathScene) ================= //
    public void GoToMainMenu()
    {
        if (string.IsNullOrEmpty(startSceneName))
        {
            Debug.LogWarning("[GameController]: startSceneName dang de trong trong Inspector.");
            return;
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        LoadSceneSafely(startSceneName);
    }

    // ================= GAN VAO NUT "THOAT GAME" ================= //
    public void QuitGame()
    {
        Debug.Log("[GameController]: Thoat game.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void GoToCreditScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LoadSceneSafely(creditSceneName);
    }

    private void LoadSceneSafely(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GameController]: Ten scene dang de trong, kiem tra lai cac field trong Inspector!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[GameController]: Scene '{sceneName}' chua duoc them vao File > Build Settings > Scenes In Build!");
            return;
        }

        // Log ro tung ky tu (bang dau ngoac vuong) de bat loi go sai ten / du khoang trang
        // ma mat thuong nhin khong ra (VD: "Canh1 " co dau cach thua o cuoi).
        Debug.Log($"[GameController]: Chuan bi load scene: [{sceneName}]");

        SceneManager.LoadScene(sceneName);
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += LogSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= LogSceneLoaded;
    }

    private void LogSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameController]: Scene THAT SU vua load xong: name=[{scene.name}], path=[{scene.path}]");
    }
}