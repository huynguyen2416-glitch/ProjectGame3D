using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public class BossAI : MonoBehaviour
{
    public enum State { Idle, Chase, Attack, Taunt, SpecialAttack }
    public State currentState = State.Idle;

    public float activationRadius = 15f; // Người chơi bước vào phạm vi 15m thì Boss mới đuổi
    private bool isAwake = false; // Đánh dấu Boss đã bị đánh thức chưa

    [Header("References")]
    public Transform player;
    public Animator anim;
    private EnemyHealth healthScript;

    [Header("Basic Attack Settings")]
    public float moveSpeed = 3.5f;
    public float attackDistance = 3f;
    public float attackCooldown = 1.5f;
    public float damageAmount = 30f;

    [Header("Summon Minions (Phase 2)")]
    public GameObject minionPrefab; // Kéo Prefab con gấu thường vào đây
    public int minionCount = 3; // Số lượng gấu gọi ra mỗi lần gầm

    [Header("Special AOE Attack (Jump)")]
    public float specialAttackCooldown = 12f; // Bao lâu xài 1 lần
    public float aoeRadius = 6f; // Bán kính sát thương
    public float aoeDamage = 60f; // Sát thương diện rộng

    
    public float recoverBuffer = 0.5f;

    private float attackTimer = 0f;
    private float specialTimer = 5f; // Mới vào game 5s sau mới xài chiêu này
    private bool isTaunting = false;
    private bool isSpecialAttacking = false;

    // Cờ nhận tín hiệu từ Animation Event - KHÔNG set tay từ code, chỉ đặt Event trong Animation window gọi tới
    private bool warnComplete = false;
    private bool landingImpactHappened = false;
    private bool tauntRoarPeakHappened = false;
    private bool tauntComplete = false;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (anim == null) anim = GetComponent<Animator>();

        healthScript = GetComponent<EnemyHealth>();
        if (healthScript != null)
        {
            healthScript.OnPhase2Entered += TriggerTaunt;
        }
    }

    void OnDestroy()
    {
        if (healthScript != null)
        {
            healthScript.OnPhase2Entered -= TriggerTaunt;
        }
    }

    void Update()
    {
        // Đang giữa 1 coroutine điều khiển riêng (Taunt/SpecialAttack) hoặc đã chết -> Update không can thiệp
        if (isTaunting || isSpecialAttacking) return;
        if (healthScript != null && healthScript.currentHealth <= 0) return;

        if (currentState == State.Idle)
        {
            WaitInZone();
            return;
        }

        if (!isAwake) return;

        if (specialTimer > 0) specialTimer -= Time.deltaTime;
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        // Đủ điều kiện tung chiêu AOE thì ưu tiên trước, ngắt hẳn state machine thường - CHỈ 1 lần kiểm tra duy nhất
        if (specialTimer <= 0f && player != null && HorizontalDistance(transform.position, player.position) <= aoeRadius * 1.5f)
        {
            StartCoroutine(JumpAttackRoutine());
            return;
        }

        switch (currentState)
        {
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }
    }

    void WaitInZone()
    {
        if (player == null) return;

        if (anim != null)
        {
            anim.SetBool("walk", false);
            anim.SetBool("run", false);
            anim.SetBool("atk", false);
        }

        if (HorizontalDistance(transform.position, player.position) <= activationRadius)
        {
            isAwake = true;
            currentState = State.Chase;
            Debug.Log("[Boss] Kẻ xâm nhập đã vào vùng! BẮT ĐẦU RƯỢT!");
        }
    }

    void Chase()
    {
        if (player == null) return;

        if (anim != null)
        {
            anim.SetBool("walk", false);
            anim.SetBool("run", true);
            anim.SetBool("atk", false);
        }

        MoveTowards(player.position, moveSpeed);

        if (HorizontalDistance(transform.position, player.position) <= attackDistance)
        {
            currentState = State.Attack;
        }
    }

    void Attack()
    {
        if (player == null) return;

        if (anim != null)
        {
            anim.SetBool("atk", true);
            anim.SetBool("run", false);
            anim.SetBool("walk", false);
        }

        // Xoay theo player: bắt đầu 1 nhát vung MỚI (attackTimer vừa hết) -> CHỐT HƯỚNG NGAY LẬP TỨC,
        // không mượt hoá, để lúc animation chạm khung hình đánh trúng thì cơ thể đã xoay xong, không bị trễ
        // dẫn tới vung hụt. Còn lúc đang hồi chiêu (chưa tới lượt vung) thì xoay mượt bằng Slerp cho tự nhiên.
        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        bool startingNewSwing = attackTimer <= 0f;

        if (Vector3.Distance(transform.position, lookPos) > 0.1f)
        {
            Vector3 direction = (lookPos - transform.position).normalized;
            if (startingNewSwing)
            {
                transform.rotation = Quaternion.LookRotation(direction); // chốt cứng, không trễ
            }
            else
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
        }

        if (HorizontalDistance(transform.position, player.position) > attackDistance)
        {
            currentState = State.Chase;
            if (anim != null) anim.SetBool("atk", false);
            return;
        }

        if (attackTimer <= 0f) attackTimer = attackCooldown;
    }

    // ANIMATION EVENT - đặt đúng khung hình "chạm trúng" của clip đánh thường
    public void ExecuteAttackDamage()
    {
        if (player == null || healthScript.currentHealth <= 0) return;
        if (HorizontalDistance(transform.position, player.position) <= attackDistance + 1.5f)
        {
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.setHealth(PlayerState.Instance.currentHealth - damageAmount);
                Debug.Log("[Boss] Cào trúng đích! Máu người chơi giảm " + damageAmount);
            }
        }
        else
        {
            Debug.Log("[Boss] ĐÁNH HỤT!");
        }
    }

    // LOGIC JUMP ATTACK (AOE)

    private IEnumerator JumpAttackRoutine()
    {
        isSpecialAttacking = true;
        currentState = State.SpecialAttack;

        // 1. Xoay mặt về phía người chơi trước khi gồng
        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);

        if (anim != null)
        {
            anim.SetBool("run", false);
            anim.SetBool("atk", false);
            anim.SetTrigger("warn");
        }
        Debug.Log("[Boss] CẢNH BÁO: Đang gồng chiêu AOE!");

        // 2. Chờ khung hình cuối của clip gồng (nhận tín hiệu từ Animation Event)
        warnComplete = false;
        yield return new WaitUntil(() => warnComplete);

        // 3. Chuyển sang clip nhảy dậm
        if (anim != null) anim.SetTrigger("jumpAtk");
        Debug.Log("[Boss] Bắt đầu dậm AOE!");

        // 4. Chờ đúng lúc tay/chân Boss chạm đất (nhận tín hiệu từ Animation Event để nổ Damage)
        landingImpactHappened = false;
        yield return new WaitUntil(() => landingImpactHappened);

        // 5. Đệm một khoảng thời gian ngắn để chờ hoạt ảnh Boss thu thế, đứng thẳng dậy
        yield return new WaitForSeconds(recoverBuffer);

        // 6. Hoàn tất, quay lại rượt đuổi
        specialTimer = specialAttackCooldown;
        isSpecialAttacking = false;
        currentState = State.Chase;
    }

    // ANIMATION EVENT
    public void AE_WarnComplete()
    {
        warnComplete = true;
    }

    // ANIMATION EVENT 
    public void AE_LandingImpact()
    {
        landingImpactHappened = true;
        ExecuteAOEDamage();
    }

    private void ExecuteAOEDamage()
    {
        if (player == null || healthScript.currentHealth <= 0) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= aoeRadius)
        {
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.setHealth(PlayerState.Instance.currentHealth - aoeDamage);
                Debug.Log("[Boss] Nổ AOE trúng người chơi! Nhận " + aoeDamage + " sát thương!");
            }
        }
        else
        {
            Debug.Log("[Boss] Người chơi đã né được AOE thành công!");
        }
    }


    // LOGIC TAUNT (GỌI BẦY GẤU) VÀO PHASE 2

    private void TriggerTaunt()
    {
        if (isTaunting) return;
        StartCoroutine(TauntRoutine());
    }

    private IEnumerator TauntRoutine()
    {
        isTaunting = true;
        currentState = State.Taunt;

        if (anim != null)
        {
            anim.SetBool("run", false);
            anim.SetBool("walk", false);
            anim.SetBool("atk", false);
            anim.SetTrigger("taunt");
        }

        // Chờ đúng lúc Boss gầm to nhất (Animation Event AE_TauntRoarPeak) mới triệu hồi gấu,
        tauntRoarPeakHappened = false;
        yield return new WaitUntil(() => tauntRoarPeakHappened);
        // Chờ animation taunt kết thúc hẳn (Animation Event AE_TauntComplete)
        tauntComplete = false;
        yield return new WaitUntil(() => tauntComplete);

        moveSpeed *= 1.35f;
        specialAttackCooldown = 5f;
        specialTimer = 0f;

        isTaunting = false;
        currentState = State.Chase;
    }

    // ANIMATION EVENT 
    public void AE_TauntRoarPeak()
    {
        tauntRoarPeakHappened = true;
    }

    // ANIMATION EVENT
    public void AE_TauntComplete()
    {
        tauntComplete = true;
    }


    // UTILS
    float HorizontalDistance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 lookPos = new Vector3(target.x, transform.position.y, target.z);
        if (Vector3.Distance(transform.position, lookPos) > 0.1f)
        {
            // Đi tới
            transform.position += (lookPos - transform.position).normalized * speed * Time.deltaTime;

            // Xoay mặt mượt mà thay vì giật cục (thình lình quay mặt)
            Quaternion targetRot = Quaternion.LookRotation((lookPos - transform.position).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }
}