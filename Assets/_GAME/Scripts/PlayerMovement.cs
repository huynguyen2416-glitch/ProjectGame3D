using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float rotationSpeed = 720f;
    public float jumpHeight = 1.5f;

    public Animator animator;
    public Transform mainCamera;

    [Header("Âm thanh bước chân")]
    public AudioClip[] grassFootstepSounds;
    public float footstepIntervalWalk = 0.45f; // Khoảng cách thời gian giữa 2 bước khi đi bộ
    public float footstepIntervalRun = 0.28f;  // Khoảng cách thời gian giữa 2 bước khi chạy
    [Tooltip("Layer của mặt đất, dùng để raycast xuống chân xác định bề mặt")]
    public LayerMask groundLayerMask = ~0;

    private CharacterController controller;
    private float gravity = -9.81f;
    private float velocityY = 0f;
    private float footstepTimer = 0f;

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
            Debug.LogError($"[SỬA LỖI] Script PlayerMovement đang chạy trên Object: '{gameObject.name}', nhưng CharacterController bị TẮT hoặc INACTIVE!");
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Dùng chung phím Sprint với PlayerState (mặc định LeftShift) để tránh lệch key giữa 2 script.
        KeyCode sprintKey = PlayerState.Instance != null ? PlayerState.Instance.sprintKey : KeyCode.LeftShift;

        // Chỉ được chạy nước rút (isRunning) nếu giữ phím Sprint VÀ còn Stamina (CanSprint).
        // Hết thể lực -> tự động ép về đi bộ dù vẫn đang giữ Shift, cho tới khi Stamina hồi lại > 0.
        bool isRunning = Input.GetKey(sprintKey) && (PlayerState.Instance == null || PlayerState.Instance.CanSprint);
        bool isJumpPressed = Input.GetButtonDown("Jump");

        //  Nhận tín hiệu Chuột trái (0 là trái, 1 là phải, 2 là chuột giữa)
        bool isSlashPressed = Input.GetMouseButtonDown(0);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Persona (MoveSpeedBonus): % cộng dồn từ các PersonaUpgradeSO đã mở khoá, áp dụng cho cả đi bộ lẫn chạy
        float moveSpeedMultiplier = 1f + (PersonaManager.Instance != null ? PersonaManager.Instance.moveSpeedBonus : 0f);
        currentSpeed *= moveSpeedMultiplier;

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


        // Xử lý trọng lực và Nhảy
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

        bool isActuallyMoving = moveDirection.magnitude >= 0.1f;

        // Xử lý Di chuyển & Xoay
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
                animator.SetBool("isMoving", moveDirection.magnitude >= 0.1f);
                animator.SetBool("isRunning", isRunning && moveDirection.magnitude >= 0.1f);
            }
        }






        // Play footsteps while the player is moving on the ground.
        if (controller.isGrounded && isActuallyMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                // Gọi hàm phát âm thanh (không cần truyền biến isRunning nữa)
                PlayFootstepSound();

                // Nhịp độ sẽ tự động nhanh hơn khi isRunning = true
                footstepTimer = isRunning ? footstepIntervalRun : footstepIntervalWalk;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void PlayFootstepSound()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1.5f, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Grass") && SoundManager.Instance != null)
            {
                // Luôn phát tiếng bước chân đi bộ
                SoundManager.Instance.PlaySound(SoundManager.Instance.grassWalkSound);
            }
            // Sau này làm thêm tiếng bước trên đá, gỗ... thì cứ:
            // else if (hit.collider.CompareTag("Stone")) { ... } 
        }
    }
}