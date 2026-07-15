using UnityEngine;

public class CreditRoller : MonoBehaviour
{
    // Hàm này sẽ được gọi khi người chơi bấm nút "Bắt đầu lại"
    public void ResetGameAndReturn()
    {
        // 1. Xóa sạch file save cũ bằng hàm DeleteSave có sẵn
        SaveSystem.DeleteSave();

        // 2. Gọi GameController đưa về Main Menu
        if (GameController.Instance != null)
        {
            GameController.Instance.GoToMainMenu();
        }
        else
        {
            Debug.LogError("Không tìm thấy GameController!");
        }
    }
}