using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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

    // ---- Player Stamina ---- //
    public float currentStamina;
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 20f;
    public float staminaRegenPerSecond = 12f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    public bool CanSprint => currentStamina > 0f;

    // ---- LƯU CHỈ SỐ GỐC (trước khi Persona tác động) - phục vụ nút Reset Persona ---- //
    private float baseMaxHealth, baseMaxStamina, baseMaxCalories, baseMaxHydrationPercent;
    private float baseStaminaDrainPerSecond, baseStaminaRegenPerSecond;
    private bool baseStatsCaptured = false;

    private void CaptureBaseStatsIfNeeded()
    {
        if (baseStatsCaptured) return;
        baseMaxHealth = maxHealth;
        baseMaxStamina = maxStamina;
        baseMaxCalories = maxCalories;
        baseMaxHydrationPercent = maxHydrationPercent;
        baseStaminaDrainPerSecond = staminaDrainPerSecond;
        baseStaminaRegenPerSecond = staminaRegenPerSecond;
        baseStatsCaptured = true;
    }

    // Gọi từ PersonaManager.ResetAllPersona() - đưa toàn bộ chỉ số đã bị Persona cộng dồn
    // (maxHealth, maxStamina, staminaDrainPerSecond,...) về đúng giá trị GỐC lúc chưa mở khoá gì cả.
    public void ResetToBaseStats()
    {
        CaptureBaseStatsIfNeeded();

        maxHealth = baseMaxHealth;
        maxStamina = baseMaxStamina;
        maxCalories = baseMaxCalories;
        maxHydrationPercent = baseMaxHydrationPercent;
        staminaDrainPerSecond = baseStaminaDrainPerSecond;
        staminaRegenPerSecond = baseStaminaRegenPerSecond;

        // Kẹp lại các chỉ số hiện tại để không vượt quá max mới (thấp hơn sau khi reset)
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentStamina > maxStamina) currentStamina = maxStamina;
        if (currentCalories > maxCalories) currentCalories = maxCalories;
        if (currentHydrationPercent > maxHydrationPercent) currentHydrationPercent = maxHydrationPercent;

        Debug.Log("[PlayerState]: Đã reset toàn bộ chỉ số về gốc (trước khi có Persona).");
    }

    // ---- HỆ THỐNG CHẾT & HỒI SINH ---- //
    public GameObject deathPanel;
    public float starvationDamageRate = 1f;
    private bool isDead = false;

    // ---- HỆ THỐNG SINH TỒN BAN ĐÊM ---- //
    [Header("---- Night Survival ----")]
    public float nightColdDamageRate = 2f;
    private bool isNearCampfire = false;

    // ==========================================
    //  HỆ THỐNG BUFF (HIỆU ỨNG THUỐC)
    // ==========================================
    [Header("---- Active Buffs ----")]
    public bool isColdImmune = false;
    public bool isStaminaInfinite = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        CaptureBaseStatsIfNeeded();

        currentHealth = maxHealth;
        currentCalories = maxCalories;
        currentHydrationPercent = maxHydrationPercent;
        currentStamina = maxStamina;

        if (deathPanel != null) deathPanel.SetActive(false);

        if (playerBody != null) lastPosition = playerBody.transform.position;
        StartCoroutine(decreaseHydration());
    }

    IEnumerator decreaseHydration()
    {
        while (true)
        {
            if (!isDead)
            {
                currentHydrationPercent -= 3;
                if (currentHydrationPercent < 0) currentHydrationPercent = 0;
            }
            yield return new WaitForSeconds(3);
        }
    }

    public void SetNearCampfire(bool isNear)
    {
        isNearCampfire = isNear;
    }

    void Update()
    {
        if (isDead) return;

        float frameDistance = Vector3.Distance(playerBody.transform.position, lastPosition);
        bool isMovingThisFrame = frameDistance > 0.001f;

        distanceTravelled += frameDistance;
        lastPosition = playerBody.transform.position;

        float calorieBurnReduction = PersonaManager.Instance != null ? PersonaManager.Instance.calorieBurnRateReduction : 0f;
        float calorieDistanceThreshold = 5f / Mathf.Max(0.1f, 1f - calorieBurnReduction);
        float healthBurnReduction = PersonaManager.Instance != null ? PersonaManager.Instance.healthBurnRateReduction : 0f;

        if (distanceTravelled >= calorieDistanceThreshold)
        {
            distanceTravelled = 0;
            currentCalories -= 1;
            if (currentCalories < 0) currentCalories = 0;
        }

        if (currentCalories <= 0 || currentHydrationPercent <= 0)
        {
            float damage = starvationDamageRate * Time.deltaTime * 2 * (1f - healthBurnReduction);
            setHealth(currentHealth - damage);
        }

        // thể lực vô hạn khi sprint
        bool isSprinting = Input.GetKey(sprintKey) && isMovingThisFrame && currentStamina > 0f;
        if (isSprinting && !isStaminaInfinite)
        {
            setStamina(currentStamina - staminaDrainPerSecond * Time.deltaTime);
        }
        else
        {
            setStamina(currentStamina + staminaRegenPerSecond * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            setHealth(currentHealth - 10);
        }

        //  Đọc biến ShouldDrainHealth từ LightingManager mới

        bool isDrainTime = LightingManager.Instance != null && LightingManager.Instance.ShouldDrainHealth;

        if (isDrainTime)
        {
            // Chỉ mất máu nếu KHÔNG đứng gần lửa trại VÀ KHÔNG có buff miễn nhiễm lạnh
            if (!isNearCampfire && !isColdImmune)
            {
                float coldDamage = nightColdDamageRate * Time.deltaTime;
                setHealth(currentHealth - coldDamage);
            }
        }
    }

    // ==========================================
    // CÁC HÀM XỬ LÝ BUFF TỪ THUỐC
    // ==========================================
    public void ApplyBuff(string buffType, float duration, float value)
    {
        switch (buffType)
        {
            case "ColdImmunity":
                StartCoroutine(ColdImmunityRoutine(duration));
                break;
            case "InfiniteStamina":
                StartCoroutine(InfiniteStaminaRoutine(duration));
                break;
            case "HealOverTime":
                StartCoroutine(HealOverTimeRoutine(value, duration));
                break;
        }
    }

    private IEnumerator ColdImmunityRoutine(float duration)
    {
        isColdImmune = true;
        Debug.Log("Đã kích hoạt: Miễn nhiễm lạnh!");
        yield return new WaitForSeconds(duration);
        isColdImmune = false;
        Debug.Log("Hết hiệu lực: Miễn nhiễm lạnh!");
    }

    private IEnumerator InfiniteStaminaRoutine(float duration)
    {
        isStaminaInfinite = true;
        Debug.Log("Đã kích hoạt: Thể lực vô hạn!");
        yield return new WaitForSeconds(duration);
        isStaminaInfinite = false;
        Debug.Log("Hết hiệu lực: Thể lực vô hạn!");
    }

    private IEnumerator HealOverTimeRoutine(float healPerSecond, float duration)
    {
        Debug.Log("Đã kích hoạt: Hồi máu theo thời gian!");
        float timePassed = 0f;
        while (timePassed < duration)
        {
            if (isDead) yield break;
            setHealth(currentHealth + healPerSecond);
            timePassed += 1f;
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("Hết hiệu lực: Hồi máu theo thời gian!");
    }

    // ==========================================

    public void setHealth(float amount)
    {
        if (isDead) return;
        currentHealth = amount;
        if (currentHealth < 0) currentHealth = 0;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentHealth <= 0) Die();
    }

    public void setCalories(float amount)
    {
        currentCalories = amount;
        if (currentCalories < 0) currentCalories = 0;
    }

    public void setHydration(float amount)
    {
        currentHydrationPercent = amount;
        if (currentHydrationPercent < 0) currentHydrationPercent = 0;
    }

    public void setStamina(float amount)
    {
        currentStamina = amount;
        if (currentStamina < 0) currentStamina = 0;
        if (currentStamina > maxStamina) currentStamina = maxStamina;
    }

    public void Die()
    {
        isDead = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameController.Instance != null)
        {
            StartCoroutine(CaptureDeathScreenshotThenTransition());
            return;
        }
        if (deathPanel != null) deathPanel.SetActive(true);
    }

    [Header("Chụp màn hình lúc chết")]
    public int deathScreenshotBlurPasses = 4;

    private IEnumerator CaptureDeathScreenshotThenTransition()
    {
        yield return new WaitForEndOfFrame();
        Texture2D rawScreenshot = ScreenCapture.CaptureScreenshotAsTexture();
        Texture2D blurred = BlurTexture(rawScreenshot, deathScreenshotBlurPasses);
        Destroy(rawScreenshot);

        GameController.SetLastDeathScreenshot(blurred);
        GameController.Instance.TriggerDeath();
    }

    private Texture2D BlurTexture(Texture2D source, int downsamplePasses)
    {
        int width = source.width;
        int height = source.height;
        RenderTexture current = RenderTexture.GetTemporary(width, height, 0);
        current.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, current);

        for (int i = 0; i < downsamplePasses; i++)
        {
            width = Mathf.Max(2, width / 2);
            height = Mathf.Max(2, height / 2);
            RenderTexture next = RenderTexture.GetTemporary(width, height, 0);
            next.filterMode = FilterMode.Bilinear;
            Graphics.Blit(current, next);
            RenderTexture.ReleaseTemporary(current);
            current = next;
        }

        RenderTexture upsampled = RenderTexture.GetTemporary(source.width, source.height, 0);
        upsampled.filterMode = FilterMode.Bilinear;
        Graphics.Blit(current, upsampled);
        RenderTexture.ReleaseTemporary(current);

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = upsampled;
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        result.Apply();
        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(upsampled);

        return result;
    }

    public void OnRespawnButtonClick()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.RestartGame();
            return;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}