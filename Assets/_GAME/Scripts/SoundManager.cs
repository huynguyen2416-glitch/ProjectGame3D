using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("--- Danh sách AudioSource Gameplay ---")]
    public AudioSource dropItemSound;
    public AudioSource craftingSound;
    public AudioSource toolSwingSound;
    public AudioSource chopSound;
    public AudioSource pickupItemSound;
    public AudioSource grassWalkSound;
    public AudioSource grassSprintSound;

    [Header("--- Nhạc và Âm thanh Môi trường ---")]
    public AudioSource backgroundMusic;

    [Tooltip("Nhạc nền lúc chạy Cutscene kết game")]
    public AudioSource outroMusic;
    [Tooltip("Tiếng sóng biển rì rào vỗ bờ")]
    public AudioSource waveSound;

    private void Awake()
    {
        // Đảm bảo chỉ có 1 SoundManager tồn tại
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(AudioSource soundToPlay)
    {
        if (soundToPlay != null && !soundToPlay.isPlaying)
        {
            soundToPlay.Play();
        }
    }

    public void StopSound(AudioSource soundToStop)
    {
        if (soundToStop != null && soundToStop.isPlaying)
        {
            soundToStop.Stop();
        }
    }

    // ================= CÁC HÀM DÙNG CHO CUTSCENE ================= //

    // Hàm gọi để làm nhỏ dần (fade out) một âm thanh bất kỳ
    public void FadeOutSound(AudioSource audioSource, float fadeDuration)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutCoroutine(audioSource, fadeDuration));
        }
    }

    private IEnumerator FadeOutCoroutine(AudioSource audioSource, float fadeDuration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            // Ép âm lượng giảm dần từ mức ban đầu về 0
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        // Trả lại âm lượng gốc để lần sau (nếu có chơi lại game) phát bình thường
        audioSource.volume = startVolume;
    }
}