using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	public enum State { Patrol, Chase, Attack }
	public State currentState = State.Patrol;
	public float damageAmount = 10f;

	[Header("References")]
	public Transform[] patrolPoints;
	public Transform player;
	public Animator anim;

	[Header("Settings")]
	public float patrolSpeed = 2f;
	public float chaseSpeed = 4f;
	public float chaseDistance = 8f;
	public float attackDistance = 1.5f;
	public float attackCooldown = 1f;

	[Header("State Materials")]
	public Material patrolMaterial;
	public Material chaseMaterial;
	public Material attackMaterial;

	private Renderer rend;
	private int patrolIndex = 0;
	private float attackTimer = 0f;

	void Start()
	{
		rend = GetComponentInChildren<Renderer>();
		rend.material = patrolMaterial;

		if (anim == null)
			anim = GetComponent<Animator>();
	}

	void Update()
	{
		switch (currentState)
		{
			case State.Patrol:
				Patrol();
				break;

			case State.Chase:
				Chase();
				break;

			case State.Attack:
				Attack();
				break;
		}

		attackTimer -= Time.deltaTime;
	}

	// -------- PATROL -------- //
	void Patrol()
	{
		rend.material = patrolMaterial;

		if (anim != null)
		{
			anim.SetBool("walk", true);
			anim.SetBool("run", false);
		}

		Transform point = patrolPoints[patrolIndex];
		MoveTowards(point.position, patrolSpeed);

		if (Vector3.Distance(transform.position, point.position) < 0.3f)
			patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

		if (Vector3.Distance(transform.position, player.position) < chaseDistance)
			currentState = State.Chase;
	}

	// -------- CHASE -------- //
	void Chase()
	{
		rend.material = chaseMaterial;

		if (anim != null)
		{
			anim.SetBool("walk", false);
			anim.SetBool("run", true);
		}

		MoveTowards(player.position, chaseSpeed);

		float dist = Vector3.Distance(transform.position, player.position);

		if (dist > chaseDistance + 2f)
			currentState = State.Patrol;

		if (dist < attackDistance)
			currentState = State.Attack;
	}

	// -------- ATTACK -------- //
	void Attack()
	{
		rend.material = attackMaterial;

		if (anim != null)
		{
			anim.SetBool("atk", true);
			anim.SetBool("run", false);
		}

		transform.LookAt(player);
		float dist = Vector3.Distance(transform.position, player.position);

		// Nếu người chơi chạy ra khỏi tầm đánh
		if (dist > attackDistance)
		{
			currentState = State.Chase;
			anim.SetBool("atk", false); // Tắt animation tấn công
			return;
		}

		// Quản lý thời gian hồi chiêu (Cooldown) của animation
		if (attackTimer <= 0f)
		{
			// Reset thời gian chờ để không lặp animation quá nhanh
			attackTimer = attackCooldown;
		}
	}

	// -------- ANIMATION EVENT -------- //
	// HÀM MỚI: Kéo hàm này vào frame thứ 6 của Animation Attack
	public void ExecuteAttackDamage()
	{
		// Kiểm tra khoảng cách một lần nữa ngay tại thời điểm vung tay trúng
		float dist = Vector3.Distance(transform.position, player.position);
		if (dist <= attackDistance)
		{
			PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
			if (playerHealth != null)
			{
				// Trừ máu thông qua script PlayerHealth
				playerHealth.TakeDamage(damageAmount);
			}
		}
	}

	// -------- MOVEMENT -------- //
	void MoveTowards(Vector3 target, float speed)
	{
		Vector3 dir = (target - transform.position).normalized;
		transform.position += dir * speed * Time.deltaTime;
		transform.LookAt(target);
	}

	// -------- GIZMOS -------- //
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, chaseDistance);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackDistance);
	}
}