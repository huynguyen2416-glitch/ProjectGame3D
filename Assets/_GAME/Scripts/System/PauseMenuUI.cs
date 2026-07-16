using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Tooltip("KÉO PANEL CON chứa hình ảnh/nút vào đây. Không gắn script này trực tiếp lên Panel đó!")]
    public GameObject pauseMenuPanel;

    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Buttons")]
    public Button continueButton;
    public Button mainMenuButton;
    public Button quitGameButton;

    private bool isPaused = false;

    void Start()
    {
        // Ẩn Panel đi lúc bắt đầu nhưng KHÔNG tắt GameObject chứa Script này
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        if (continueButton != null) continueButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnClickMainMenu);
        if (quitGameButton != null) quitGameButton.onClick.AddListener(OnClickQuit);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnClickMainMenu()
    {
        //tiến trình chơi hiện tại sẽ mất khi về Menu.
        if (GameController.Instance != null)
        {
            GameController.Instance.GoToMainMenu();
        }
    }

    public void OnClickQuit()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.QuitGame();
        }
    }
}