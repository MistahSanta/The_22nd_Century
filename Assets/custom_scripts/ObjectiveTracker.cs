using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Shows current objective, direction indicator, distance, and light beam on target.
/// Attach to Player.
/// </summary>
public class ObjectiveTracker : MonoBehaviour
{
    // UI elements
    GameObject uiCanvas;
    TextMeshProUGUI objectiveText;
    TextMeshProUGUI directionText;
    TextMeshProUGUI distanceText;
    Image directionArrow;

    // Light beam on target
    GameObject lightBeam;
    LineRenderer beamLine;

    // State
    Transform currentTarget;
    string currentObjective = "";
    Camera mainCam;
    bool wasInPresent = false;
    GameObject missionBriefing;
    float briefingTimer = 0;

    // Kill counter
    GameObject killCounterCanvas;
    TMPro.TextMeshProUGUI killCounterText;
    int lastZombieCount = -1;
    int totalKills = 0;

    // Victory screen
    bool victoryShown = false;

    // Proximity vibration
    float proximityPulseTimer = 0f;

    void Start()
    {
        mainCam = GetComponentInChildren<Camera>();
        if (mainCam == null) mainCam = Camera.main;
        CreateUI();
        CreateLightBeam();
        Debug.Log("ObjectiveTracker ready on " + gameObject.name);
    }

