using UnityEngine;


public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Gọi thẳng hàm Die() thông qua Singleton của PlayerState
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.Die();
            }
        }
    }
}