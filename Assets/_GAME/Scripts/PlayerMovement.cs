using UnityEngine;

[RequireComponent(typeof(CharacterController))] // Đảm bảo luôn có CharacterController
public class PlayerMovement : MonoBehaviour
{
	public float walkSpeed = 3f;  // Tốc độ khi đi bộ bình thường
	public float runSpeed = 7f;   // Tốc độ khi đè phím Shift
	public float rotationSpeed = 720f;

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

		// KIỂM TRA PHÍM SHIFT: Trả về true nếu đang đè Left Shift
		bool isRunning = Input.GetKey(KeyCode.LeftShift);

		// Chọn tốc độ dựa trên việc có đè Shift hay không
		float currentSpeed = isRunning ? runSpeed : walkSpeed;

		Vector3 moveDirection = Vector3.zero;

		// Xử lý hướng di chuyển theo Camera an toàn
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
			// Fallback nếu không có camera: di chuyển theo trục thế giới (World Space)
			moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
		}

		// Xử lý trọng lực
		if (controller.isGrounded)
		{
			velocityY = -0.5f; // Giữ nhân vật bám đất tốt hơn
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
				animator.SetBool("isRunning", isRunning); // Truyền lệnh chạy sang Animator
			}
		}
		else
		{
			// Xử lý khi đứng im (chỉ áp dụng trọng lực rơi xuống)
			Vector3 fallVelocity = new Vector3(0, velocityY, 0);
			controller.Move(fallVelocity * Time.deltaTime);

			if (animator != null)
			{
				animator.SetBool("isMoving", false);
				animator.SetBool("isRunning", false);
			}
		}
	}
}