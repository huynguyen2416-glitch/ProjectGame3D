using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public string gameSceneName = "Canh1";

    // Hàm gắn cho nút Bắt Đầu (NewGameButton)
    public void OnClick_BatDau()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.LoadGameScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Không tìm thấy MenuManager trong Scene!");
        }
    }

    // Hàm gắn cho nút Quay lại (ReloadButton)
    public void OnClick_QuayLai()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ReloadCurrentScene();
        }
    }

    // Hàm gắn cho nút Thoát (ExitButton)
    public void OnClick_Thoat()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.QuitApp();
        }
    }
}