using UnityEngine;

public class TimeMachineScript : MonoBehaviour
{
    public Light portalLight;
    public float interactRange = 5f;
    bool glowing = false;

    void Start()
    {

        // Make collider big enough for raycast (model is scale 0.05)
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        //col.size = new Vector3(60f, 60f, 60f);
        col.size = new Vector3(60f, 60f, 60f);
        col.center = new Vector3(0, 30f, 0);
        col.isTrigger = true;

        SetGlow(false);
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            bool shouldGlow = !GameManager.Instance.IsInPresent ||
                              !GameManager.Instance.TimerRunning;
            if (shouldGlow != glowing) SetGlow(shouldGlow);
        }

        // Pulsing light effect
        if (glowing && portalLight != null)
        {
            float pulse = (Mathf.Sin(Time.time * 2f) + 1f) / 2f; // 0 to 1
            portalLight.intensity = Mathf.Lerp(1f, 5f, pulse);
        }
    }

    void SetGlow(bool on)
    {
        glowing = on;
        if (portalLight != null)
        {
            portalLight.enabled = on;
            portalLight.color = GameManager.Instance != null && GameManager.Instance.IsInPresent
                ? new Color(0.2f, 0.8f, 0.2f) // Green = ready to go back
                : new Color(0.2f, 0.5f, 1f);   // Blue = go to present
        }

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in r.materials)
            {
                if (on)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(0.2f, 0.5f, 0.8f) * 2f);
                }
                else
                {
                    mat.DisableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }

    public void ActivateTravel()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsReady) return;

        Debug.Log("Travel button is pressed");
        // Check distance
        Transform player = LocalPlayerHolder.GetLocalCamera();

        if (player != null)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist > interactRange)
            {
                Debug.Log($"Too far: {dist:F1}m (need < {interactRange}m)");
                return;
            }
        }

        // In Present: only allow travel back if all trash collected
        if (GameManager.Instance.IsInPresent && GameManager.Instance.CleanlinessPercent < 100f && GameManager.Instance.TimerRunning)
        {
            Debug.Log("Collect all trash before traveling back!");
            return;
        }

        HapticFeedback.VibrateInteract();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayTimeMachine();
        GameManager.Instance.ActivateTimeMachine();
    }

    public void ResetGlow()
    {
        glowing = false;
    }
}
