using UnityEngine;
using UnityEngine.UI; // Bắt buộc phải thêm dòng này để làm việc với ScrollRect

public class MapManager : MonoBehaviour
{
    [Header("--- UI References ---")]
    public GameObject worldMapPanel;
    public GameObject minimapUI;

    [Tooltip("Kéo GameObject MapContent vào đây")]
    public RectTransform mapContent;

    [Header("--- Zoom Settings ---")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 3f;

    void Start()
    {
        // Mới vào game: Hiện bản đồ nhỏ, ẩn bản đồ to 
        if (worldMapPanel != null) worldMapPanel.SetActive(false);
        if (minimapUI != null) minimapUI.SetActive(true);
    }

    void Update()
    {
        HandleMapToggle();

        // Nếu bản đồ đang mở thì cho phép Zoom
        if (worldMapPanel != null && worldMapPanel.activeSelf)
        {
            HandleMapZoom();
        }
    }

    private void HandleMapToggle()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (worldMapPanel == null) return;

            bool isWorldMapActive = !worldMapPanel.activeSelf;
            worldMapPanel.SetActive(isWorldMapActive);

            if (minimapUI != null)
            {
                minimapUI.SetActive(!isWorldMapActive);
            }
            if (isWorldMapActive)
            {
                // 1. Khi MỞ bản đồ: Thả tự do chuột và hiện con trỏ để tương tác UI
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // 2. Khi ĐÓNG bản đồ: Khóa chuột lại vào tâm và ẩn đi để chơi tiếp
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void HandleMapZoom()
    {
        if (mapContent == null) return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            Vector3 newScale = mapContent.localScale + Vector3.one * scroll * zoomSpeed;

            newScale.x = Mathf.Clamp(newScale.x, minZoom, maxZoom);
            newScale.y = Mathf.Clamp(newScale.y, minZoom, maxZoom);
            newScale.z = 1f;

            mapContent.localScale = newScale;
        }
    }
}