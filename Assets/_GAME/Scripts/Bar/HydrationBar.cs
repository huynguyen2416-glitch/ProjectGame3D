using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class HydrationBar : MonoBehaviour
{
    private Slider slider;
    public Text hydrationCounter;
    public GameObject playerState;
    public float currentHydration;
    public float maxHydration;
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        currentHydration = playerState.GetComponent<PlayerState>().currentHydrationPercent;
        maxHydration = playerState.GetComponent<PlayerState>().maxHydrationPercent;

        float fillValue = currentHydration / maxHydration;
        slider.value = fillValue;

        hydrationCounter.text = currentHydration + "%";
    }
}