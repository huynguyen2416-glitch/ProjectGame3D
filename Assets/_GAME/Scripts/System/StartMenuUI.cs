using UnityEngine;
using UnityEngine.UI;


public class StartMenuUI : MonoBehaviour
{
    [Tooltip("Kéo Button 'New Game' vào đây")]
    public Button newGameButton;

    [Tooltip("Kéo Button 'Continue' vào đây")]
    public Button continueButton;

    [Tooltip("Kéo Button 'Quit' vào đây")]
    public Button quitGameButton;

    void Start()
    {
        if (continueButton != null)
        {

            continueButton.interactable = SaveSystem.HasSaveFile();
            continueButton.onClick.AddListener(OnClickContinue);
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnClickNewGame);
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(OnClickQuit);
        }
    }

    public void OnClickNewGame()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.NewGame();
        }
        else
        {
            Debug.LogWarning("[StartMenuUI]: Không tìm thấy GameController.Instance! Kiểm tra lại GameController đã được đặt trong StartScene chưa.");
        }
    }

    public void OnClickContinue()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.ContinueGame();
        }
        else
        {
            Debug.LogWarning("[StartMenuUI]: Không tìm thấy GameController.Instance!");
        }
    }

    public void OnClickQuit()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.QuitGame();
        }
        else
        {
            Debug.LogWarning("[StartMenuUI]: Không tìm thấy GameController.Instance!");
        }
    }
}