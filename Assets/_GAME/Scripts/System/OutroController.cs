using System.Collections;
using UnityEngine;

public class OutroController : MonoBehaviour
{
    [Header("Nhân vật")]
    public Animator playerAnimator;
    public Transform playerTransform;
    public string sitTrigger = "Sit";
    public string standTrigger = "Stand";
    public string runTrigger = "Run";
    public string waveTrigger = "Wave";
    public MonoBehaviour[] scriptsToDisableDuringOutro;

    [Header("Thuyền & điểm đến")]
    public GameObject boatObject;
    public Transform boatStartPoint;
    public Transform boatArrivalPoint;
    public float boatMoveSpeed = 1.5f;

    [Header("Nhân vật chạy ra gặp thuyền")]
    public Transform playerMeetPoint;
    public float runSpeed = 3.5f;
    public float arrivalThreshold = 0.3f;

    [Header("Ánh sáng hoàng hôn")]
    public Light directionalLight;
    public Color sunsetColor = new Color(1f, 0.55f, 0.35f);
    public Vector3 sunsetEulerRotation = new Vector3(10f, 170f, 0f);

    [Header("Thời lượng từng đoạn (giây)")]
    public float sitDuration = 4f;
    public float sunsetTransitionDuration = 3f;
    public float waveDuration = 2.5f;
    public float fadeDuration = 2f;
    public float delayBeforeReturnToMenu = 1f;

    [Header("Fade to black & Audio")]
    public CanvasGroup fadeCanvasGroup;
    public bool fadeAudioWithScreen = true;

    private void Start()
    {
        // 1. Giấu con trỏ chuột
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(PlayOutroSequence());
    }

    private IEnumerator PlayOutroSequence()
    {
        SetPlayerScriptsEnabled(false);

        // 2. Kích hoạt âm thanh từ SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSound(SoundManager.Instance.backgroundMusic);
            SoundManager.Instance.PlaySound(SoundManager.Instance.outroMusic);
            SoundManager.Instance.PlaySound(SoundManager.Instance.waveSound);
        }

        // BƯỚC 1: Ngồi suy tư
        if (playerAnimator != null && !string.IsNullOrEmpty(sitTrigger))
            playerAnimator.SetTrigger(sitTrigger);
        yield return new WaitForSeconds(sitDuration);

        // BƯỚC 2: Hoàng hôn + Thuyền xuất hiện
        if (boatObject != null)
        {
            if (boatStartPoint != null)
            {
                boatObject.transform.position = boatStartPoint.position;
                boatObject.transform.rotation = boatStartPoint.rotation;
            }
            boatObject.SetActive(true);
        }
        yield return StartCoroutine(TransitionToSunset());

        // BƯỚC 3: Đứng dậy + Thuyền vào bờ + Chạy ra đón
        if (playerAnimator != null && !string.IsNullOrEmpty(standTrigger))
            playerAnimator.SetTrigger(standTrigger);
        if (playerAnimator != null && !string.IsNullOrEmpty(runTrigger))
            playerAnimator.SetTrigger(runTrigger);

        Coroutine boatRoutine = StartCoroutine(MoveBoatToArrivalPoint());
        Coroutine playerRoutine = StartCoroutine(RunPlayerToMeetPoint());
        yield return boatRoutine;
        yield return playerRoutine;

        // BƯỚC 4: Vẫy tay
        if (playerAnimator != null && !string.IsNullOrEmpty(waveTrigger))
            playerAnimator.SetTrigger(waveTrigger);
        yield return new WaitForSeconds(waveDuration);

        // BƯỚC 5: Fade đen màn hình & nhỏ nhạc
        yield return StartCoroutine(FadeToBlackAndAudio());
        yield return new WaitForSeconds(delayBeforeReturnToMenu);

        // BƯỚC 6: Chuyển sang CreditScene
        if (GameController.Instance != null)
        {
            GameController.Instance.GoToCreditScene();
        }
    }

    private void SetPlayerScriptsEnabled(bool isEnabled)
    {
        if (scriptsToDisableDuringOutro == null) return;
        foreach (MonoBehaviour script in scriptsToDisableDuringOutro)
        {
            if (script != null) script.enabled = isEnabled;
        }
    }

    private IEnumerator TransitionToSunset()
    {
        if (directionalLight == null) yield break;
        Color startColor = directionalLight.color;
        Quaternion startRotation = directionalLight.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(sunsetEulerRotation);

        float elapsed = 0f;
        while (elapsed < sunsetTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / sunsetTransitionDuration);
            directionalLight.color = Color.Lerp(startColor, sunsetColor, t);
            directionalLight.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        directionalLight.color = sunsetColor;
        directionalLight.transform.rotation = targetRotation;
    }

    private IEnumerator RunPlayerToMeetPoint()
    {
        if (playerTransform == null || playerMeetPoint == null) yield break;
        while (Vector3.Distance(playerTransform.position, playerMeetPoint.position) > arrivalThreshold)
        {
            Vector3 direction = playerMeetPoint.position - playerTransform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
            }
            playerTransform.position = Vector3.MoveTowards(playerTransform.position, playerMeetPoint.position, runSpeed * Time.deltaTime);
            yield return null;
        }
        playerTransform.position = playerMeetPoint.position;
    }

    private IEnumerator MoveBoatToArrivalPoint()
    {
        if (boatObject == null || boatArrivalPoint == null) yield break;
        Transform boatTransform = boatObject.transform;
        while (Vector3.Distance(boatTransform.position, boatArrivalPoint.position) > arrivalThreshold)
        {
            Vector3 direction = boatArrivalPoint.position - boatTransform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                boatTransform.rotation = Quaternion.Slerp(boatTransform.rotation, Quaternion.LookRotation(direction), 3f * Time.deltaTime);
            }
            boatTransform.position = Vector3.MoveTowards(boatTransform.position, boatArrivalPoint.position, boatMoveSpeed * Time.deltaTime);
            yield return null;
        }
        boatTransform.position = boatArrivalPoint.position;
    }

    private IEnumerator FadeToBlackAndAudio()
    {
        float elapsed = 0f;
        float startAlpha = fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 0f;
        if (fadeCanvasGroup != null) fadeCanvasGroup.gameObject.SetActive(true);

        // Gọi Fade nhạc từ SoundManager
        if (SoundManager.Instance != null && fadeAudioWithScreen)
        {
            SoundManager.Instance.FadeOutSound(SoundManager.Instance.outroMusic, fadeDuration);
            SoundManager.Instance.FadeOutSound(SoundManager.Instance.waveSound, fadeDuration);
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
            yield return null;
        }
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;
    }
}