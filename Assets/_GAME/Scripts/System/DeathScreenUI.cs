using UnityEngine;
using UnityEngine.UI;

// Gắn script này lên 1 GameObject bất kỳ TRONG DeathScene (vd: Canvas).
// Vì GameController là singleton DontDestroyOnLoad được tạo từ StartScene, bạn KHÔNG THỂ
// kéo-thả trực tiếp GameObject GameController vào OnClick của Button trong DeathScene (nó chưa
// tồn tại lúc bạn đang edit scene này). Thay vào đó: kéo GameObject có gắn script NÀY vào
// OnClick của Button, và trỏ tới các hàm public bên dưới - script này gọi GameController.Instance
// bằng code lúc runtime.
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

    // Gắn vào Button "Quay lại" (load lại save gần nhất = đầu ngày hôm trước lúc chết)
    public void OnClickRestart()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.RestartFromLastSave();
        }
        else
        {
            Debug.LogWarning("[DeathScreenUI]: Không tìm thấy GameController.Instance!");
        }
    }

    // (Tùy chọn) Gắn vào Button "Về Menu" nếu bạn muốn có thêm lựa chọn này ở DeathScene
    public void OnClickMainMenu()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.GoToMainMenu();
        }
    }
}