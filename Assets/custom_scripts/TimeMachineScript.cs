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
        col.size = new Vector3(60f, 60f, 60f);
        col.center = new Vector3(0, 30f, 0);
        col.isTrigger = true;

        SetGlow(false);
    }

    void Update()
    {
        // In Present: glow only when all trash collected (100%)
        // In Future: always glow (so player can travel to present)
        if (GameManager.Instance != null)
        {
            bool shouldGlow = !GameManager.Instance.IsInPresent ||
                              GameManager.Instance.CleanlinessPercent >= 100f;
            if (shouldGlow != glowing) SetGlow(shouldGlow);
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
        if (GameManager.Instance == null) return;

        // Check distance
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist > interactRange)
            {
                Debug.Log($"Too far: {dist:F1}m (need < {interactRange}m)");
                return;
            }
        }

        // In Present: only allow travel back if all trash collected
        if (GameManager.Instance.IsInPresent && GameManager.Instance.CleanlinessPercent < 100f)
        {
            Debug.Log("Collect all trash before traveling back!");
            return;
        }

        HapticFeedback.VibrateInteract();
        GameManager.Instance.ActivateTimeMachine();
    }
}
