using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CleanlinessHUD : NetworkBehaviour
{
    [Header("UI Elements")]
    public Image progressBarFill;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI percentText;
    public GameObject hudPanel;
    public TextMeshProUGUI timerText;

    [Header("Follow Settings")]
    public float followDistance = 2.5f;
    public float heightOffset = 1.2f;
    public float followSpeed = 3f;

    Transform _camera;

    // Try to find local camera every frame until found
    void EnsureCamera()
    {
        if (_camera != null) return;
        var cam = Camera.main;
        if (cam != null)
        {
            _camera = cam.transform;
            Debug.Log("[CleanlinessHUD] Camera found: " + cam.name);
        }
    }

    void Update()
    {
        EnsureCamera();

        if (GameManager.Instance == null || !GameManager.Instance.IsReady) return;

        bool showHUD = GameManager.Instance.IsInPresent;
        if (hudPanel != null) hudPanel.SetActive(showHUD);
        if (!showHUD) return;

        RefreshBar();
        RefreshTimer();
        FollowCamera();
    }

    void RefreshBar()
    {
        float pct = GameManager.Instance.CleanlinessPercent;
        int collected = GameManager.Instance.TrashCollected;
        int trees = GameManager.Instance.TreesPlanted;
        int totalItems = GameManager.Instance.totalTrashInPresent + GameManager.Instance.totalTreesInPresent;

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = pct / 100f;
            progressBarFill.color = pct < 25f ? new Color(0.9f, 0.2f, 0.2f)
                                  : pct < 50f ? new Color(0.9f, 0.7f, 0.2f)
                                  : pct < 75f ? new Color(0.5f, 0.8f, 0.2f)
                                              : new Color(0.2f, 0.9f, 0.3f);
        }

        if (percentText != null)
            percentText.text = $"{collected + trees}/{totalItems}";

        if (statusText != null)
            statusText.text = "Pick up trash and plant trees to save the future!";
    }

    void RefreshTimer()
    {
        if (timerText == null) return;

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

    void FollowCamera()
    {
        if (_camera == null) return;

        Vector3 target = _camera.position
            + _camera.forward * followDistance
            + Vector3.up * heightOffset;

        // Use Time.deltaTime here — this is Update(), not FixedUpdateNetwork()
        transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(transform.position - _camera.position);
    }
}