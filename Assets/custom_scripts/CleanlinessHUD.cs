using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows cleanliness progress bar only in Present world.
/// Attach to a World Space Canvas that follows the player.
/// </summary>
public class CleanlinessHUD : MonoBehaviour
{
    [Header("UI Elements")]
    public Image progressBarFill;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI percentText;
    public GameObject hudPanel;

    [Header("Follow Settings")]
    public Transform playerCamera;
    public float followDistance = 2.5f;
    public float heightOffset = 1.2f;
    public float followSpeed = 3f;

    void Start()
    {
        // Auto-find panel if not assigned
        if (hudPanel == null && transform.childCount > 0)
            hudPanel = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Only show HUD in Present world
        bool showHUD = GameManager.Instance.IsInPresent;
        if (hudPanel != null)
            hudPanel.SetActive(showHUD);

        if (!showHUD) return;

        float pct = GameManager.Instance.CleanlinessPercent;
        int collected = GameManager.Instance.TrashCollected;
        int total = GameManager.Instance.totalTrashInPresent;

        // Update progress bar
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = pct / 100f;

            // Color: red -> yellow -> green
            if (pct < 25f)
                progressBarFill.color = new Color(0.9f, 0.2f, 0.2f);
            else if (pct < 50f)
                progressBarFill.color = new Color(0.9f, 0.7f, 0.2f);
            else if (pct < 75f)
                progressBarFill.color = new Color(0.5f, 0.8f, 0.2f);
            else
                progressBarFill.color = new Color(0.2f, 0.9f, 0.3f);
        }

        if (percentText != null)
            percentText.text = $"{collected}/{total}";

        if (statusText != null)
        {
            if (collected >= total)
                statusText.text = "All clean! Go to the Time Machine!";
            else
                statusText.text = "Pick up trash to save the future!";
        }

        // Follow player camera
        if (playerCamera != null)
        {
            Vector3 targetPos = playerCamera.position
                + playerCamera.forward * followDistance
                + Vector3.up * heightOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(transform.position - playerCamera.position);
        }
    }
}
