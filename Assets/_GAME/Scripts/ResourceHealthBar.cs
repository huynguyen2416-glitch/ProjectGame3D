using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ResourceHealthBar : MonoBehaviour
{

    private Slider slider;
    private float currentHealth, maxHealth;

    public GameObject globalState;

    // Cached source of the displayed health values.
    private GlobalState globalStateComponent;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        if (globalState != null) globalStateComponent = globalState.GetComponent<GlobalState>();
    }

    private void Update()
    {
        if (globalStateComponent == null || slider == null) return;

        currentHealth = globalStateComponent.resourceHealth;
        maxHealth = globalStateComponent.resourceMaxHealth;

        if (maxHealth <= 0f) return;

        float fillValue = currentHealth / maxHealth;
        slider.value = fillValue;
    }


}
