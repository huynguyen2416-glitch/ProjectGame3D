using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [Header("Time Settings")]
    [Tooltip("Thời gian hiện tại trong ngày")]
    [SerializeField, Range(0, 48)] public float TimeOfDay;
    [Tooltip("Độ dài của 1 ngày (Mặc định: 48)")]
    [SerializeField] private float dayLength = 48f;

    [Header("Win Condition: Survival Mode")]
    public int daysSurvived = 0;
    public int daysToWin = 10;

    [Tooltip("Kéo Panel hiệu ứng 'Đêm thứ X' (gắn DayTransitionUI.cs) vào đây")]
    public DayTransitionUI dayTransitionUI;

    private float previousTime = 0f;
    private bool hasWon = false;

    // ================= SỬA TẠI ĐÂY: KHÔI PHỤC NGÀY & GIỜ KHI LOAD GAME ================= //
    private void Start()
    {
        if (GameController.PendingLoad != null)
        {
            daysSurvived = GameController.PendingLoad.daysSurvived;
            TimeOfDay = GameController.PendingLoad.timeOfDay;
            Debug.Log($"[LightingManager]: Đã khôi phục thành công ngày {daysSurvived}, lúc {TimeOfDay}h từ file save!");
        }
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

                if (GameController.Instance != null)
                {
                    GameController.Instance.PerformAutosave(daysSurvived, TimeOfDay);
                }
                else
                {
                    Debug.LogWarning("[LightingManager]: Không tìm thấy GameController.Instance, KHÔNG THỂ autosave ngày mới!");
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