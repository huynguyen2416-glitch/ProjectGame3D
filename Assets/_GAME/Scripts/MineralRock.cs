using System.Collections;
using UnityEngine;

// ⚠️ QUAN TRỌNG: Tên FILE này (MineableRock.cs) PHẢI trùng tên CLASS bên dưới.
// Đây là lý do đập đá không mất máu trước đây: file cũ tên "MineralRock.cs"
// nhưng class là "MineableRock" -> Unity không gắn được script vào GameObject
// -> GetComponent<MineableRock>() luôn null -> GetHit() không bao giờ chạy.
[RequireComponent(typeof(BoxCollider))]
public class MineableRock : MonoBehaviour
{
    public bool playerInRange;
    public bool canBeMined;

    public float rockMaxHealth;
    public float rockHealth;

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

        yield return new WaitForSeconds(0.6f);

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
        if (GlobalState.Instance == null) return;
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

        Destroy(objectToDestroy);

        if (!string.IsNullOrEmpty(brokenPrefabName))
        {
            GameObject brokenPrefab = Resources.Load<GameObject>(brokenPrefabName);
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