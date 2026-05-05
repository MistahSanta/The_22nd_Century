using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player health with heart system. Uses HealthHeartSystem asset for visuals.
/// Falls back to simple squares if asset not available.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public bool isLocalPlayer = true;
    int currentHealth;

    Color heartFull = new Color(0.9f, 0.1f, 0.1f);
    Color heartEmpty = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    float invincibleTimer = 0f;
    float invincibleDuration = 1.5f;

    Image damageFlash;
    Camera playerCam;
    GameObject gameOverUI;

    // Heart system integration
    PlayerStats playerStats;

    public static PlayerHealth Instance { get; private set; }

    void Awake()
    {
        if (GetComponents<PlayerHealth>().Length > 1)
        {
            Destroy(this);
            return;
        }
    }

    bool heartSystemReady = false;
    bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        invincibleDuration = 2.5f; // longer invincibility window

        if (isLocalPlayer)
        {
            Instance = this;
            playerCam = GetComponentInChildren<Camera>();
            if (playerCam == null) playerCam = Camera.main;
            CreateDamageFlash();
        }
        else
        {
            // Remote player: show floating health bar above head
            CreateOverheadHealthBar();
        }

        Debug.Log("PlayerHealth ready on " + gameObject.name + " local=" + isLocalPlayer);
    }

    void Update()
    {
        if (invincibleTimer > 0)
            invincibleTimer -= Time.deltaTime;

        if (damageFlash != null && damageFlash.color.a > 0)
        {
            Color c = damageFlash.color;
            c.a = Mathf.Max(0, c.a - Time.deltaTime * 1.5f);
            damageFlash.color = c;
        }

        // Remote player: update overhead bar
        if (!isLocalPlayer)
        {
            UpdateOverheadBar();
            return;
        }

        // Show hearts only after tutorial ends
        if (!heartSystemReady)
        {
            try
            {
                if (GameManager.Instance != null && !GameManager.Instance.IsTutorialActive)
                {
                    SetupHeartSystem();
                    heartSystemReady = true;
                }
            }
            catch { }
        }

        // Check if timer ran out
        if (!isDead && heartSystemReady)
        {
            try
            {
                if (GameManager.Instance != null && GameManager.Instance.IsInPresent
                    && !GameManager.Instance.TimerRunning && GameManager.Instance.TimeRemaining <= 0)
                {
                    Die();
                }
            }
            catch { }
        }
    }

    // Other player health tracking
    GameObject otherPlayerHealthUI;
    Image[] otherHeartImages;
    float otherHealthCheckTimer = 0;

    void LateUpdate()
    {
        if (gameOverUI != null)
        {
            if (isLocalPlayer && playerCam != null)
            {
                gameOverUI.transform.position = playerCam.transform.position
                    + playerCam.transform.forward * 1.5f;
                gameOverUI.transform.rotation = Quaternion.LookRotation(
                    gameOverUI.transform.position - playerCam.transform.position);
            }
            else
            {
                gameOverUI.transform.position = transform.position + Vector3.up * 3f;
                Camera cam = Camera.main;
                if (cam != null)
                    gameOverUI.transform.rotation = Quaternion.LookRotation(
                        gameOverUI.transform.position - cam.transform.position);
            }
        }

        // Check for other players' health
        otherHealthCheckTimer -= Time.deltaTime;
        if (otherHealthCheckTimer <= 0 && playerCam != null)
        {
            otherHealthCheckTimer = 0.5f;
            UpdateOtherPlayerHealth();
        }
    }

    void UpdateOtherPlayerHealth()
    {
        var allHealth = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        PlayerHealth other = null;
        foreach (var h in allHealth)
        {
            if (h != this) { other = h; break; }
        }

        if (other == null)
        {
            if (otherPlayerHealthUI != null) otherPlayerHealthUI.SetActive(false);
            return;
        }

        if (otherPlayerHealthUI == null)
            CreateOtherPlayerHealthUI();

        otherPlayerHealthUI.SetActive(true);

        // Update hearts
        if (otherHeartImages != null)
        {
            for (int i = 0; i < otherHeartImages.Length; i++)
                otherHeartImages[i].color = (i < other.currentHealth)
                    ? new Color(0.9f, 0.5f, 0.5f) : heartEmpty;
        }

        // Position below own hearts
        otherPlayerHealthUI.transform.position = playerCam.transform.position
            + playerCam.transform.forward * 1.2f
            + playerCam.transform.up * 0.5f
            + playerCam.transform.right * 0.45f;
        otherPlayerHealthUI.transform.rotation = Quaternion.LookRotation(
            otherPlayerHealthUI.transform.position - playerCam.transform.position);
    }

    void CreateOtherPlayerHealthUI()
    {
        otherPlayerHealthUI = new GameObject("OtherPlayerHealth");
        otherPlayerHealthUI.transform.SetParent(transform, false);

        Canvas canvas = otherPlayerHealthUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 149;

        RectTransform cRect = otherPlayerHealthUI.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(200, 40);
        cRect.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(otherPlayerHealthUI.transform, false);
        var labelText = label.AddComponent<TMPro.TextMeshProUGUI>();
        labelText.text = "P2";
        labelText.fontSize = 16;
        labelText.color = Color.white;
        labelText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        var lRect = label.GetComponent<RectTransform>();
        lRect.anchoredPosition = new Vector2(-80, 0);
        lRect.sizeDelta = new Vector2(40, 30);

        // Hearts
        GameObject container = new GameObject("Hearts");
        container.transform.SetParent(otherPlayerHealthUI.transform, false);
        var containerRect = container.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(120, 30);
        containerRect.anchoredPosition = new Vector2(20, 0);
        var layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        otherHeartImages = new Image[maxHealth];
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = new GameObject("Heart_" + i);
            heart.transform.SetParent(container.transform, false);
            otherHeartImages[i] = heart.AddComponent<Image>();
            otherHeartImages[i].color = new Color(0.9f, 0.5f, 0.5f);
            heart.GetComponent<RectTransform>().sizeDelta = new Vector2(25, 25);
        }
    }

    public void TakeDamage()
    {
        if (isDead) return;
        if (invincibleTimer > 0) return;
        if (currentHealth <= 0) return;

        currentHealth--;
        invincibleTimer = invincibleDuration;

        HapticFeedback.Vibrate(300, 255);

        if (damageFlash != null)
            damageFlash.color = new Color(1f, 0f, 0f, 0.5f);

        // Update heart system
        if (playerStats != null)
            playerStats.TakeDamage(1);

        Debug.Log("Player hit! Health: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        HapticFeedback.Vibrate(500, 200);

        // Full red screen
        if (damageFlash != null)
            damageFlash.color = new Color(0.7f, 0, 0, 0.85f);

        // Freeze everything on this player
        foreach (var mb in GetComponents<MonoBehaviour>())
        {
            if (mb != this) mb.enabled = false;
        }
        foreach (var mb in GetComponentsInChildren<MonoBehaviour>())
        {
            if (mb != this) mb.enabled = false;
        }

        // Disable controller
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        ShowGameOverUI();
        // Only affect this player, not the whole game
        // Other player can continue playing

        StartCoroutine(WaitForRestart());
    }

    System.Collections.IEnumerator WaitForRestart()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            bool restart = false;
            if (ControllerMapping.Instance != null)
                restart = ControllerMapping.Instance.GetInteractDown();
            if (Input.GetKeyDown(KeyCode.E)) restart = true;

            if (restart)
            {
                DoRestart();
                yield break;
            }
            yield return null;
        }
    }

    void DoRestart()
    {
        // Shutdown Photon runner first to avoid network errors
        var runners = FindObjectsByType<Fusion.NetworkRunner>(FindObjectsSortMode.None);
        foreach (var r in runners)
        {
            if (r != null)
            {
                r.Shutdown();
            }
        }

        // Delay scene load slightly to let Photon clean up
        Invoke("LoadScene", 0.3f);
    }

    void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsInvincible()
    {
        return invincibleTimer > 0;
    }

    void SetupHeartSystem()
    {
        // Try to use HealthHeartSystem asset
        GameObject prefab = Resources.Load<GameObject>("HeartContainer");

        // Try loading from asset path
        if (prefab == null)
        {
            var allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var p in allPrefabs)
            {
                if (p.name == "HeartContainer")
                {
                    prefab = p;
                    break;
                }
            }
        }

        // Create PlayerStats
        playerStats = gameObject.GetComponent<PlayerStats>();
        if (playerStats == null)
            playerStats = gameObject.AddComponent<PlayerStats>();

        // Set health values via serialized fields
        var so = new System.Reflection.FieldInfo[] { };
        try
        {
            var healthField = typeof(PlayerStats).GetField("health",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxHealthField = typeof(PlayerStats).GetField("maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxTotalField = typeof(PlayerStats).GetField("maxTotalHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (healthField != null) healthField.SetValue(playerStats, (float)maxHealth);
            if (maxHealthField != null) maxHealthField.SetValue(playerStats, (float)maxHealth);
            if (maxTotalField != null) maxTotalField.SetValue(playerStats, (float)maxHealth);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not set PlayerStats fields: " + e.Message);
        }

        // Create HUD Canvas
        GameObject hudCanvas = new GameObject("HeartHUD");
        hudCanvas.transform.SetParent(transform, false);
        Canvas canvas = hudCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 160;

        RectTransform cRect = hudCanvas.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(200, 60);
        cRect.localScale = new Vector3(0.003f, 0.003f, 0.003f);

        // Hearts parent
        GameObject heartsParent = new GameObject("HeartsParent");
        heartsParent.transform.SetParent(hudCanvas.transform, false);
        var hpRect = heartsParent.AddComponent<RectTransform>();
        hpRect.sizeDelta = new Vector2(200, 60);
        var layout = heartsParent.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        if (prefab != null)
        {
            // Use HeartContainer prefab from asset
            var controller = hudCanvas.AddComponent<HealthBarController>();
            controller.heartsParent = heartsParent.transform;
            controller.heartContainerPrefab = prefab;
            Debug.Log("Heart system using HeartContainer prefab");
        }
        else
        {
            // Fallback: create simple red squares
            for (int i = 0; i < maxHealth; i++)
            {
                GameObject heart = new GameObject("Heart_" + i);
                heart.transform.SetParent(heartsParent.transform, false);
                var img = heart.AddComponent<Image>();
                img.color = new Color(0.9f, 0.1f, 0.1f);
                var r = heart.GetComponent<RectTransform>();
                r.sizeDelta = new Vector2(40, 40);
            }
            Debug.Log("Heart system using fallback red squares");
        }

        // Start updating position
        StartCoroutine(UpdateHeartPosition(hudCanvas));
    }

    System.Collections.IEnumerator UpdateHeartPosition(GameObject hud)
    {
        while (true)
        {
            if (hud != null && playerCam != null)
            {
                hud.transform.position = playerCam.transform.position
                    + playerCam.transform.forward * 1.2f
                    + playerCam.transform.up * 0.6f
                    + playerCam.transform.right * 0.45f;
                hud.transform.rotation = Quaternion.LookRotation(
                    hud.transform.position - playerCam.transform.position);
            }
            yield return null;
        }
    }

    void ShowGameOverUI()
    {
        if (gameOverUI != null) return;

        gameOverUI = new GameObject("GameOverUI");
        gameOverUI.transform.SetParent(transform, false);
        gameOverUI.transform.localPosition = new Vector3(0, 3f, 0); // Above player head
        Canvas c = gameOverUI.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingOrder = 250;

        RectTransform cRect = gameOverUI.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(500, 200);
        cRect.localScale = new Vector3(0.003f, 0.003f, 0.003f);

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(gameOverUI.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.6f, 0, 0, 0.85f);
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 200);

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(bg.transform, false);
        var text = txt.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "GAME OVER\n\nPress [A] to restart";
        text.fontSize = 36;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = Color.white;
        var tRect = txt.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;
    }

    GameObject overheadBar;
    GameObject[] overheadSpheres;

    void CreateOverheadHealthBar()
    {
        overheadBar = new GameObject("OverheadHP");
        overheadBar.transform.SetParent(transform, false);
        overheadBar.transform.localPosition = new Vector3(0, 2.2f, 0);

        // 3 red spheres as hearts — no Canvas needed, always visible
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        redMat.EnableKeyword("_EMISSION");
        redMat.SetColor("_EmissionColor", Color.red * 0.5f);

        Material grayMat = new Material(Shader.Find("Standard"));
        grayMat.color = new Color(0.3f, 0.3f, 0.3f);

        overheadSpheres = new GameObject[maxHealth];
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Heart3D_" + i;
            sphere.transform.SetParent(overheadBar.transform, false);
            sphere.transform.localPosition = new Vector3((i - 1) * 0.35f, 0, 0);
            sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            // Remove collider
            var col = sphere.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            sphere.GetComponent<MeshRenderer>().material = redMat;
            overheadSpheres[i] = sphere;
        }
    }

    void UpdateOverheadBar()
    {
        if (overheadBar == null || overheadSpheres == null) return;

        // Face camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            overheadBar.transform.rotation = Quaternion.LookRotation(
                overheadBar.transform.position - cam.transform.position);
        }

        // Update sphere colors
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        redMat.EnableKeyword("_EMISSION");
        redMat.SetColor("_EmissionColor", Color.red * 0.5f);

        Material grayMat = new Material(Shader.Find("Standard"));
        grayMat.color = new Color(0.3f, 0.3f, 0.3f);

        for (int i = 0; i < overheadSpheres.Length; i++)
        {
            if (overheadSpheres[i] != null)
                overheadSpheres[i].GetComponent<MeshRenderer>().material =
                    (i < currentHealth) ? redMat : grayMat;
        }
    }

    void CreateDamageFlash()
    {
        GameObject flashCanvas = new GameObject("DamageFlashCanvas");
        flashCanvas.transform.SetParent(transform, false);
        Canvas c = flashCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 200;

        GameObject flashObj = new GameObject("Flash");
        flashObj.transform.SetParent(flashCanvas.transform, false);
        damageFlash = flashObj.AddComponent<Image>();
        damageFlash.color = new Color(1, 0, 0, 0);
        var rect = flashObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }
}
