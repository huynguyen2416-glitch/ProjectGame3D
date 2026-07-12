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

    [Tooltip("Object gốc của cả cụm cây cần Destroy khi cây chết. Nếu để trống sẽ fallback về transform.parent.parent.")]
    public Transform rootToDestroy;

    [Tooltip("Tên prefab cây gãy trong Resources")]
    public string brokenPrefabName = "ChoppedTree";

    private bool isBeingHit; // chặn nhiều coroutine hit() chạy chồng nhau
    private bool isDead;     // chặn TreeIssDead() bị gọi 2 lần

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

        yield return new WaitForSeconds(0.6f);

        treeHealth -= 1;
        SyncGlobalState();

        if (treeHealth <= 0)
        {
            TreeIssDead();
        }

        isBeingHit = false;
    }

    private void SyncGlobalState()
    {
        if (GlobalState.Instance == null) return;
        GlobalState.Instance.resourceHealth = treeHealth;
        GlobalState.Instance.resourceMaxHealth = treeMaxHealth;
    }

    void TreeIssDead()
    {
        if (isDead) return;
        isDead = true;

        Vector3 treePosition = transform.position;

        GameObject objectToDestroy;
        if (rootToDestroy != null)
        {
            objectToDestroy = rootToDestroy.gameObject;
        }
        else
        {
            Debug.LogWarning("[ChoppableTree]: 'Root To Destroy' chưa được gán, fallback transform.parent.parent.");
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