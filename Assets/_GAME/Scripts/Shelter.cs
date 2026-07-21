using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class Shelter : MonoBehaviour
{
    [Header("Persona ---")]
    public bool isPlayerBuilt = false;

    private bool hasAwardedPoint = false;
    private bool isPlayerNearby = false;

    private BoxCollider triggerCollider;
    private PlayerState cachedPlayerState;

    // Danh sách TẤT CẢ Shelter đang tồn tại trong Scene 
    public static readonly List<Shelter> ActiveShelters = new List<Shelter>();

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        if (isPlayerBuilt && !hasAwardedPoint && PersonaManager.Instance != null)
        {
            hasAwardedPoint = true;
            PersonaManager.Instance.AwardPoint(1, "Xây lều trú ẩn");
        }
    }

    private void OnEnable()
    {
        ActiveShelters.Add(this);
    }

    private void OnDisable()
    {
        ActiveShelters.Remove(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (PlayerState.Instance != null)
            {
                cachedPlayerState = PlayerState.Instance;
                // Dùng chung đúng cờ "gần lửa" mà Campfire đang dùng để miễn lạnh
                cachedPlayerState.SetNearCampfire(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (cachedPlayerState != null)
            {
                cachedPlayerState.SetNearCampfire(false);
            }
            cachedPlayerState = null;
        }
    }

    private void OnDestroy()
    {
        if (isPlayerNearby && cachedPlayerState != null)
        {
            cachedPlayerState.SetNearCampfire(false);
        }
    }

    // Kiểm tra 1 điểm có nằm trong vùng BoxCollider của Shelter này không
    public bool ContainsPoint(Vector3 point)
    {
        if (triggerCollider == null) return false;
        return triggerCollider.bounds.Contains(point);
    }

    // Kiểm tra 1 điểm bất kỳ có đang được BẤT KỲ Shelter nào bảo vệ không - EnemyAI gọi hàm này
    public static bool IsPointProtected(Vector3 point)
    {
        foreach (var shelter in ActiveShelters)
        {
            if (shelter != null && shelter.ContainsPoint(point)) return true;
        }
        return false;
    }
}