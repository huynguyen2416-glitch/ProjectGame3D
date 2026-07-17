using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DayTransitionUI : MonoBehaviour
{
    // Sử dụng CanvasGroup thay vì bật tắt GameObject. 
    public CanvasGroup canvasGroup;
    public Text dayText;

    [Header("Timing Settings")]
    public float fadeInDuration = 0.6f;
    public float holdDuration = 1.5f;
    public float fadeOutDuration = 0.8f;
    public string textFormat = "Đêm thứ {0}";

    private Coroutine currentRoutine;

    private void Awake()
    {
        // Khởi tạo ẩn hoàn toàn UI lúc vừa vào màn chơi.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false; // Tắt tính năng chặn tia chuột (Raycast)
            canvasGroup.interactable = false;
        }
    }

    public void ShowDay(int dayNumber)
    {
        if (canvasGroup == null) return;
        if (dayText != null) dayText.text = string.Format(textFormat, dayNumber);

        // Cơ chế dọn dẹp Coroutine cũ. 
        // Nếu trời tối đột ngột khi hiệu ứng cũ chưa chạy xong, lệnh này sẽ ngắt luồng hoạt họa cũ ngay lập tức để chạy luồng mới, tránh hiện tượng hai luồng tranh chấp chỉ số Alpha gây nhấp nháy màn hình.
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration); 
            yield return null; 
        }
        canvasGroup.alpha = 1f;

        // Đóng băng trạng thái hiển thị rõ ràng trong một khoảng thời gian cố định
        yield return new WaitForSeconds(holdDuration);
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (t / fadeOutDuration));
            yield return null;
        }
        canvasGroup.alpha = 0f;

        currentRoutine = null; // Trả luồng về trống để chuẩn bị cho đêm tiếp theo
    }
}