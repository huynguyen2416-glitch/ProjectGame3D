using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	public Slider slider;

	public void UpdateHealth(float current, float max)
	{
		slider.maxValue = max;
		slider.value = current;
	}
}