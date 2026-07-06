using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;
    public Text healthCounter;
    public GameObject playerState;
    private float currentHealth, maxHealth;
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }
    
    void Update()
    {
        currentHealth = playerState.GetComponent<PlayerState>().currentHealth;
        maxHealth = playerState.GetComponent<PlayerState>().maxHealth;

        float fillValue = currentHealth / maxHealth;
        slider.value = fillValue;

        healthCounter.text = currentHealth + "/" + maxHealth;
    }
}