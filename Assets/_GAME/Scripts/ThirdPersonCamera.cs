using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform target;          // Nhân vật 

    [Header("Cấu hình Camera (Phong cách Wukong)")]
    public float distance = 2.5f;     
    public float sensitivity = 3f;

    [Header("Độ lệch (Over-the-Shoulder)")]
    public float offsetX = 1.2f;      // Đẩy cam sang PHẢI 1.2m -> Nhân vật sẽ nằm bên TRÁI
    public float offsetY = 2f;      // Chiều cao ngắm tới (Ngang ngực/vai thay vì đỉnh đầu)

    [Header("Giới hạn góc nhìn ngước lên/cúi xuống")]
    public float minY = -15f;
    public float maxY = 60f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Đọc dữ liệu di chuyển từ chuột
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Tính toán góc quay của Camera
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);

        // ĐIỂM MẤU CHỐT TẠO GÓC WUKONG NẰM Ở ĐÂY:
        // Lấy hướng "sang phải" của camera nhân với offsetX để đẩy điểm nhìn lệch đi
        Vector3 rightOffset = rotation * Vector3.right * offsetX;

        // Vị trí mục tiêu bây giờ = Vị trí NV + Chiều cao (offsetY) + Lệch sang phải (rightOffset)
        Vector3 targetLookAtPos = target.position + (Vector3.up * offsetY) + rightOffset;

        // Kéo vị trí camera lùi về sau dựa trên khoảng cách (distance)
        Vector3 position = targetLookAtPos - (rotation * Vector3.forward * distance);

        // Áp dụng
        transform.rotation = rotation;
        transform.position = position;
    }
}