using System.Collections;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]

public class MineableRock : MonoBehaviour
{
    public bool playerInRange;
    public bool canBeMined;

    public float rockMaxHealth;
    public float rockHealth;

    [Tooltip("Thời gian chờ giữa mỗi nhát đập (giây) TRƯỚC KHI áp bonus tốc độ đập từ Persona")]
    public float baseHitDelay = 0.6f;

    [Header("Hierarchy - thống nhất cấu trúc: RockParent > Prefab > RockBase (script này)")]
    [Tooltip("Kéo transform GỐC của cả cụm đá (RockParent, cấp cao nhất trong prefab) vào đây. " +
             "Nếu để trống, script sẽ tự dò 2 cấp cha như code cũ (có cảnh báo log).")]
    public Transform rootToDestroy;

    [Tooltip("Tên prefab mảnh đá vỡ trong thư mục Resources, mặc định 'BrokenRock'")]
    public string brokenPrefabName = "BrokenRock";

    private bool isBeingHit; // Chặn nhiều coroutine hit() chạy chồng nhau
    private bool isDead;     // Chặn RockIsDead() bị gọi 2 lần


    private void Start()
    {
        rockHealth = rockMaxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void GetHit()
    {
        // Nếu đang có 1 nhát đập "trong quá trình xử lý" (chưa hết 0.6s) thì bỏ qua,
        // tránh nhiều coroutine hit() chạy song song rồi cùng trừ máu 1 lượt.
        if (isDead || isBeingHit) return;

        StartCoroutine(hit());
    }

    public IEnumerator hit()
    {
        isBeingHit = true;


        float harvestSpeedBonus = PersonaManager.Instance != null ? PersonaManager.Instance.harvestSpeedBonus : 0f;
        float actualDelay = Mathf.Max(0.05f, baseHitDelay * (1f - harvestSpeedBonus));

        yield return new WaitForSeconds(actualDelay);

        rockHealth -= 1;

        // Đồng bộ trừ máu ngay lập tức, không phụ thuộc raycast/canBeMined
        SyncGlobalState();

        if (rockHealth <= 0)
        {
            RockIsDead();
        }

        isBeingHit = false;
    }

    private void SyncGlobalState()
    {
        if (GlobalState.Instance == null)
        {
            Debug.LogWarning("[MineableRock]: GlobalState.Instance đang null - máu đá có giảm thật (biến rockHealth) nhưng KHÔNG hiển thị lên thanh máu. Kiểm tra scene có object nào gắn script GlobalState.cs không.");
            return;
        }
        GlobalState.Instance.resourceHealth = rockHealth;
        GlobalState.Instance.resourceMaxHealth = rockMaxHealth;
    }

    void RockIsDead()
    {
        if (isDead) return; // chặn gọi trùng nếu có race
        isDead = true;

        Vector3 rockPosition = transform.position;

        GameObject objectToDestroy;
        if (rootToDestroy != null)
        {
            objectToDestroy = rootToDestroy.gameObject;
        }
        else
        {
            Debug.LogWarning("[MineableRock]: 'Root To Destroy' chưa được gán trong Inspector, " +
                              "đang fallback về transform.parent.parent (dễ sai nếu cấu trúc prefab khác). " +
                              "Nên kéo object cha gốc của cụm đá vào field 'Root To Destroy'.");
            objectToDestroy = transform.parent != null && transform.parent.parent != null
                ? transform.parent.parent.gameObject
                : gameObject;
        }

        canBeMined = false;
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.selectedRock = null;
            if (SelectionManager.Instance.mineHolder != null)
                SelectionManager.Instance.mineHolder.gameObject.SetActive(false);
        }

        // PERSONA: đập vỡ thành công 1 cục đá -> +1 Điểm Sinh Tồn
        if (PersonaManager.Instance != null)
        {
            PersonaManager.Instance.AwardPoint(1, "Đập vỡ đá");
        }

        Destroy(objectToDestroy);

        if (!string.IsNullOrEmpty(brokenPrefabName))
        {
            GameObject brokenPrefab = ResourceCache.Load(brokenPrefabName);
            if (brokenPrefab != null)
            {
                Instantiate(brokenPrefab, rockPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"[MineableRock]: Không tìm thấy prefab '{brokenPrefabName}' trong Resources!");
            }
        }
    }

    private void Update()
    {
        if (canBeMined)
        {
            SyncGlobalState();
        }
    }

}