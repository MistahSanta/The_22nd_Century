using UnityEngine;

public class TreePlanting : MonoBehaviour
{
    public GameObject treePrefab;

    void Start()
    {
        var interactable = GetComponent<InteractableObjectScript>();
        if (interactable != null)
        {
            interactable.maxInteractDistance = 4f;
            interactable.promptText = GetPromptText();
        }

        if (GetComponent<TreeSpotProximityDetector>() == null)
            gameObject.AddComponent<TreeSpotProximityDetector>();

        // Green glow
        try
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 0.1f) * 0.5f);
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

    public void Plant()
    {
        if (GameManager.Instance != null && !GameManager.Instance.HasShovel)
        {
            Debug.Log("Need shovel first!");
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.TimerRunning)
        {
            Debug.Log("Time's up!");
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentTool != GameManager.EquippedTool.Shovel)
        {
            Debug.Log("Need shovel to plant trees!");
            return;
        }

        if (treePrefab != null)
            Instantiate(treePrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlantTree();
            HapticFeedback.VibrateInteract();
        }

        gameObject.SetActive(false);
    }

    public string GetPromptText()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentTool == GameManager.EquippedTool.None)
            return "Pick up Shovel first!\n[Press A]";

        if (GameManager.Instance != null && GameManager.Instance.CurrentTool != GameManager.EquippedTool.Shovel)
            return "Switch to Shovel!\n[Press X]";

        if (GameManager.Instance != null && !GameManager.Instance.TimerRunning)
            return "Time's up! Return to the Time Machine!";

        return "Plant Tree\n[Press A]";
    }
}