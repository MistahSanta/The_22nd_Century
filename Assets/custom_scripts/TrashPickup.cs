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
            interactable.promptText = "Pick up Trash";
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

    public void Collect()
    {
        // Can only collect if player has garbage picker
        if (GameManager.Instance != null && !GameManager.Instance.HasGarbagePicker)
        {
            Debug.Log("Need garbage picker first!");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectTrash();
            HapticFeedback.VibrateInteract();
        }
        gameObject.SetActive(false);
    }
}
