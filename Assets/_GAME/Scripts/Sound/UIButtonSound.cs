using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    private static AudioSource uiAudioSource;

    void Start()
    {
        // Tự động lắng nghe sự kiện khi nút bị bấm
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        if (clickSound == null) return;

        // Nếu chưa có nguồn phát âm thanh UI, hệ thống sẽ tự sinh ra 1 cái tàng hình
        if (uiAudioSource == null)
        {
            GameObject soundObj = new GameObject("UI_Sound_Player");
            DontDestroyOnLoad(soundObj);//Không bị tắt tiếng khi chuyển Scene

            uiAudioSource = soundObj.AddComponent<AudioSource>();
            uiAudioSource.spatialBlend = 0f; // Chuyển thành âm thanh 2D UI sẽ nghe dc ở bất kỳ đâu
            uiAudioSource.ignoreListenerPause = true; // Phát bình thường kể cả khi Game đang Pause 
        }

        // Phát tiếng Click
        uiAudioSource.PlayOneShot(clickSound);
    }
}