using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Campfire : MonoBehaviour
{
    [Header("--- Chỉ số Lửa ---")]
    public float fireDamagePerSecond = 15f; // Sát thương khi dẫm thẳng vào lửa
    public float burnRadius = 1.2f;         // Khoảng cách bị bỏng 

    private bool isPlayerNearby = false;

    private void Awake()
    {
        // Yêu cầu bắt buộc để nhận diện vùng an toàn
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            // Báo cho Player biết: "Đang ở cạnh lửa, đêm nay không mất máu!"
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.SetNearCampfire(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // Báo cho Player biết: "Đã rời xa đống lửa!"
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.SetNearCampfire(false);
            }
        }
    }

    private void Update()
    {
        // Kiểm tra xem người chơi có dẫm chân trực tiếp vào vùng cháy của lửa không
        if (isPlayerNearby && PlayerState.Instance != null && PlayerState.Instance.playerBody != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerState.Instance.playerBody.transform.position);

            if (distanceToPlayer <= burnRadius)
            {
                // Dẫm vào lửa -> Trừ máu ngay lập tức
                float currentHp = PlayerState.Instance.currentHealth;
                PlayerState.Instance.setHealth(currentHp - fireDamagePerSecond * Time.deltaTime);
            }
        }
    }
}