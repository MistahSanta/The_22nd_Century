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

    [Header("Timer UI")]
    public TextMeshProUGUI timerText;

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
        {
            int trees = GameManager.Instance.TreesPlanted;
            int totalItems = GameManager.Instance.totalTrashInPresent + GameManager.Instance.totalTreesInPresent;
            percentText.text = $"{collected + trees}/{totalItems}";
        }

        if (statusText != null)
        {
            statusText.text = "Pick up trash and plant trees to save the future!";
        }

        if (timerText != null)
        {
            if (GameManager.Instance.TimerRunning)
            {
                int seconds = Mathf.CeilToInt(GameManager.Instance.TimeRemaining);
                timerText.text = $"Time: {seconds}s";
                timerText.color = seconds <= 10 ? Color.red : Color.white;
            }
            else if (GameManager.Instance.IsInPresent)
            {
                timerText.text = "Time's up! Return to the Time Machine!";
                timerText.color = Color.red;
            }
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
