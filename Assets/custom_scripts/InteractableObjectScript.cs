using UnityEngine;
using UnityEngine.Events;

public class InteractableObjectScript : MonoBehaviour
{
    public bool pointer_on_obj = false;
    public UnityEvent runButtonClickFunction;

    [Header("Settings")]
    public string promptText = "Interact";
    public float maxInteractDistance = 5f;

    static InteractableObjectScript currentlyTargeted;

    // Shared world-space prompt UI
    static GameObject worldPrompt;
    static TMPro.TextMeshProUGUI worldPromptText;

    public void PointerEnter()
    {
        pointer_on_obj = true;
        currentlyTargeted = this;
        SetHighlight(true);

        string display = BuildPromptText();
        ShowWorldPrompt(display);
    }

    public void OnPointerExit()
    {
        pointer_on_obj = false;
        if (currentlyTargeted == this) currentlyTargeted = null;
        SetHighlight(false);
        HideWorldPrompt();
    }

    string BuildPromptText()
    {
        string display = promptText;
        var tm = GetComponent<TimeMachineScript>();
        if (tm != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.IsInPresent)
            {
                if (!GameManager.Instance.TimerRunning)
                    display = "Travel back to Future";
                else
                    display = "Pick up trash and plant trees!";
            }
            else
                display = "Travel to Present";
        }

        if (GetComponent<TrashPickup>() != null)
            return display;

        if (GetComponent<TreePlanting>() != null)
            return display;

        return display + "\n[Press A]";
    }

    void SetHighlight(bool on)
    {
        try
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                foreach (Material mat in r.materials)
                {
                    if (on)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.2f) * 0.5f);
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", Color.black);
                    }
                }
        }
        catch { }
    }

    void Update()
    {
        if (!pointer_on_obj) return;

        bool interact = ControllerMapping.Instance != null
            ? ControllerMapping.Instance.GetInteractDown()
            : Input.GetKeyDown(KeyCode.E);

        if (interact && runButtonClickFunction != null)
        {
            
            runButtonClickFunction.Invoke();

            pointer_on_obj = false;
            currentlyTargeted = null;
            SetHighlight(false);
            HideWorldPrompt();
        }

        // Keep prompt in front of camera
        UpdatePromptPosition();
    }

    // ---- World Space Prompt (works in VR) ----

    static void CreateWorldPrompt()
    {
        if (worldPrompt != null) return;

        worldPrompt = new GameObject("InteractPrompt");
        Object.DontDestroyOnLoad(worldPrompt);

        // Canvas
        Canvas canvas = worldPrompt.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        worldPrompt.AddComponent<UnityEngine.UI.CanvasScaler>();

        RectTransform canvasRect = worldPrompt.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(300, 60);
        canvasRect.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // Background
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(worldPrompt.transform, false);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(300, 60);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bg.transform, false);
        worldPromptText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        worldPromptText.fontSize = 24;
        worldPromptText.alignment = TMPro.TextAlignmentOptions.Center;
        worldPromptText.color = Color.white;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        worldPrompt.SetActive(false);
    }

    static void ShowWorldPrompt(string text)
    {
        CreateWorldPrompt();
        if (worldPromptText != null) worldPromptText.text = text;
        if (worldPrompt != null) worldPrompt.SetActive(true);
        UpdatePromptPosition();
    }

    static void HideWorldPrompt()
    {
        if (worldPrompt != null) worldPrompt.SetActive(false);
    }

    static void UpdatePromptPosition()
    {
        if (worldPrompt == null || !worldPrompt.activeSelf) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Position prompt 2m in front of camera, slightly below center
        Vector3 pos = cam.transform.position
            + cam.transform.forward * 2f
            - cam.transform.up * 0.3f;
        worldPrompt.transform.position = pos;
        worldPrompt.transform.rotation = Quaternion.LookRotation(
            worldPrompt.transform.position - cam.transform.position);
    }
}
