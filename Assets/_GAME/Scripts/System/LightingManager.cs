using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance { get; private set; }

    [Header("Scene References")]
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [Header("Time Settings")]
    [SerializeField, Range(0, 48)] public float TimeOfDay;
    [SerializeField] private float dayLength = 48f;

    [Header("Win Condition: Survival Mode")]
    public int daysSurvived = 0;
    public int daysToWin = 7;

    [Header("Boss cuối game")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public DayTransitionUI dayTransitionUI;

    private float previousTime = 0f;
    private bool wasNight = false;

    // Biến đánh dấu Boss đang thao túng thời gian
    public bool isTimeStopped = false;
    private bool bossSpawned = false; // Ngăn spawn Boss nhiều lần

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool IsNight()
    {
        if (dayLength <= 0) return false;
        float percent = TimeOfDay / dayLength;
        return percent < 0.25f || percent > 0.85f;
    }

    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            // THỜI GIAN BỊ ĐÓNG BĂNG KHI BOSS XUẤT HIỆN
            if (isTimeStopped) return;

            previousTime = TimeOfDay;
            TimeOfDay += Time.deltaTime;
            TimeOfDay %= dayLength;

            bool isNightNow = IsNight();

            // ĐÊM VỪA BẮT ĐẦU (trước đó vẫn là ngày) -> hiện UI "Đêm thứ X" ĐÚNG lúc trời vừa tối,
            // không phải lúc TimeOfDay reset về 0 (mốc đó nằm giữa đêm, không phải lúc đêm bắt đầu)
            if (isNightNow && !wasNight)
            {
                int upcomingNight = daysSurvived + 1;
                Debug.Log($"Trời đã tối! Đêm thứ {upcomingNight} bắt đầu.");

                if (dayTransitionUI != null)
                {
                    dayTransitionUI.ShowDay(upcomingNight);
                }
            }

            // ĐÊM VỪA KẾT THÚC (trước đó vẫn là đêm) -> tính là đã sống sót qua 1 đêm
            if (!isNightNow && wasNight)
            {
                daysSurvived++;
                Debug.Log($"Trời đã sáng! Bạn đã sống sót được: {daysSurvived}/{daysToWin} đêm.");

                if (PersonaManager.Instance != null)
                {
                    PersonaManager.Instance.AwardPoint(1, "Sống sót qua 1 đêm");
                }

                // KIỂM TRA: ĐẾN TRƯỚC NGÀY CỨU HỘ 1 NGÀY -> BOSS XUẤT HIỆN CHẶN ĐƯỜNG
                if (daysSurvived == daysToWin - 1 && !bossSpawned)
                {
                    SpawnBoss();
                }
                // KIỂM TRA: ĐẾN ĐÚNG NGÀY CỨU HỘ VÀ BOSS ĐÃ CHẾT -> HIỆN MÀN HÌNH THẮNG
                else if (daysSurvived >= daysToWin)
                {
                    Debug.Log("ĐÃ ĐẾN NGÀY CỨU HỘ! KẾT THÚC GAME!");
                    if (GameController.Instance != null)
                    {
                        GameController.Instance.TriggerWin();
                    }
                }
            }

            wasNight = isNightNow;

            UpdateLighting(TimeOfDay / dayLength);
        }
        else
        {
            UpdateLighting(TimeOfDay / dayLength);
        }
    }

    private void SpawnBoss()
    {
        isTimeStopped = true; // Khóa đồng hồ, ngăn ngày cứu hộ bắt đầu
        bossSpawned = true;
        Debug.Log("BOSS XUẤT HIỆN VÀO TRƯỚC NGÀY CUỐI CÙNG! NÓ ĐANG THAO TÚNG THỜI GIAN!");

        if (bossPrefab == null)
        {
            Debug.LogError("[LightingManager]: Chưa gán bossPrefab, không thể spawn Boss!");
            return;
        }

        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        Quaternion spawnRot = bossSpawnPoint != null ? bossSpawnPoint.rotation : Quaternion.identity;
        Instantiate(bossPrefab, spawnPos, spawnRot);
    }

    // HÀM NÀY GỌI KHI BOSS CHẾT, GIÚP THỜI GIAN TRÔI TIẾP ĐẾN NGÀY CỨU HỘ
    public void OnBossDefeated()
    {
        isTimeStopped = false;
        Debug.Log("BOSS ĐÃ CHẾT! LỜI NGUYỀN THỜI GIAN ĐƯỢC GIẢI TRỪ, BÌNH MINH CỦA NGÀY CỨU HỘ ĐANG ĐẾN!");
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
        if (DirectionalLight != null) return;
        if (RenderSettings.sun != null) DirectionalLight = RenderSettings.sun;
        else
        {
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
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