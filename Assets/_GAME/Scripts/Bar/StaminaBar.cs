using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    private Slider slider;
    public GameObject playerState;
    private float currentStamina, maxStamina;
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        currentStamina = playerState.GetComponent<PlayerState>().currentStamina;
        maxStamina = playerState.GetComponent<PlayerState>().maxStamina;

        float fillValue = currentStamina / maxStamina;
        slider.value = fillValue;

    }
}