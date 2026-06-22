using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;

    [Header("Liên kết Camera")]
    public Transform cameraTransform;

    [Header("Cài đặt tốc độ")]
    public float walkSpeed = 3f;       // Tốc độ đi bộ bình thường
    public float runSpeed = 50f;        // Tốc độ khi nhấn giữ Shift để chạy nhanh
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Hệ thống Trọng lực (Giúp đứng dưới đất)")]
    public float gravity = -9.81f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. XỬ LÝ TRỌNG LỰC
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 2. XỬ LÝ DI CHUYỂN
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // MẶC ĐỊNH LÀ TỐC ĐỘ ĐI BỘ
            float currentSpeed = walkSpeed;

            // KIỂM TRA NẾU NGƯỜI CHƠI NHẤN GIỮ NÚT SHIFT TRÁI HOẶC PHẢI
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentSpeed = runSpeed; // Đổi sang tốc độ chạy nhanh
                anim.SetFloat("Speed", 1f); // Kích hoạt animation chạy (hoặc chạy nhanh) dứt khoát
            }
            else
            {
                anim.SetFloat("Speed", 0.5f); // Khi đi bộ bình thường, đặt Speed vừa phải để Animator nhận diện
            }

            // Tính toán hướng di chuyển theo Camera xoay theo chuột
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Di chuyển nhân vật với tốc độ hiện tại (Walk hoặc Run)
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }
        else
        {
            anim.SetFloat("Speed", 0f); // Đứng im dứt khoát (Về trạng thái Idle)
        }

        // Áp dụng trọng lực kéo xuống đất liên tục
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}