using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Giao diện UI")]
    public GameObject menuPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Nếu bạn muốn MenuManager này sống xuyên suốt qua các màn chơi khác, bỏ comment dòng dưới:
             DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Lắng nghe sự kiện người chơi nhấn phím F1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleMenu();
        }
    }

    // Hàm xử lý việc đóng/mở Menu
    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            bool isMenuOpen = menuPanel.activeSelf;

            // Đổi trạng thái ẩn/hiện
            menuPanel.SetActive(!isMenuOpen);

            // Nếu menu mở thì dừng thời gian game (0f), nếu đóng thì chạy bình thường (1f)
            Time.timeScale = isMenuOpen ? 1f : 0f;
        }
    }

    public void LoadGameScene(string sceneName)
    {
        Time.timeScale = 1f; // Đảm bảo thời gian chạy lại bình thường khi sang màn mới
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitApp()
    {
        Debug.Log("Đang thoát game...");
        Application.Quit();
    }
}