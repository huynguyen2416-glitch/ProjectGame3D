using UnityEngine;
using UnityEngine.UI;

public class DeathScreenUI : MonoBehaviour
{
    public RawImage backgroundImage;

    void Start()
    {
        // Cơ chế nạp cảnh khi run time
        // Lấy bức ảnh chụp màn hình lúc nhân vật vừa hết máu từ GameController gán vào rawimage
        if (backgroundImage != null && GameController.LastDeathScreenshot != null)
        {
            backgroundImage.texture = GameController.LastDeathScreenshot;
        }
    }

    public void OnClickRestart()
    {
        if (GameController.Instance != null) GameController.Instance.RestartGame();
    }

    public void OnClickMainMenu()
    {
        if (GameController.Instance != null) GameController.Instance.GoToMainMenu();
    }
}