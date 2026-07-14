using UnityEngine;
using System.Collections;

public class CreditRoller : MonoBehaviour
{
    public float scrollSpeed = 50f;
    public float waitTimeAtEnd = 3f; // Đợi mấy giây sau khi cuộn xong
    public float stopYPosition = 1000f; // Tọa độ Y mà chữ sẽ dừng lại (tùy độ dài đoạn text của bác)

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        StartCoroutine(RollCredits());
    }

    IEnumerator RollCredits()
    {
        // Cuộn chữ lên từ từ
        while (rectTransform.anchoredPosition.y < stopYPosition)
        {
            rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            yield return null;
        }

        // Đợi một chút cho người chơi đọc chữ "Cảm ơn đã chơi"
        yield return new WaitForSeconds(waitTimeAtEnd);

        // Gọi GameController đưa về Main Menu
        if (GameController.Instance != null)
        {
            GameController.Instance.GoToMainMenu();
        }
    }
}