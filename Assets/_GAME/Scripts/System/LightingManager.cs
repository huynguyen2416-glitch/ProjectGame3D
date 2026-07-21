using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance { get; private set; }


    public static event System.Action OnDawn;

    [Header("Scene References")]
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [Header("Time Settings")]
    [Tooltip("Thời gian hiện tại trong ngày. MẶC ĐỊNH = 0 (Nửa đêm) để game LUÔN bắt đầu vào ĐÊM THỨ " +
             "1 đúng như thiết kế sinh tồn (trước đây mặc định = 12, tức đúng lúc Bình minh, khiến " +
             "game bắt đầu giữa ban ngày thay vì ban đêm - đây chính là nguyên nhân gây lỗi).")]
    [SerializeField, Range(0, 48)] public float TimeOfDay = 0f;
    [Tooltip("Độ dài của 1 ngày (Mặc định: 48)")]
    [SerializeField] private float dayLength = 48f;

    [Header("Win Condition: Survival Mode")]
    public int daysSurvived = 0;
    public int daysToWin = 10;

    [Tooltip("Kéo Panel hiệu ứng 'Đêm thứ X' (gắn DayTransitionUI.cs) vào đây")]
    public DayTransitionUI dayTransitionUI;

    private bool hasWon = false;

    // Tracks transitions between day and night.
    private bool isNightState = false;
    public bool ShouldDrainHealth { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Khởi tạo trạng thái môi trường ngay khi vào game để tránh lỗi UI
        if (Application.isPlaying)
        {
            float percent = TimeOfDay / dayLength;
            isNightState = (percent >= 0.75f || percent < 0.25f);
            ShouldDrainHealth = isNightState;
            if (isNightState && dayTransitionUI != null)
            {
                dayTransitionUI.ShowDay(daysSurvived + 1);
            }
        }
    }

    // Hàm kiểm tra tổng quát (Dành cho PlayerState hoặc hệ thống khác gọi tới)
    public bool IsNight()
    {
        if (dayLength <= 0) return false;
        float percent = TimeOfDay / dayLength;
        // Ban đêm chính thức: Hoàng hôn (75%) vòng qua Nửa đêm (0%) đến Bình minh (25%)
        return (percent >= 0.75f || percent < 0.25f);
    }

    private void Update()
    {
        if (Preset == null) return;

        if (Application.isPlaying)
        {
            if (hasWon) return;

            TimeOfDay += Time.deltaTime;
            TimeOfDay %= dayLength;

            float percent = TimeOfDay / dayLength;

            // Kiểm tra theo quy ước chuẩn: 75% là tối, 25% là sáng
            bool currentlyNight = (percent >= 0.75f || percent < 0.25f);

            // 1. CHUYỂN GIAO: SẬP TỐI
            if (currentlyNight && !isNightState)
            {
                isNightState = true;
                ShouldDrainHealth = true; // Kích hoạt thời tiết lạnh

                if (dayTransitionUI != null)
                {
                    dayTransitionUI.ShowDay(daysSurvived + 1);
                }
                Debug.Log($"Trời đổ tối! Bắt đầu vào Đêm thứ {daysSurvived + 1}. Hệ thống kích hoạt hiệu ứng lạnh.");
            }
            // 2. CHUYỂN GIAO: BÌNH MINH (Sáng sớm)
            else if (!currentlyNight && isNightState)
            {
                isNightState = false;
                ShouldDrainHealth = false; // Ngừng đốt máu ban đêm

                daysSurvived++; // Sống sót an toàn đến bình minh mới được cộng ngày

                Debug.Log($"Trời đã sáng! Bạn đã sống sót an toàn qua: {daysSurvived}/{daysToWin} đêm.");

                // Thưởng điểm sinh tồn chính xác lúc mặt trời mọc
                if (PersonaManager.Instance != null)
                {
                    PersonaManager.Instance.AwardPoint(1, "Sống sót qua 1 đêm");
                }

                // Báo cho các hệ thống khác (VD WaterSource) biết trời vừa sáng để tự làm mới theo ngày
                OnDawn?.Invoke();

                if (daysSurvived >= daysToWin)
                {
                    TriggerWinCondition();
                }
            }

            UpdateLighting(percent);
        }
        else
        {
            UpdateLighting(TimeOfDay / dayLength);
        }
    }

    private void TriggerWinCondition()
    {
        hasWon = true;
        Debug.Log("CHÚC MỪNG! BẠN ĐÃ SỐNG SÓT ĐỦ SỐ ĐÊM QUY ĐỊNH!");
        if (GameController.Instance != null) GameController.Instance.TriggerWin();
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            // Xoay chuẩn: 0% = Nửa đêm, 25% = Bình minh, 50% = Trưa, 75% = Hoàng hôn
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }

    private void OnValidate()
    {
        if (DirectionalLight != null) return;
        if (RenderSettings.sun != null) DirectionalLight = RenderSettings.sun;
    }
}