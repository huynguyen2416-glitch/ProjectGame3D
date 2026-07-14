using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Ten cac Scene (phai khop CHINH XAC ten trong File > Build Settings)")]
    public string gameplaySceneName = "Canh1";
    public string outroSceneName = "OutroScene";
    public string deathSceneName = "DeathScene";
    public string startSceneName = "StartScene";
    public string creditSceneName = "CreditScene";

    public static SaveData PendingLoad { get; private set; }
    public static Texture2D LastDeathScreenshot { get; private set; }

    public static void SetLastDeathScreenshot(Texture2D texture)
    {
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

        PlayerState.Instance.FillSaveData(data);
        SaveSystem.SaveGame(data);
    }

    // ================= SỬA TẠI ĐÂY: XÓA SAVE CŨ KHI CHƠI MỚI ================= //
    public void NewGame()
    {
        SaveSystem.DeleteSave(); // Xóa sạch dữ liệu lưu cũ để tránh xung đột
        PendingLoad = null;
        Time.timeScale = 1f;
        LoadSceneSafely(gameplaySceneName);
    }

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

    public void RestartFromLastSave()
    {
        SetLastDeathScreenshot(null);
        ContinueGame();
    }

    public void TriggerWin()
    {
        PrepareForUIScene();
        LoadSceneSafely(outroSceneName);
    }

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

    public void GoToMainMenu()
    {
        if (string.IsNullOrEmpty(startSceneName))
        {
            Debug.LogWarning("[GameController]: startSceneName dang de trong trong Inspector.");
            return;
        }

        Time.timeScale = 1f;
        LoadSceneSafely(startSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("[GameController]: Thoat game.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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

        SceneManager.LoadScene(sceneName);
    }

    public void GoToCreditScene()
    {
        if (string.IsNullOrEmpty(creditSceneName)) return;
        Time.timeScale = 1f;
        LoadSceneSafely(creditSceneName);
    }
}