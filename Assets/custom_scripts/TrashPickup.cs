using UnityEngine;

/// <summary>
/// Trash item. Can only be collected after picking up the garbage picker.
/// </summary>
public class TrashPickup : MonoBehaviour
{
    void Start()
    {
        var interactable = GetComponent<InteractableObjectScript>();
        if (interactable != null)
        {
            interactable.maxInteractDistance = 4f;
            interactable.promptText = GetPromptText();
        }

        // Add proximity detector for gaze interaction
        if (GetComponent<TrashProximityDetector>() == null)
            gameObject.AddComponent<TrashProximityDetector>();

        // Add slight red glow so trash is visible
        try
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                foreach (Material mat in r.materials)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(0.8f, 0.2f, 0.1f) * 0.5f);
                }
        }
        catch { }
    }

    void Update()
    {
        var interactable = GetComponent<InteractableObjectScript>();
        if (interactable != null && interactable.pointer_on_obj)
            interactable.promptText = GetPromptText();
    }

    public void Collect()
    {
        // Can only collect if player has garbage picker
        if (GameManager.Instance != null && !GameManager.Instance.HasGarbagePicker)
        {
            Debug.Log("Need garbage picker first!");
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.TimerRunning)
        {
            Debug.Log("Time's up! Can't collect trash anymore!");
            return;
        }

        if (GameManager.Instance.CurrentTool != GameManager.EquippedTool.GarbagePicker)
        {
            Debug.Log("Need garbage picker to collect trash!");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectTrash();
            HapticFeedback.VibrateInteract();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayTrashPickup();
        }
        gameObject.SetActive(false);
    }

    public string GetPromptText()
    {
        if (GameManager.Instance != null && !GameManager.Instance.TimerRunning && GameManager.Instance.TimeRemaining == 0f)
            return "Time's up! Can't collect trash anymore!";

        if (GameManager.Instance != null && !GameManager.Instance.HasGarbagePicker)
            return "Pick up Garbage Picker!\n[Press A]";

        if (GameManager.Instance != null && GameManager.Instance.CurrentTool != GameManager.EquippedTool.GarbagePicker)
            return "Switch to Garbage Picker!\n[Press X]";

        return "Pick up Trash\n[Press A]";
    }
}
