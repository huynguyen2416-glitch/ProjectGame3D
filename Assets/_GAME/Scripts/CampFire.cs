using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Campfire : MonoBehaviour
{
    [Header("--- Chỉ số Lửa ---")]
    public float fireDamagePerSecond = 15f; // Sát thương khi dẫm thẳng vào lửa
    public float burnRadius = 1.2f;         // Khoảng cách bị bỏng 

    [Header("--- Persona ---")]
    public bool isPlayerBuilt = false;

    private bool hasAwardedPoint = false;
    private bool isPlayerNearby = false;

    // khoảng cách khi bị đốt
    private float burnRadiusSqr;
    private Transform cachedPlayerTransform;
    private PlayerState cachedPlayerState;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        burnRadiusSqr = burnRadius * burnRadius;
        if (isPlayerBuilt && !hasAwardedPoint && PersonaManager.Instance != null)
        {
            hasAwardedPoint = true;
            PersonaManager.Instance.AwardPoint(1, "Xây lửa trại");// kích hoạt nhận điểm
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (PlayerState.Instance != null)
            {
                cachedPlayerState = PlayerState.Instance;
                if (cachedPlayerState.playerBody != null)
                {
                    cachedPlayerTransform = cachedPlayerState.playerBody.transform;
                }

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

            cachedPlayerTransform = null;
            cachedPlayerState = null;
        }
    }

    private void Update()
    {
        if (isPlayerNearby && cachedPlayerState != null && cachedPlayerTransform != null)
        {
            Vector3 offset = transform.position - cachedPlayerTransform.position;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance <= burnRadiusSqr)
            {
                float currentHp = cachedPlayerState.currentHealth;
                cachedPlayerState.setHealth(currentHp - fireDamagePerSecond * Time.deltaTime);
            }
        }
    }
}