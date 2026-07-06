using UnityEngine;
using System.Collections;
public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; set; }

    // ---- Player Health ---- //
    public float currentHealth;
    public float maxHealth;

    // ---- Player Calories ---- //
    public float currentCalories;
    public float maxCalories;
    float distanceTravelled = 0;
    Vector3 lastPosition;
    public GameObject playerBody;

    // ---- Player Hydration ---- //
    public float currentHydrationPercent;
    public float maxHydrationPercent;
    public bool isHydrationActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
            Instance = this;
     }
    private void Start()
    {
        currentHealth = maxHealth;
        currentCalories = maxCalories;
        currentHydrationPercent = maxHydrationPercent;
        StartCoroutine(decreaseHydration());
    }
    IEnumerator decreaseHydration()
    {
        while (true)
        {
            currentHydrationPercent -= 1;
            yield return new WaitForSeconds(2);
        }
    }
    void Update()
    {
        distanceTravelled += Vector3.Distance(playerBody.transform.position, lastPosition);
        lastPosition = playerBody.transform.position;

        if (distanceTravelled >= 5)
        {
            distanceTravelled = 0;
            currentCalories -= 1;
        }

        // Testing the health bar
        if (Input.GetKeyDown(KeyCode.N))
        {
            currentHealth -= 10;
        }
    }
    public void setHealth(float amount)
    {
        currentHealth = amount;
    }

    public void setCalories(float amount)
    {
        currentCalories = amount;
    }

    public void setHydration(float amount)
    {
        currentHydrationPercent = amount;
    }
}

