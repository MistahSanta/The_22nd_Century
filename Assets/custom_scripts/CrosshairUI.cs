using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Screen-space crosshair reticle and interaction prompt.
/// Auto-creates its own Canvas on Start if needed.
/// </summary>
public class CrosshairUI : MonoBehaviour
{
    public static CrosshairUI Instance { get; private set; }

    TextMeshProUGUI promptLabel;
    GameObject promptPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        // Create Screen Space Overlay Canvas
        GameObject canvasObj = new GameObject("CrosshairCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Crosshair dot
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(canvasObj.transform, false);
        Image crossImg = crosshair.AddComponent<Image>();
        crossImg.color = Color.white;
        RectTransform crossRect = crosshair.GetComponent<RectTransform>();
        crossRect.anchoredPosition = Vector2.zero;
        crossRect.sizeDelta = new Vector2(6, 6);

        // Crosshair lines (horizontal)
        CreateLine(canvasObj.transform, new Vector2(12, 2), new Vector2(-15, 0));
        CreateLine(canvasObj.transform, new Vector2(12, 2), new Vector2(15, 0));
        // Crosshair lines (vertical)
        CreateLine(canvasObj.transform, new Vector2(2, 12), new Vector2(0, 15));
        CreateLine(canvasObj.transform, new Vector2(2, 12), new Vector2(0, -15));

        // Prompt panel (below crosshair)
        promptPanel = new GameObject("PromptPanel");
        promptPanel.transform.SetParent(canvasObj.transform, false);
        Image panelImg = promptPanel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.6f);
        RectTransform panelRect = promptPanel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = new Vector2(0, -60);
        panelRect.sizeDelta = new Vector2(300, 50);

        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptPanel.transform, false);
        promptLabel = textObj.AddComponent<TextMeshProUGUI>();
        promptLabel.fontSize = 16;
        promptLabel.alignment = TextAlignmentOptions.Center;
        promptLabel.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        promptPanel.SetActive(false);
    }

    void CreateLine(Transform parent, Vector2 size, Vector2 pos)
    {
        GameObject line = new GameObject("CrosshairLine");
        line.transform.SetParent(parent, false);
        Image img = line.AddComponent<Image>();
        img.color = Color.white;
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
    }

    public void ShowPrompt(string text)
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (promptLabel != null) promptLabel.text = text;
    }

    public void HidePrompt()
    {
        if (promptPanel != null) promptPanel.SetActive(false);
    }
}
