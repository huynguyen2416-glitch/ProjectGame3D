using UnityEngine;

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
		if (animator == null) animator = GetComponentInChildren<Animator>();
		if (mainCamera == null) mainCamera = Camera.main.transform;
	}

	void Update()
	{
		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");

		// KIỂM TRA PHÍM SHIFT: Trả về true nếu đang đè Left Shift
		bool isRunning = Input.GetKey(KeyCode.LeftShift);

		// Chọn tốc độ dựa trên việc có đè Shift hay không
		float currentSpeed = isRunning ? runSpeed : walkSpeed;

		Vector3 camForward = mainCamera.forward;
		Vector3 camRight = mainCamera.right;
		camForward.y = 0f;
		camRight.y = 0f;
		camForward.Normalize();
		camRight.Normalize();

		Vector3 moveDirection = (camForward * vertical + camRight * horizontal).normalized;

		if (controller.isGrounded) { velocityY = -0.5f; }
		else { velocityY += gravity * Time.deltaTime; }

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