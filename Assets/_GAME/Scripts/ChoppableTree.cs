using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ChoppableTree : MonoBehaviour
{
    public bool playerInRange;
    public bool canBeChopped;

    public float treeMaxHealth;
    public float treeHealth;

    [Tooltip("Thời gian chờ giữa mỗi nhát chặt (giây) TRƯỚC KHI áp bonus tốc độ chặt từ Persona")]
    public float baseHitDelay = 0.6f;

    [Header("Hierarchy - thống nhất cấu trúc: TreeParent > Prefab > TreeBase (script này)")]
    [Tooltip("Kéo transform GỐC của cả cụm cây (TreeParent, cấp cao nhất trong prefab) vào đây. " +
             "Nếu để trống, script sẽ tự dò 2 cấp cha như code cũ (có cảnh báo log).")]
    public Transform rootToDestroy;

    [Tooltip("Tên prefab cây gãy trong thư mục Resources, mặc định 'ChoppedTree'")]
    public string brokenPrefabName = "ChoppedTree";
    private bool isBeingHit; // Chặn nhiều coroutine hit() chạy chồng nhau
    private bool isDead;     // Chặn TreeIssDead() bị gọi 2 lần


    private void Start()
    {
        treeHealth = treeMaxHealth;
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
        if (isDead || isBeingHit) return;

        StartCoroutine(hit());
    }

    public IEnumerator hit()
    {
        isBeingHit = true;
        float harvestSpeedBonus = PersonaManager.Instance != null ? PersonaManager.Instance.harvestSpeedBonus : 0f;
        float actualDelay = Mathf.Max(0.05f, baseHitDelay * (1f - harvestSpeedBonus));

        yield return new WaitForSeconds(actualDelay);

        treeHealth -= 1;

        // Đồng bộ trừ máu ngay lập tức ko phụ thuộc raycast
        SyncGlobalState();

        if (treeHealth <= 0)
        {
            TreeIssDead();
        }

        isBeingHit = false;
    }

    private void SyncGlobalState()
    {
        if (GlobalState.Instance == null)
        {
            Debug.LogWarning("[ChoppableTree]: GlobalState.Instance đang null - máu cây có giảm thật (biến treeHealth) nhưng KHÔNG hiển thị lên thanh máu. Kiểm tra scene có object nào gắn script GlobalState.cs không.");
            return;
        }
        GlobalState.Instance.resourceHealth = treeHealth;
        GlobalState.Instance.resourceMaxHealth = treeMaxHealth;
    }

    void TreeIssDead()
    {
        if (isDead) return; // chặn gọi trùng nếu có race
        isDead = true;

        Vector3 treePosition = transform.position;

        GameObject objectToDestroy;
        if (rootToDestroy != null)
        {
            objectToDestroy = rootToDestroy.gameObject;
        }
        else
        {
            Debug.LogWarning("[ChoppableTree]: 'Root To Destroy' chưa được gán trong Inspector, " +
                              "đang fallback về transform.parent.parent (dễ sai nếu cấu trúc prefab khác). " +
                              "Nên kéo object cha gốc của cụm cây vào field 'Root To Destroy'.");
            objectToDestroy = transform.parent != null && transform.parent.parent != null
                ? transform.parent.parent.gameObject
                : gameObject;
        }

        canBeChopped = false;
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.selectedTree = null;
            if (SelectionManager.Instance.chopHolder != null)
                SelectionManager.Instance.chopHolder.gameObject.SetActive(false);
        }


        Destroy(objectToDestroy);

        if (!string.IsNullOrEmpty(brokenPrefabName))
        {
            GameObject brokenTree = Resources.Load<GameObject>(brokenPrefabName);
            if (brokenTree != null)
            {
                Instantiate(brokenTree, treePosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"[ChoppableTree]: Không tìm thấy prefab '{brokenPrefabName}' trong Resources!");
            }
        }
    }

    private void Update()
    {
        if (canBeChopped)
        {
            SyncGlobalState();
        }
    }

}