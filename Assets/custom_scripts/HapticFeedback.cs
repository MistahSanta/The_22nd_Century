using UnityEngine;

/// <summary>
/// Provides haptic vibration feedback on Android devices.
/// Call the static methods from other scripts (e.g., GunScript, ZombieScript).
/// Attach to Player or any persistent GameObject.
/// </summary>
public class HapticFeedback : MonoBehaviour
{
    static HapticFeedback instance;

#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaObject vibrator;
    static AndroidJavaClass vibrationEffectClass;
    static bool hasAmplitudeControl;
#endif

    void Awake()
    {
        instance = this;
        InitVibrator();
    }

    static void InitVibrator()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            // Check if VibrationEffect is available (API 26+)
            int sdkVersion = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
            if (sdkVersion >= 26)
            {
                vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                hasAmplitudeControl = vibrator.Call<bool>("hasAmplitudeControl");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("HapticFeedback: Failed to init vibrator: " + e.Message);
        }
#endif
    }

    /// <summary>
    /// Short vibration for shooting feedback.
    /// </summary>
    public static void VibrateShoot()
    {
        Vibrate(50, 128);
    }

    /// <summary>
    /// Strong vibration when player gets hit by a zombie.
    /// </summary>
    public static void VibrateHit()
    {
        Vibrate(200, 255);
    }

    /// <summary>
    /// Light vibration for interaction feedback (pickup, etc.).
    /// </summary>
    public static void VibrateInteract()
    {
        Vibrate(30, 80);
    }

    /// <summary>
    /// Vibrate with specified duration (ms) and amplitude (1-255).
    /// Falls back to Handheld.Vibrate() on older devices.
    /// </summary>
    public static void Vibrate(long milliseconds, int amplitude = -1)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (vibrationEffectClass != null)
            {
                int amp = hasAmplitudeControl ? amplitude : -1;
                AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                    "createOneShot", milliseconds, amp);
                vibrator.Call("vibrate", effect);
            }
            else
            {
                Handheld.Vibrate();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("HapticFeedback: Vibrate failed: " + e.Message);
            Handheld.Vibrate();
        }
#else
        Debug.Log($"[HapticFeedback] Vibrate {milliseconds}ms, amplitude {amplitude} (editor - no vibration)");
#endif
    }
}
