using UnityEngine;
using UnityEngine.SceneManagement;
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
    public AudioSource backgroundMusic; //Canh1
    public AudioSource outroMusic;  //Outro
    public AudioSource waveSound;  //Outro
    public AudioSource introSound; //Intro
    public AudioSource creditsSound; //Credit
    public AudioSource personaSound;

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

        if (gameObject.scene.name != "StartScene")
        {
            Debug.LogWarning($"[SoundManager]: SoundManager đang được tạo LẦN ĐẦU ở scene '{gameObject.scene.name}', không phải 'StartScene'. " +
                              "Theo đúng kiến trúc (giống GameController), object này nên đặt DUY NHẤT trong StartScene - nếu để ở scene khác " +
                              "(VD: Canh1), mọi AudioSource không phải con trực tiếp của nó sẽ bị Destroy mỗi khi scene đó unload, " +
                              "gây mất âm thanh từ lần chơi thứ 2 trở đi.");
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopSound(backgroundMusic);
        StopSound(outroMusic);
        StopSound(waveSound);
        StopSound(introSound);
        StopSound(creditsSound);

        switch (scene.name)
        {
            case "StartScene":
                PlaySound(introSound);
                break;

            case "Canh1":
                // Vào chính thức gameplay: phát nhạc nền Canh1 từ đầu.
                if (backgroundMusic != null)
                {
                    backgroundMusic.time = 0f;
                    backgroundMusic.Play();
                }
                break;

            case "CreditsScene":
                PlaySound(creditsSound);
                break;

            case "OutroScene":
                PlaySound(waveSound);
                PlaySound(outroMusic);
                break;
            case "DeathScene":

                break;

            default:
                break;
        }
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