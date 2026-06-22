using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform target;          // Nhân vật (rp_nathan_...)

    [Header("Cấu hình Camera")]
    public float distance = 6f;       // Khoảng cách từ cam đến lưng nhân vật
    public float sensitivity = 3f;    // Tốc độ xoay/Độ nhạy của chuột

    [Header("Giới hạn góc nhìn ngước lên/cúi xuống")]
    public float minY = -10f;
    public float maxY = 60f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Ẩn con trỏ chuột và khóa nó vào giữa màn hình khi đang chơi game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Đọc dữ liệu di chuyển từ chuột trái/phải và lên/xuống
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;

        // Giới hạn góc để camera không bị lộn nhào lộn ngửa qua đầu nhân vật
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Tính toán góc quay của Camera
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);

        // Tính vị trí đứng của Camera (nhìn vào điểm ngang vai/đầu nhân vật cao hơn gốc chân 1.5m)
        Vector3 targetLookAtPos = target.position + Vector3.up * 1.5f;
        Vector3 position = targetLookAtPos - (rotation * Vector3.forward * distance);

        // Áp dụng vị trí và hướng nhìn cho Camera
        transform.rotation = rotation;
        transform.position = position;
    }
}