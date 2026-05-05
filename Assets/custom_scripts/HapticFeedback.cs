using UnityEngine;

/// <summary>
/// Provides haptic vibration feedback on Android devices.
/// Different vibration patterns for different game events.
/// </summary>
public class HapticFeedback : MonoBehaviour
{
    static HapticFeedback instance;
    static bool isVibrating = false;
    static float pulseTimer = 0f;

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

    void Update()
    {
        // Handle proximity pulse vibration (zombie nearby)
        if (pulseTimer > 0)
        {
            pulseTimer -= Time.deltaTime;
            if (pulseTimer <= 0) isVibrating = false;
        }
    }

    static void InitVibrator()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

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

    // --- Game Event Vibrations ---

    /// <summary>Short vibration for shooting.</summary>
    public static void VibrateShoot()
    {
        Vibrate(50, 128);
    }

    /// <summary>Strong vibration when player takes damage.</summary>
    public static void VibrateHit()
    {
        Vibrate(300, 255);
    }

    /// <summary>Light double-tap vibration for picking up items.</summary>
    public static void VibratePickup()
    {
        VibratePattern(new long[] { 0, 30, 40, 30 }, new int[] { 0, 120, 0, 120 });
    }

    /// <summary>Light vibration for general interaction.</summary>
    public static void VibrateInteract()
    {
        Vibrate(30, 80);
    }

    /// <summary>Long vibration for game over.</summary>
    public static void VibrateGameOver()
    {
        Vibrate(500, 200);
    }

    /// <summary>Heartbeat pulse when zombie is nearby (call repeatedly).</summary>
    public static void VibrateProximityPulse()
    {
        if (isVibrating) return;
        isVibrating = true;
        pulseTimer = 0.7f;
        Vibrate(100, 60);
    }

    /// <summary>Rapid pulse for timer running low.</summary>
    public static void VibrateTimerUrgent()
    {
        VibratePattern(new long[] { 0, 50, 50, 50, 50, 50 }, new int[] { 0, 180, 0, 180, 0, 180 });
    }

    // --- Core Vibration Methods ---

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
        Debug.Log($"[Haptic] {milliseconds}ms amp={amplitude}");
#endif
    }

    public static void VibratePattern(long[] pattern, int[] amplitudes)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (vibrationEffectClass != null && hasAmplitudeControl)
            {
                AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                    "createWaveform", pattern, amplitudes, -1);
                vibrator.Call("vibrate", effect);
            }
            else
            {
                Handheld.Vibrate();
            }
        }
        catch
        {
            Handheld.Vibrate();
        }
#else
        Debug.Log($"[Haptic] Pattern: {pattern.Length} steps");
#endif
    }
}
