using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("--- Danh sách AudioSource ---")]
    public AudioSource dropItemSound;
    public AudioSource craftingSound;
    public AudioSource toolSwingSound;
    public AudioSource chopSound;
    public AudioSource pickupItemSound;
    public AudioSource grassWalkSound;

    public AudioSource backgroundMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
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
}