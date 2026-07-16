using UnityEngine;
using UnityEngine.UI;


public class DeathScreenUI : MonoBehaviour
{
    [Tooltip("Kéo 1 RawImage phủ toàn màn hình vào đây để làm nền mờ lúc chết")]
    public RawImage backgroundImage;

    void Start()
    {
        if (backgroundImage != null && GameController.LastDeathScreenshot != null)
        {
            backgroundImage.texture = GameController.LastDeathScreenshot;
        }
    }

    // Gắn vào Button "Quay lại" (load lại Canh1 HOÀN TOÀN MỚI, không còn save để rollback)
    public void OnClickRestart()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.RestartGame();
        }
        else
        {
            Debug.LogWarning("[DeathScreenUI]: Không tìm thấy GameController.Instance!");
        }
    }

    //  Gắn vào Button "Về Menu" 
    public void OnClickMainMenu()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.GoToMainMenu();
        }
    }
}