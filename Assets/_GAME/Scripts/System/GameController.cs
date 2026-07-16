using UnityEngine;
using UnityEngine.SceneManagement;

// GameController: quản lý chuyển scene giữa Menu/Gameplay/Death/Outro/Credits.
// KHÔNG còn tính năng save game - mỗi lần New Game/Restart đều là 1 lượt chơi hoàn toàn mới,
// không có gì được ghi ra đĩa hay giữ lại giữa các lần chơi.
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

    // Anh chup man hinh da lam mo luc chet gan nhat, duoc PlayerState set truoc khi goi TriggerDeath().
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

    // ================= GAN VAO NUT "BAT DAU CHOI MOI" TRONG StartScene ================= //
    public void NewGame()
    {
        Time.timeScale = 1f;
        LoadSceneSafely(gameplaySceneName);
    }

    // ================= GAN VAO NUT "QUAY LAI" TRONG DeathScene ================= //
    public void RestartGame()
    {
        // Anh chup luc chet khong can dung nua khi da quay lai gameplay, giai phong ngay cho gon bo nho.
        SetLastDeathScreenshot(null);
        NewGame();
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
        LoadSceneSafely(startSceneName);
    }

    // ================= GAN VAO NUT "THOAT GAME" (neu co) ================= //
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