using UnityEngine;
using UnityEngine.UI;

public class StartMenuUI : MonoBehaviour
{
    public Button newGameButton;
    public Button quitGameButton;

    void Start()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OnClickNewGame);
        if (quitGameButton != null) quitGameButton.onClick.AddListener(OnClickQuit);
    }

    public void OnClickNewGame()
    {
        // Gọi thẳng lệnh kích hoạt thế giới mới từ Singleton gốc
        if (GameController.Instance != null) GameController.Instance.NewGame();
    }

    public void OnClickQuit()
    {
        if (GameController.Instance != null) GameController.Instance.QuitGame();
    }
}