using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	private CharacterController controller;
	private Animator anim;

	[Header("Liên kết Camera")]
	public Transform cameraTransform;

	[Header("Cài đặt tốc độ")]
	public float walkSpeed = 3f;
	public float runSpeed = 8f; // Giảm từ 50 xuống 8 vì 50 quá nhanh, sẽ làm nhân vật xuyên tường
	public float turnSmoothTime = 0.1f;
	private float turnSmoothVelocity;

	public float gravity = -9.81f;
	private Vector3 velocity;

	void Start()
	{
		controller = GetComponent<CharacterController>();
		anim = GetComponent<Animator>();

		if (cameraTransform == null && Camera.main != null)
			cameraTransform = Camera.main.transform;
	}

	void Update()
	{
		if (controller == null) return;

		// 1. TRỌNG LỰC
		if (controller.isGrounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}

		// 2. DI CHUYỂN
		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");
		Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

		if (direction.magnitude >= 0.1f)
		{
			float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
			transform.rotation = Quaternion.Euler(0f, angle, 0f);

			Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

			float currentSpeed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? runSpeed : walkSpeed;

			// Di chuyển chính
			controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);

			if (anim != null)
			{
				anim.SetFloat("Speed", (currentSpeed == runSpeed) ? 1f : 0.5f);
			}
		}
		else
		{
			if (anim != null) anim.SetFloat("Speed", 0f);
		}

		// 3. ÁP DỤNG TRỌNG LỰC
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}
}