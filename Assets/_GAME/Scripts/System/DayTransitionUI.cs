using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Gắn script này lên 1 Panel PHỦ KÍN màn hình có component CanvasGroup, bên trong chứa 1 Text.
// Panel này giữ nguyên activeSelf = true xuyên suốt (KHÔNG SetActive tắt/bật), toàn bộ hiệu ứng
// ẩn/hiện chỉ điều khiển qua CanvasGroup.alpha - nhờ vậy coroutine không bị huỷ giữa chừng.
public class DayTransitionUI : MonoBehaviour
{
    [Tooltip("CanvasGroup của Panel chứa chữ 'Đêm thứ...' - dùng để fade in/out")]
    public CanvasGroup canvasGroup;

    [Tooltip("Text hiển thị 'Đêm thứ X'")]
    public Text dayText;

    [Header("Thời lượng (giây)")]
    public float fadeInDuration = 0.6f;
    public float holdDuration = 1.5f;
    public float fadeOutDuration = 0.8f;

    [Tooltip("Định dạng chữ hiển thị, {0} sẽ được thay bằng số ngày")]
    public string textFormat = "Đêm thứ {0}";

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false; // Không chặn thao tác của người chơi khi đang mờ/ẩn
            canvasGroup.interactable = false;
        }
    }

    // Gọi hàm này từ LightingManager mỗi khi sang ngày mới
    public void ShowDay(int dayNumber)
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("[DayTransitionUI]: Chưa gán CanvasGroup trong Inspector!");
            return;
        }

        if (dayText != null)
        {
            dayText.text = string.Format(textFormat, dayNumber);
        }

        // Nếu lỡ đang có 1 lần hiển thị trước đó chưa kết thúc, dừng nó lại trước khi bắt đầu
        // lần mới, tránh 2 coroutine tranh nhau chỉnh alpha.
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // FADE IN
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // GIỮ NGUYÊN để người chơi đọc chữ
        yield return new WaitForSeconds(holdDuration);

        // FADE OUT
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (t / fadeOutDuration));
            yield return null;
        }
        canvasGroup.alpha = 0f;

        currentRoutine = null;
    }
}