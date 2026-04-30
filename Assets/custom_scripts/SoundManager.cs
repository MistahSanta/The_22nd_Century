using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sounds")]
    public AudioClip trashPickupSound;
    public AudioClip treePlantSound;
    public AudioClip timeMachineSound;
    public AudioClip garbagePickerPickupSound;
    public AudioClip shovelPickupSound;
    public AudioClip toolSwitchSound;
    public AudioClip timerEndSound;

    AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayTrashPickup() => audioSource.PlayOneShot(trashPickupSound);
    public void PlayTreePlant() => audioSource.PlayOneShot(treePlantSound);
    public void PlayTimeMachine() => audioSource.PlayOneShot(timeMachineSound);
    public void PlayGarbagePickerPickup() => audioSource.PlayOneShot(garbagePickerPickupSound);
    public void PlayShovelPickup() => audioSource.PlayOneShot(shovelPickupSound);
    public void PlayToolSwitch() => audioSource.PlayOneShot(toolSwitchSound);
    public void PlayTimerEnd() => audioSource.PlayOneShot(timerEndSound);
}