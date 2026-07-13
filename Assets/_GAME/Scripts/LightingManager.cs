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
    [Tooltip("Kéo Panel màn hình Chiến Thắng vào đây")]
    public GameObject winScreenUI;

    // Biến lưu thời gian frame trước để tính lúc qua ngày mới
    private float previousTime = 0f;
    private bool hasWon = false;

    private void Start()
    {
        if (winScreenUI != null) winScreenUI.SetActive(false);
    }

    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            if (hasWon) return; // Nếu đã thắng thì dừng đếm thời gian

            previousTime = TimeOfDay;
            TimeOfDay += Time.deltaTime;

            // Ép thời gian quay vòng theo 1 ngày (VD: hết 48 thì reset về 0)
            TimeOfDay %= dayLength;

            // LOGIC ĐẾM NGÀY: Nếu thời gian hiện tại nhỏ hơn thời gian frame trước => Đã nhảy sang ngày mới
            if (TimeOfDay < previousTime)
            {
                daysSurvived++;
                Debug.Log($"Trời đã sáng! Bạn đã sống sót được: {daysSurvived}/{daysToWin} đêm.");

                if (daysSurvived >= daysToWin)
                {
                    TriggerWinCondition();
                }
            }

            UpdateLighting(TimeOfDay / dayLength);
        }
        else
        {
            // Cho phép xem trước ánh sáng trong Editor
            UpdateLighting(TimeOfDay / dayLength);
        }
    }

    // KÍCH HOẠT CHIẾN THẮNG
    private void TriggerWinCondition()
    {
        hasWon = true;
        Debug.Log("CHÚC MỪNG! BẠN ĐÃ SỐNG SÓT ĐỦ 10 ĐÊM!");

        if (winScreenUI != null)
        {
            winScreenUI.SetActive(true);

            // Giải phóng chuột để người chơi bấm nút "Chơi lại" hoặc "Thoát"
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Dừng hẳn game lại
            Time.timeScale = 0f;
        }
    }

    private void UpdateLighting(float timePercent)
    {
        // Set ambient and fog
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if (DirectionalLight != null)
        {
            // Set color based on gradient
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            // Quay góc mặt trời
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }

    // Tự động tìm Directional Light nếu chưa gán
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
            Light[] lights = GameObject.FindObjectsOfType<Light>();
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