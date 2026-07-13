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

	private CharacterController controller;
	private float gravity = -9.81f;
	private float velocityY = 0f;

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

		bool isRunning = Input.GetKey(KeyCode.LeftShift);
		bool isJumpPressed = Input.GetButtonDown("Jump");

		// THÊM MỚI: Nhận tín hiệu Chuột trái (0 là trái, 1 là phải, 2 là chuột giữa)
		bool isSlashPressed = Input.GetMouseButtonDown(0);

		float currentSpeed = isRunning ? runSpeed : walkSpeed;
		Vector3 moveDirection = Vector3.zero;

		if (mainCamera != null)
		{
			Vector3 camForward = mainCamera.forward;
			Vector3 camRight = mainCamera.right;
			camForward.y = 0f;
			camRight.y = 0f;
			camForward.Normalize();
			camRight.Normalize();

<<<<<<< HEAD
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Persona (MoveSpeedBonus): % cộng dồn từ các PersonaUpgradeSO đã mở khoá, áp dụng cho cả đi bộ lẫn chạy
        float moveSpeedMultiplier = 1f + (PersonaManager.Instance != null ? PersonaManager.Instance.moveSpeedBonus : 0f);
        currentSpeed *= moveSpeedMultiplier;

        Vector3 moveDirection = Vector3.zero;
=======
			moveDirection = (camForward * vertical + camRight * horizontal).normalized;
		}
		else
		{
			moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
		}
>>>>>>> parent of a57ad63 (tạm thời như v)

		//if (isSlashPressed)
		{
			//if (animator != null)
			//	animator.SetTrigger("Slash");
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

		// Xử lý Di chuyển & Xoay
		if (moveDirection.magnitude >= 0.1f)
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

<<<<<<< HEAD
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






        // ÂM THANH BƯỚC CHÂN
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
=======
			if (animator != null)
			{
				animator.SetBool("isMoving", moveDirection.magnitude >= 0.1f);
				animator.SetBool("isRunning", isRunning && moveDirection.magnitude >= 0.1f);
			}
		}
	}
>>>>>>> parent of a57ad63 (tạm thời như v)
}