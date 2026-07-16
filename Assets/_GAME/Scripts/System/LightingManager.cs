using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance { get; private set; } // THÊM MỚI: Để các script khác gọi đến

    [Header("Scene References")]
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [Header("Time Settings")]
    [Tooltip("Thời gian hiện tại trong ngày")]
    [SerializeField, Range(0, 96)] public float TimeOfDay;
    [Tooltip("Độ dài của 1 ngày (Mặc định: 96)")]
    [SerializeField] private float dayLength = 96f;

    [Header("Win Condition: Survival Mode")]
    public int daysSurvived = 0;
    public int daysToWin = 10;

    [Tooltip("Kéo Panel hiệu ứng 'Đêm thứ X' (gắn DayTransitionUI.cs) vào đây")]
    public DayTransitionUI dayTransitionUI;

    private float previousTime = 0f;
    private bool hasWon = false;

    private void Awake()
    {
        // THÊM MỚI: Khởi tạo Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // THÊM MỚI: Hàm kiểm tra xem trời có đang là ban đêm không
    public bool IsNight()
    {
        if (dayLength <= 0) return false;
        float percent = TimeOfDay / dayLength;
        // Mặc định: Trước 25% (Sáng sớm) và Sau 70% (Chiều tối) được tính là Đêm lạnh
        return percent < 0.25f || percent > 0.70f;
    }

    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            if (hasWon) return;

            previousTime = TimeOfDay;
            TimeOfDay += Time.deltaTime;
            TimeOfDay %= dayLength;

            if (TimeOfDay < previousTime)
            {
                daysSurvived++;
                Debug.Log($"Trời đã sáng! Bạn đã sống sót được: {daysSurvived}/{daysToWin} đêm.");

                if (dayTransitionUI != null)
                {
                    dayTransitionUI.ShowDay(daysSurvived);
                }

                if (daysSurvived >= daysToWin)
                {
                    TriggerWinCondition();
                }
            }

            UpdateLighting(TimeOfDay / dayLength);
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

        if (GameController.Instance != null)
        {
            GameController.Instance.TriggerWin();
        }
        else
        {
            Debug.LogError("[LightingManager]: Không tìm thấy GameController.Instance! Bạn phải chạy game từ StartScene.");
        }
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }

    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>();

            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}