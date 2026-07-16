using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Để xử lý việc tải lại màn chơi khi bấm nút quay lại

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
    public float staminaDrainPerSecond = 20f;  // Tốc độ giảm khi chạy nước rút (Sprint)
    public float staminaRegenPerSecond = 12f;  // Tốc độ hồi khi không chạy nước rút
    public KeyCode sprintKey = KeyCode.LeftShift;// nút shift chạy

    // Cho script di chuyển (PlayerMovement) kiểm tra trước khi cho phép tăng tốc chạy:
    public bool CanSprint => currentStamina > 0f;

    //CÁC BIẾN ĐƯỢC THÊM MỚI ĐỂ PHỤC VỤ HỆ THỐNG CHẾT & HỒI SINH
    public GameObject deathPanel; // kéo Panel "Bạn đã chết" vào ngoài Unity
    public float starvationDamageRate = 1f; // Lượng máu bị trừ mỗi giây nếu hết sạch Calo hoặc Nước
    private bool isDead = false; // Cờ kiểm tra xem người chơi đã chết chưa


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
        // Nếu đang Continue/Restart-from-death, nạp lại toàn bộ chỉ số + vị trí từ save.
        if (GameController.PendingLoad != null)
        {
            ApplySaveData(GameController.PendingLoad);
        }
        else
        {
            currentHealth = maxHealth;
            currentCalories = maxCalories;
            currentHydrationPercent = maxHydrationPercent;
            currentStamina = maxStamina;
        }

        // Đảm bảo ẩn bảng chết lúc mới vào game
        if (deathPanel != null) deathPanel.SetActive(false);

        // Khởi tạo lastPosition ngay từ đầu, tránh frame đầu tiên tính nhầm 1 quãng đường
        // "ảo" bằng khoảng cách từ gốc toạ độ (0,0,0) tới vị trí spawn thật của người chơi.
        if (playerBody != null) lastPosition = playerBody.transform.position;

        StartCoroutine(decreaseHydration()); // Khởi chạy Coroutine giảm nước
    }

    // ================= NẠP LẠI TOÀN BỘ CHỈ SỐ + VỊ TRÍ TỪ SAVE ================= //
    public void ApplySaveData(SaveData data)
    {
        if (playerBody != null)
        {
            // 1. Tắt tạm CharacterController (tránh lỗi xung đột tọa độ của Unity)
            CharacterController cc = playerBody.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 2. Kiểm tra xem có dùng NavMeshAgent không (phòng trường hợp bác thêm tính năng auto-move)
            UnityEngine.AI.NavMeshAgent agent = playerBody.GetComponent<UnityEngine.AI.NavMeshAgent>();
            Vector3 targetPos = data.GetPosition();

            if (agent != null && agent.enabled)
            {
                agent.Warp(targetPos);
            }
            else
            {
                playerBody.transform.position = targetPos;
            }

            // Khôi phục hướng xoay lúc save - nếu không có dòng này, mỗi lần Continue nhân vật
            // sẽ luôn quay về hướng mặc định của điểm spawn, không đúng hướng lúc save.
            playerBody.transform.rotation = data.playerRotation;

            // 3. KHẮC PHỤC LỖI RƠI XUYÊN ĐẤT: Ép Unity đồng bộ vật lý ngay lập tức
            Physics.SyncTransforms();

            // 4. Bật lại CharacterController
            if (cc != null) cc.enabled = true;
        }

        // 5. Khôi phục các chỉ số sinh tồn
        currentHealth = data.currentHealth;
        maxHealth = data.maxHealth;
        currentCalories = data.currentCalories;
        maxCalories = data.maxCalories;
        currentHydrationPercent = data.currentHydrationPercent;
        maxHydrationPercent = data.maxHydrationPercent;
        currentStamina = data.currentStamina;
        maxStamina = data.maxStamina;
        staminaDrainPerSecond = data.staminaDrainPerSecond;
        staminaRegenPerSecond = data.staminaRegenPerSecond;

        isDead = false; // Đảm bảo gỡ trạng thái chết
        Debug.Log("[PlayerState]: Đã spawn an toàn tại vị trí đã lưu và khôi phục chỉ số.");
    }

    // ================= ĐIỀN CHỈ SỐ + VỊ TRÍ HIỆN TẠI VÀO 1 SaveData ĐỂ GHI FILE ================= //
    // Được GameController.PerformAutosave() gọi mỗi khi sang ngày mới.
    public void FillSaveData(SaveData data)
    {
        data.currentHealth = currentHealth;
        data.maxHealth = maxHealth;
        data.currentCalories = currentCalories;
        data.maxCalories = maxCalories;
        data.currentHydrationPercent = currentHydrationPercent;
        data.maxHydrationPercent = maxHydrationPercent;
        data.currentStamina = currentStamina;
        data.maxStamina = maxStamina;
        data.staminaDrainPerSecond = staminaDrainPerSecond;
        data.staminaRegenPerSecond = staminaRegenPerSecond;

        if (playerBody != null)
        {
            data.SetPosition(playerBody.transform.position);
            data.playerRotation = playerBody.transform.rotation;
        }
    }

    // Coroutine đếm thời gian trừ nước mỗi 2 giây
    IEnumerator decreaseHydration()
    {
        while (true)
        {
            if (!isDead) // Thêm điều kiện: Chỉ trừ nước khi còn sống
            {
                currentHydrationPercent -= 1;
                if (currentHydrationPercent < 0) currentHydrationPercent = 0;
            }
            yield return new WaitForSeconds(2);
        }
    }

    void Update()
    {
        if (isDead) return; // Nếu đã chết thì dừng mọi xử lý di chuyển hay tính toán chỉ số bên dưới

        // Tính khoảng cách di chuyển TRONG FRAME NÀY (dùng để nhận biết đang di chuyển hay đứng yên,
        // phục vụ tính Stamina), rồi mới cộng dồn vào distanceTravelled như logic Calo cũ.
        float frameDistance = Vector3.Distance(playerBody.transform.position, lastPosition);
        bool isMovingThisFrame = frameDistance > 0.001f;

        distanceTravelled += frameDistance; // khoảng cách di chuyển
        lastPosition = playerBody.transform.position; //vị trí đứng kết thúc

        // Nhánh Persona (Sinh tồn) có thể làm chậm tốc độ đốt calo qua calorieBurnRateReduction (vd 0.2 = -20%)
        float calorieBurnReduction = PersonaManager.Instance != null ? PersonaManager.Instance.calorieBurnRateReduction : 0f;
        float calorieDistanceThreshold = 5f / Mathf.Max(0.1f, 1f - calorieBurnReduction);
        float healthBurnReduction = PersonaManager.Instance != null ? PersonaManager.Instance.healthBurnRateReduction : 0f;
        if (distanceTravelled >= calorieDistanceThreshold)
        {
            distanceTravelled = 0;
            currentCalories -= 1;
            if (currentCalories < 0) currentCalories = 0;
        }

        // ĐÓI KHÁT QUÁ SẼ BỊ TRỪ MÁU
        // Nếu Calo chạm đáy HOẶC Nước chạm đáy (bằng 0) thì người chơi mất máu dần dần theo thời gian
        if (currentCalories <= 0 || currentHydrationPercent <= 0)
        {
            // Trước đây healthBurnReduction được tính ra nhưng KHÔNG dùng ở đây - persona
            // "giảm đốt máu" hoàn toàn không có tác dụng gì. Giờ nhân vào công thức thật.
            float damage = starvationDamageRate * Time.deltaTime * 2 * (1f - healthBurnReduction);
            setHealth(currentHealth - damage);
        }

        //LOGIC STAMINA: chạy nước rút (giữ sprintKey + đang di chuyển) thì tốn thể lực,
        // còn lại (đứng yên hoặc đi bộ thường) thì hồi thể lực dần ---
        bool isSprinting = Input.GetKey(sprintKey) && isMovingThisFrame && currentStamina > 0f;
        if (isSprinting)
        {
            setStamina(currentStamina - staminaDrainPerSecond * Time.deltaTime);
        }
        else
        {
            setStamina(currentStamina + staminaRegenPerSecond * Time.deltaTime);
        }

        // Nút N thần thánh để test tụt máu
        if (Input.GetKeyDown(KeyCode.N)) //
        {
            setHealth(currentHealth - 10); //
        }
    }
    // NÂNG CẤP HÀM SET HEALTH: Tự động kiểm tra chết và chặn vượt giới hạn
    public void setHealth(float amount)
    {
        if (isDead) return;

        currentHealth = amount;

        // Giới hạn dưới: Không cho máu bị âm
        if (currentHealth < 0) currentHealth = 0;

        // Giới hạn trên: Không cho máu vượt qua giới hạn maxHealth
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // Nếu máu thực sự bằng 0 thì Kích hoạt chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Tối ưu hàm set Calo & Nước để tránh lỗi chỉ số hiển thị bị âm
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

    // Set Stamina, giới hạn trong khoảng [0, maxStamina]
    public void setStamina(float amount)
    {
        currentStamina = amount;
        if (currentStamina < 0) currentStamina = 0;
        if (currentStamina > maxStamina) currentStamina = maxStamina;
    }







    // ================= HÀM XỬ LÝ KHI NGƯỜI CHƠI CHẾT ================= //
    void Die()
    {
        isDead = true;
        Debug.Log("Người chơi đã cạn kiệt sinh lực và chết!");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ưu tiên: nếu đã setup GameController + DeathScene riêng, chụp lại khung hình lúc chết,
        // làm mờ, rồi mới chuyển scene (cần chờ hết frame nên phải chạy qua Coroutine).
        if (GameController.Instance != null)
        {
            StartCoroutine(CaptureDeathScreenshotThenTransition());
            return;
        }

        // Fallback: chưa gắn GameController -> giữ hành vi cũ (hiện Panel ngay trong scene hiện tại).
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
    }

    [Header("Chụp màn hình lúc chết (dùng cho nền mờ của DeathScene)")]
    [Tooltip("Số lần downsample khi làm mờ - càng nhiều càng mờ nhiều, nhưng cũng tốn thời gian xử lý hơn 1 chút. 4-5 là hợp lý.")]
    public int deathScreenshotBlurPasses = 4;

    private IEnumerator CaptureDeathScreenshotThenTransition()
    {
        // Chờ hết frame hiện tại để đảm bảo chụp đúng khung hình đang hiển thị trên màn hình
        yield return new WaitForEndOfFrame();

        Texture2D rawScreenshot = ScreenCapture.CaptureScreenshotAsTexture();
        Texture2D blurred = BlurTexture(rawScreenshot, deathScreenshotBlurPasses);

        // Không cần bản gốc (chưa mờ, full-res) nữa, giải phóng ngay để đỡ tốn bộ nhớ.
        Destroy(rawScreenshot);

        GameController.SetLastDeathScreenshot(blurred);
        GameController.Instance.TriggerDeath();
    }

    // Làm mờ bằng cách downsample rồi upsample nhiều lần qua RenderTexture (lọc bilinear).
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

    // ================= HÀM XỬ LÝ KHI BẤM NÚT "QUAY LẠI / HỒI SINH" ================= //
    public void OnRespawnButtonClick()
    {
        // Ưu tiên: nếu có GameController, load lại save gần nhất (đầu ngày hôm trước lúc chết).
        if (GameController.Instance != null)
        {
            GameController.Instance.RestartFromLastSave();
            return;
        }

        // Fallback: chưa gắn GameController -> tải lại chính Scene hiện tại (reset sạch từ đầu).
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}