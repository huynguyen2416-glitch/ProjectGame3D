using System.Security.Cryptography;
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
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        rend = GetComponentInChildren<Renderer>();
        if (rend != null && patrolMaterial != null) rend.material = patrolMaterial;
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }

        attackTimer -= Time.deltaTime;
    }

    // TUẦN TRA (PATROL)
    void Patrol()
    {
        if (rend != null && patrolMaterial != null) rend.material = patrolMaterial;
        if (anim != null)
        {
            anim.SetBool("walk", true);
            anim.SetBool("run", false);
            anim.SetBool("atk", false);
        }

        //  NẾU KHÔNG CÓ ĐIỂM TUẦN TRA -> Tự động ĐI BỘ từ từ về phía người chơi
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            if (player != null)
            {
                MoveTowards(player.position, patrolSpeed); // Dùng tốc độ đi bộ

                // Nếu đi bộ đến đủ gần (lọt vào tầm nhìn) thì mới bắt đầu CHẠY (Chase)
                if (HorizontalDistance(transform.position, player.position) < chaseDistance)
                {
                    currentState = State.Chase;
                }
            }
            return; // Thoát hàm để không chạy logic tuần tra bên dưới
        }

        //  NẾU CÓ ĐIỂM TUẦN TRA -> Đi lượn lờ theo các điểm đó (Dành cho gấu ban ngày)
        Transform point = patrolPoints[patrolIndex];
        MoveTowards(point.position, patrolSpeed);

        if (HorizontalDistance(transform.position, point.position) < 0.5f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

        // Thấy người chơi là chuyển sang chạy
        if (player != null && HorizontalDistance(transform.position, player.position) < chaseDistance)
            currentState = State.Chase;
    }

    // TRUY ĐUỔI (CHASE)
    void Chase()
    {
        if (player == null) return;
        if (rend != null && chaseMaterial != null) rend.material = chaseMaterial;

        if (anim != null)
        {
            anim.SetBool("walk", false);
            anim.SetBool("run", true);
            anim.SetBool("atk", false);
        }

        MoveTowards(player.position, chaseSpeed);

        float dist = HorizontalDistance(transform.position, player.position);
        if (dist > chaseDistance + 2f) currentState = State.Patrol;
        if (dist < attackDistance) currentState = State.Attack;
    }

    //ĐÒN TẤN CÔNG (ATK)
    void Attack()
    {
        if (player == null) return;
        if (rend != null && attackMaterial != null) rend.material = attackMaterial;

        if (anim != null)
        {
            anim.SetBool("atk", true);
            anim.SetBool("run", false);
            anim.SetBool("walk", false);
        }

        // Lấy tọa độ X và Z của người chơi, nhưng giữ nguyên trục Y của gấu (Không cho ngước lên/xuống)
        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);

        // Tránh lỗi khi khoảng cách = 0 thì LookAt bị xoay mòng mòng
        if (Vector3.Distance(transform.position, lookPos) > 0.1f)
        {
            transform.LookAt(lookPos);
        }

        float dist = HorizontalDistance(transform.position, player.position);
        if (dist > attackDistance)
        {
            currentState = State.Chase;
            if (anim != null) anim.SetBool("atk", false);
            return;
        }

        if (attackTimer <= 0f) attackTimer = attackCooldown;
    }

    //ANIMATION EVENT (GẤU VẢ NGƯỜI CHƠI)
    public void ExecuteAttackDamage()
    {
        if (player == null) return;
        float dist = HorizontalDistance(transform.position, player.position);
        if (dist <= attackDistance + 0.5f) // Cộng thêm 0.5f bù trừ sai số khi gấu vung tay
        {
            if (PlayerState.Instance != null)
            {
                float newHealth = PlayerState.Instance.currentHealth - damageAmount;
                if (newHealth < 0) newHealth = 0;
                PlayerState.Instance.setHealth(newHealth);
                Debug.Log("Gấu vả trúng! Máu còn: " + newHealth);
            }
        }
    }

    float HorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector3 flatA = new Vector3(a.x, 0f, a.z);
        Vector3 flatB = new Vector3(b.x, 0f, b.z);
        return Vector3.Distance(flatA, flatB);
    }

    //MOVEMENT 
    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 lookPos = new Vector3(target.x, transform.position.y, target.z);
        if (Vector3.Distance(transform.position, lookPos) > 0.1f)
        {
            Vector3 dir = (lookPos - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
            transform.LookAt(lookPos);
        }
    }
    public void ForceChasePlayer()
    {
        // 1. Chữa cháy lỗi Start() chưa kịp chạy bằng cách ép tìm Player ngay lập tức
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // 2. Chuyển state sang rượt đuổi nếu có player
        if (player != null)
        {
            currentState = State.Chase;
            chaseDistance = 999f; // Tầm nhìn vô hạn
        }

        // 3. Đưa các lệnh Buff và hóa điên ra ngoài để LUÔN ĐƯỢC KÍCH HOẠT
        chaseSpeed *= 1.5f;
        damageAmount *= 1.5f;

        if (rend != null && attackMaterial != null) rend.material = attackMaterial;

        // Đánh dấu con gấu này là tha hóa để KHÔNG RỚT THỊT NỮA
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null) health.isCorrupted = true;

        Debug.Log(gameObject.name + " đã bị Boss tha hóa! Không rớt thịt!");
    }
}