using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float rotationSpeed = 720f;
    public float jumpHeight = 2.5f;

    [Header("Thành phần liên kết")]
    public Animator animator;
    public Transform mainCamera;
    public GameObject waterOverlayUI;

    [Header("Âm thanh bước chân")]
    public AudioClip[] grassFootstepSounds;
    public float footstepIntervalWalk = 0.45f; // Khoảng thời gian giữa 2 bước khi đi bộ
    public float footstepIntervalRun = 0.28f;  // Khoảng thời gian giữa 2 bước khi chạy

    [Tooltip("Layer của mặt đất, dùng để bắn tia raycast xuống chân kiểm tra bề mặt")]
    public LayerMask groundLayerMask = ~0;

    // --- Biến nội bộ ---
    private CharacterController controller;
    private float gravity = -9.81f;
    private float velocityY = 0f;
    private float footstepTimer = 0f;

    private bool isInWater = false;
    private bool wasRunning = false;
    public float waterSpeedMultiplier = 0.5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (controller == null || !controller.enabled || !controller.gameObject.activeInHierarchy)
        {
            Debug.LogError($"[LỖI] Script PlayerMovement đang chạy trên '{gameObject.name}', nhưng CharacterController đang bị TẮT!");
            return;
        }

        // 1. Nhận phím bấm
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Dùng chung phím chạy (Sprint) với PlayerState để tránh bị lệch nút giữa 2 script.
        KeyCode sprintKey = PlayerState.Instance != null ? PlayerState.Instance.sprintKey : KeyCode.LeftShift;

        // Chỉ cho phép chạy nhanh nếu giữ phím VÀ còn thể lực (Stamina).
        // Hết thể lực -> tự động ép về đi bộ dù vẫn đang giữ phím, chờ đến khi hồi lại.
        bool isRunning = Input.GetKey(sprintKey) && (PlayerState.Instance == null || PlayerState.Instance.CanSprint);
        bool isJumpPressed = Input.GetButtonDown("Jump");

        // Chuột trái (0 = trái, 1 = phải, 2 = giữa)
        bool isSlashPressed = Input.GetMouseButtonDown(0);

        // 2. Tính toán tốc độ
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Persona: Cộng dồn % tốc độ di chuyển từ cây kỹ năng đã mở khóa, tính cho cả đi bộ lẫn chạy
        float moveSpeedMultiplier = 1f + (PersonaManager.Instance != null ? PersonaManager.Instance.moveSpeedBonus : 0f);
        currentSpeed *= moveSpeedMultiplier;

        if (isInWater)
        {
            currentSpeed *= waterSpeedMultiplier;  
        }
        // 3. Tính hướng di chuyển
        Vector3 moveDirection = Vector3.zero;

        if (mainCamera != null)
        {
            Vector3 camForward = mainCamera.forward;
            Vector3 camRight = mainCamera.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * vertical + camRight * horizontal).normalized;
        }
        else
        {
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }

        bool isActuallyMoving = moveDirection.magnitude >= 0.1f;

        // --- NGẮT TIẾNG CHẠY NHANH KHI DỪNG CHẠY ---
        if (SoundManager.Instance != null)
        {
            // Nếu frame trước đang chạy mà frame này nhả phím (hoặc đứng im không di chuyển)
            if ((wasRunning && !isRunning) || !isActuallyMoving)
            {
                SoundManager.Instance.StopSound(SoundManager.Instance.grassSprintSound);
            }
        }
        wasRunning = isRunning; // Lưu lại trạng thái để frame sau check tiếp

        // 4. Xử lý trọng lực và Nhảy
        if (controller.isGrounded)
        {
            velocityY = -0.5f;
            if (isJumpPressed)
            {
                velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (animator != null)
                    animator.SetTrigger("Jumping");
            }
        }
        else
        {
            velocityY += gravity * Time.deltaTime;
        }

        // 5. Xử lý Di chuyển & Xoay nhân vật
        if (isActuallyMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Vector3 moveVelocity = moveDirection * currentSpeed;
            moveVelocity.y = velocityY;
            controller.Move(moveVelocity * Time.deltaTime);

            if (animator != null)
            {
                animator.SetBool("isMoving", true);
                animator.SetBool("isRunning", isRunning);
            }
        }
        else
        {
            Vector3 fallVelocity = new Vector3(0, velocityY, 0);
            controller.Move(fallVelocity * Time.deltaTime);

            if (animator != null)
            {
                animator.SetBool("isMoving", false);
                animator.SetBool("isRunning", false);
            }
        }

        // 6. Xử lý tiếng bước chân
        if (controller.isGrounded && isActuallyMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootstepSound(isRunning);
                // Cập nhật lại nhịp bước chân nhanh hay chậm tùy trạng thái
                footstepTimer = isRunning ? footstepIntervalRun : footstepIntervalWalk;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void PlayFootstepSound(bool isRunning)
    {
        // 1. Nếu đang lội nước -> Ưu tiên phát tiếng nước rồi kết thúc hàm luôn
        if (isInWater && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.waterWalkSound);
            return;
        }

        // 2. Nếu ở trên mặt đất -> Bắn tia Raycast xuống đất để check xem đang dẫm lên cái gì
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1.5f, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Grass") && SoundManager.Instance != null)
            {
                // Phân loại phát tiếng chạy hoặc đi bộ
                if (isRunning)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.grassSprintSound);
                }
                else
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.grassWalkSound);
                }
            }
            // Mở rộng sau này: nếu thêm các bề mặt khác như gỗ, đá... thì cứ viết thêm ở đây
            // else if (hit.collider.CompareTag("Stone")) { ... }
        }
    }

    // --- XỬ LÝ VA CHẠM VÙNG (TRIGGER) ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;

            // Bật màn hình xanh
            if (waterOverlayUI != null)
            {
                waterOverlayUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false; // Đã lên bờ

            // Tắt màn hình xanh
            if (waterOverlayUI != null)
            {
                waterOverlayUI.SetActive(false);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopSound(SoundManager.Instance.waterWalkSound);
            }
        }
    }
}