    void Update()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsTutorialActive) return;

        try
        {
            UpdateObjective();
            UpdateDirectionIndicator();
            UpdateLightBeam();
            UpdateProximityVibration();
            UpdateTimerVibration();
            UpdateUIPosition();
            UpdateMissionBriefing();
            UpdateKillCounter();
            CheckVictory();
        }
        catch { }
    }

    void UpdateObjective()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        string newObjective = "";
        Transform newTarget = null;

        try
        {
            if (gm.IsGameOver)
            {
                newObjective = "";
                newTarget = null;
            }
            else if (!gm.IsInPresent)
            {
                // Future world — check gun first
                GunScript gunScript = FindObjectOfType<GunScript>();
                bool hasGun = (gunScript != null && gunScript.IsEquipped());

                if (!hasGun)
                {
                    // Step 1: Must pick up gun first
                    newObjective = "Find and pick up the Gun";
                    if (gunScript != null)
                        newTarget = gunScript.transform;
                    else if (gm.gunObject != null && gm.gunObject.activeSelf)
                        newTarget = gm.gunObject.transform;
                }
                else
                {
                    // Step 2: Find Time Machine (killing zombies is optional/side task)
                    newObjective = "Find the Time Machine\n(Shoot zombies in your way!)";
                    if (gm.timeMachineObject != null)
                        newTarget = gm.timeMachineObject.transform;
                }
            }
            else
            {
                // Present world — show current tool
                string toolInfo = "";
                if (gm.CurrentTool == GameManager.EquippedTool.GarbagePicker)
                    toolInfo = "[Holding: Garbage Picker]  ";
                else if (gm.CurrentTool == GameManager.EquippedTool.Shovel)
                    toolInfo = "[Holding: Shovel]  ";
                else if (!gm.HasGarbagePicker && !gm.HasShovel)
                    toolInfo = "[No tool]  ";

                if (!gm.HasGarbagePicker && !gm.HasShovel)
                {
                    newObjective = toolInfo + "Find tools nearby!";
                    var grabber = FindObjectOfType<TrashGrabberScript>();
                    if (grabber != null) newTarget = grabber.transform;
                }
                else if (gm.CleanlinessPercent < 100f)
                {
                    newObjective = toolInfo + $"Trash: {gm.TrashCollected}/{gm.totalTrashInPresent}  Trees: {gm.TreesPlanted}/{gm.totalTreesInPresent}";

                    if (gm.CurrentTool == GameManager.EquippedTool.Shovel)
                        newObjective += "\nFind green spots to plant trees";
                    else if (gm.CurrentTool == GameManager.EquippedTool.GarbagePicker)
                        newObjective += "\nPick up red glowing trash";
                    else
                        newObjective += "\nPress X to switch tools";

                    var trashItems = FindObjectsByType<TrashPickup>(FindObjectsSortMode.None);
                    float closest = float.MaxValue;
                    foreach (var t in trashItems)
                    {
                        if (!t.gameObject.activeSelf) continue;
                        float d = Vector3.Distance(transform.position, t.transform.position);
                        if (d < closest) { closest = d; newTarget = t.transform; }
                    }
                }
                else
                {
                    newObjective = "All clean! Return to Time Machine";
                    if (gm.timeMachineObject != null)
                        newTarget = gm.timeMachineObject.transform;
                }

                // No timer display in objective bar
            }
        }
        catch
        {
            // Networked properties not ready yet
            newObjective = "Initializing...";
        }

        if (newObjective != currentObjective)
        {
            currentObjective = newObjective;
            if (objectiveText != null)
                objectiveText.text = currentObjective;
        }

        currentTarget = newTarget;
    }

    void UpdateDirectionIndicator()
    {
        if (currentTarget == null || mainCam == null)
        {
            if (directionText != null) directionText.text = "";
            if (distanceText != null) distanceText.text = "";
            if (directionArrow != null) directionArrow.color = Color.clear;
            return;
        }

        Vector3 toTarget = currentTarget.position - transform.position;
        toTarget.y = 0;
        float distance = toTarget.magnitude;

        // Distance text
        if (distanceText != null)
            distanceText.text = $"{distance:F0}m";

        // Direction relative to camera forward
        Vector3 camForward = mainCam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        float angle = Vector3.SignedAngle(camForward, toTarget.normalized, Vector3.up);

        // Arrow rotation
        if (directionArrow != null)
        {
            directionArrow.color = Color.white;
            directionArrow.transform.localRotation = Quaternion.Euler(0, 0, -angle);

            // Green when facing target (within 30 degrees)
            bool facingTarget = Mathf.Abs(angle) < 30f;
            directionArrow.color = facingTarget ? Color.green : Color.white;
            if (directionText != null)
                directionText.color = facingTarget ? Color.green : Color.white;
        }
    }

    void UpdateLightBeam()
    {
        if (lightBeam == null) return;

        try
        {
            if (currentTarget == null || GameManager.Instance == null || GameManager.Instance.IsGameOver)
            {
                lightBeam.SetActive(false);
                return;
            }
        }
        catch { lightBeam.SetActive(false); return; }

        lightBeam.SetActive(true);

        float beamHeight = 25f;
        if (currentTarget.GetComponent<TrashPickup>() != null)
            beamHeight = 10f;

        Vector3 basePos = currentTarget.position;
        if (beamLine != null)
        {
            beamLine.SetPosition(0, basePos + Vector3.up * 0.2f);
            beamLine.SetPosition(1, basePos + Vector3.up * beamHeight);
        }
    }

    void UpdateProximityVibration()
    {
        try { if (GameManager.Instance == null || GameManager.Instance.IsInPresent) return; } catch { return; }

        // Find closest zombie
        var zombies = Object.FindObjectsByType<ZombieScript>(FindObjectsSortMode.None);
        float closestDist = float.MaxValue;
        foreach (var z in zombies)
        {
            float d = Vector3.Distance(transform.position, z.transform.position);
            if (d < closestDist) closestDist = d;
        }

        // Pulse vibration when zombie is close
        if (closestDist < 6f)
        {
            proximityPulseTimer -= Time.deltaTime;
            if (proximityPulseTimer <= 0)
            {
                HapticFeedback.VibrateProximityPulse();
                // Faster pulses when closer
                proximityPulseTimer = Mathf.Lerp(0.3f, 0.8f, closestDist / 5f);
            }
        }
        else
        {
            proximityPulseTimer = 0;
        }
    }

    void UpdateTimerVibration()
    {
        try { if (GameManager.Instance == null || !GameManager.Instance.TimerRunning) return; } catch { return; }

        float timeLeft = GameManager.Instance.TimeRemaining;

        // Vibrate when timer is running low
        if (timeLeft <= 10f && timeLeft > 0)
        {
            proximityPulseTimer -= Time.deltaTime;
            if (proximityPulseTimer <= 0)
            {
                HapticFeedback.VibrateTimerUrgent();
                proximityPulseTimer = 2f;
            }
        }
    }

    // --- UI Creation ---

    void CreateUI()
    {
        uiCanvas = new GameObject("ObjectiveCanvas");
        uiCanvas.transform.SetParent(transform, false);
        Canvas canvas = uiCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 120;

        RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 100);
        canvasRect.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        // Background panel
        GameObject panel = new GameObject("ObjectivePanel");
        panel.transform.SetParent(uiCanvas.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.5f);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 100);

        // Objective text
        GameObject textObj = new GameObject("ObjectiveText");
        textObj.transform.SetParent(panel.transform, false);
        objectiveText = textObj.AddComponent<TextMeshProUGUI>();
        objectiveText.text = "";
        objectiveText.fontSize = 18;
        objectiveText.color = Color.white;
        objectiveText.alignment = TextAlignmentOptions.MidlineLeft;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(0.7f, 1);
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(0, -5);

        // Direction arrow (right side of panel)
        GameObject arrowObj = new GameObject("DirectionArrow");
        arrowObj.transform.SetParent(panel.transform, false);
        directionArrow = arrowObj.AddComponent<Image>();
        directionArrow.color = Color.white;
        var arrowRect = arrowObj.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(0.75f, 0.3f);
        arrowRect.anchorMax = new Vector2(0.85f, 0.8f);
        arrowRect.offsetMin = Vector2.zero;
        arrowRect.offsetMax = Vector2.zero;

        // Direction text (compass label)
        GameObject dirObj = new GameObject("DirectionLabel");
        dirObj.transform.SetParent(panel.transform, false);
        directionText = dirObj.AddComponent<TextMeshProUGUI>();
        directionText.text = "";
        directionText.fontSize = 14;
        directionText.color = Color.white;
        directionText.alignment = TextAlignmentOptions.Center;
        var dirRect = dirObj.GetComponent<RectTransform>();
        dirRect.anchorMin = new Vector2(0.7f, 0);
        dirRect.anchorMax = new Vector2(0.88f, 0.35f);
        dirRect.offsetMin = Vector2.zero;
        dirRect.offsetMax = Vector2.zero;

        // Distance text
        GameObject distObj = new GameObject("DistanceText");
        distObj.transform.SetParent(panel.transform, false);
        distanceText = distObj.AddComponent<TextMeshProUGUI>();
        distanceText.text = "";
        distanceText.fontSize = 16;
        distanceText.color = Color.white;
        distanceText.alignment = TextAlignmentOptions.Center;
        var distRect = distObj.GetComponent<RectTransform>();
        distRect.anchorMin = new Vector2(0.88f, 0.2f);
        distRect.anchorMax = new Vector2(1f, 0.8f);
        distRect.offsetMin = Vector2.zero;
        distRect.offsetMax = Vector2.zero;
    }

    void UpdateMissionBriefing()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        bool inPresent = gm.IsInPresent;

        // Detect transition to Present
        if (inPresent && !wasInPresent)
        {
            ShowMissionBriefing();
        }
        wasInPresent = inPresent;

        // Auto-dismiss after timeout or button press
        if (missionBriefing != null && missionBriefing.activeSelf)
        {
            briefingTimer -= Time.deltaTime;
            bool dismiss = briefingTimer <= 0;
            if (ControllerMapping.Instance != null)
                dismiss = dismiss || ControllerMapping.Instance.GetInteractDown();
            if (Input.GetKeyDown(KeyCode.E)) dismiss = true;

            if (dismiss)
                missionBriefing.SetActive(false);

            // Keep in front of camera
            if (mainCam != null)
            {
                missionBriefing.transform.position = mainCam.transform.position
                    + mainCam.transform.forward * 1.5f;
                missionBriefing.transform.rotation = Quaternion.LookRotation(
                    missionBriefing.transform.position - mainCam.transform.position);
            }
        }
    }

    void ShowMissionBriefing()
    {
        if (missionBriefing == null)
        {
            missionBriefing = new GameObject("MissionBriefing");
            missionBriefing.transform.SetParent(transform, false);
            Canvas c = missionBriefing.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            c.sortingOrder = 180;

            RectTransform cRect = missionBriefing.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(500, 250);
            cRect.localScale = new Vector3(0.003f, 0.003f, 0.003f);

            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(missionBriefing.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.85f);
            bg.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 250);

            GameObject txt = new GameObject("Text");
            txt.transform.SetParent(bg.transform, false);
            var text = txt.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "MISSION: CLEAN THE PRESENT\n\n" +
                        "1. Pick up Garbage Picker & Shovel\n" +
                        "2. Collect trash & plant trees\n" +
                        "3. Return to Time Machine before time runs out!\n\n" +
                        "You have 2 minutes. Good luck!\n\n" +
                        "[Press A to start]";
            text.fontSize = 22;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.color = Color.white;
            var tRect = txt.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
        }

        missionBriefing.SetActive(true);
        briefingTimer = 12f; // Auto dismiss after 15s
    }

    void UpdateKillCounter()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Only show in Future
        bool inFuture = !gm.IsInPresent;

        if (killCounterCanvas == null && inFuture)
            CreateKillCounter();

        if (killCounterCanvas != null)
        {
            killCounterCanvas.SetActive(inFuture);

            if (inFuture && mainCam != null)
            {
                // Position between objective (top-left) and hearts (top-right)
                killCounterCanvas.transform.position = mainCam.transform.position
                    + mainCam.transform.forward * 1.2f
                    + mainCam.transform.up * 0.55f;
                killCounterCanvas.transform.rotation = Quaternion.LookRotation(
                    killCounterCanvas.transform.position - mainCam.transform.position);

                // Count zombies killed
                var zombies = FindObjectsByType<ZombieScript>(FindObjectsSortMode.None);
                int currentCount = zombies.Length;
                if (lastZombieCount >= 0 && currentCount < lastZombieCount)
                    totalKills += (lastZombieCount - currentCount);
                lastZombieCount = currentCount;

                if (killCounterText != null)
                    killCounterText.text = "Kills: " + totalKills;
            }
        }
    }

    void CreateKillCounter()
    {
        killCounterCanvas = new GameObject("KillCounter");
        killCounterCanvas.transform.SetParent(transform, false);
        Canvas c = killCounterCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingOrder = 155;

        RectTransform cRect = killCounterCanvas.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(150, 40);
        cRect.localScale = new Vector3(0.003f, 0.003f, 0.003f);

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(killCounterCanvas.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0, 0, 0.5f);
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 40);

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(bg.transform, false);
        killCounterText = txt.AddComponent<TMPro.TextMeshProUGUI>();
        killCounterText.text = "Kills: 0";
        killCounterText.fontSize = 24;
        killCounterText.alignment = TMPro.TextAlignmentOptions.Center;
        killCounterText.color = Color.white;
        var tRect = txt.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;
    }

    void CheckVictory()
    {
        if (victoryShown) return;
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Victory: back in future with 100% cleanliness
        try
        {
            if (!gm.IsInPresent && gm.CleanlinessPercent >= 100f && totalKills > 0)
            {
                victoryShown = true;
                ShowVictoryScreen();
            }
        }
        catch { }
    }

    void ShowVictoryScreen()
    {
        GameObject victory = new GameObject("VictoryScreen");
        victory.transform.SetParent(transform, false);
        Canvas c = victory.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingOrder = 300;

        RectTransform cRect = victory.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(600, 300);
        cRect.localScale = new Vector3(0.003f, 0.003f, 0.003f);

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(victory.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0.3f, 0, 0.85f);
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 300);

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(bg.transform, false);
        var text = txt.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "YOU SAVED THE FUTURE!\n\n" +
                    "Zombies Defeated: " + totalKills + "\n" +
                    "The world is clean and restored.\n\n" +
                    "Thank you for playing!";
        text.fontSize = 28;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = Color.white;
        var tRect = txt.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;

        // Keep in front of camera
        StartCoroutine(FollowCamera(victory));
    }

    System.Collections.IEnumerator FollowCamera(GameObject ui)
    {
        while (ui != null)
        {
            if (mainCam != null)
            {
                ui.transform.position = mainCam.transform.position + mainCam.transform.forward * 2f;
                ui.transform.rotation = Quaternion.LookRotation(
                    ui.transform.position - mainCam.transform.position);
            }
            yield return null;
        }
    }

    void UpdateUIPosition()
    {
        if (uiCanvas == null || mainCam == null) return;

        // Top-left of player view — closer and higher
        uiCanvas.transform.position = mainCam.transform.position
            + mainCam.transform.forward * 1.2f
            + mainCam.transform.up * 0.6f
            - mainCam.transform.right * 0.55f;
        uiCanvas.transform.rotation = Quaternion.LookRotation(
            uiCanvas.transform.position - mainCam.transform.position);
    }

    void CreateLightBeam()
    {
        lightBeam = new GameObject("TargetBeam");

        // Use LineRenderer for a natural transparent beam
        beamLine = lightBeam.AddComponent<LineRenderer>();
        beamLine.positionCount = 2;
        beamLine.startWidth = 0.08f;
        beamLine.endWidth = 0.01f;
        beamLine.useWorldSpace = true;
        beamLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beamLine.receiveShadows = false;

        // Transparent additive material (doesn't block objects behind it)
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
        mat.SetFloat("_Mode", 2); // Fade mode
        mat.renderQueue = 3000;
        mat.color = new Color(0.9f, 0.95f, 1f, 0.12f);
        beamLine.material = mat;

        // Gradient: bright at bottom, fade at top
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.9f, 0.95f, 1f), 0f),
                new GradientColorKey(new Color(0.9f, 0.95f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.3f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        beamLine.colorGradient = gradient;

        lightBeam.SetActive(false);
    }
}
