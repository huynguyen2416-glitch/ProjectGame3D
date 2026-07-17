using UnityEngine;

public class CreditRoller : MonoBehaviour
{
    // Hàm sự kiện này được cấu hình để gắn vào nút "Bắt đầu lại" sau khi credit chạy xong 
    public void ResetGameAndReturn()
    {
        if (GameController.Instance != null) GameController.Instance.GoToMainMenu(); //về menu start
    }
}