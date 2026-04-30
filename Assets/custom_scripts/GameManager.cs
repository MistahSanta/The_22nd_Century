using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("World Roots")]
    public GameObject futureApocalypse;
    public GameObject futureCleaner;
    public GameObject futureGettingCleaner;
    public GameObject futureVeryClean;
    public GameObject presentWorld;

    [Header("Skyboxes")]
    public Material skyboxApocalypse;
    public Material skyboxCleaner;
    public Material skyboxGettingCleaner;
    public Material skyboxVeryClean;
    public Material skyboxPresent;


    [Header("Timer")]
    public float presentWorldTime = 60f;

    [Header("Shared Objects")]
    public GameObject gunObject;
    public GameObject timeMachineObject;

    [Header("Cleanliness")]
    public int totalTrashInPresent = 5;
    public int totalTreesInPresent = 3;
    bool isSpawned = false;

    public bool IsReady => isSpawned && Runner != null;

    // ─── Networked State ───────────────────────────────────────────────────────
    // WorldState: 0=future-apocalypse, 1=future-cleaner, 2=future-gettingcleaner,
    //             3=future-veryclean,  4=present
    [Networked, OnChangedRender(nameof(OnWorldStateChanged))]
    public int NetworkedWorldState { get; set; } = 0;

    [Networked, OnChangedRender(nameof(OnCleanlinessChanged))]
    public int NetworkedTrashCollected { get; set; } = 0;

    [Networked, OnChangedRender(nameof(OnCleanlinessChanged))]
    public int NetworkedTreesPlanted { get; set; } = 0;

    [Networked, OnChangedRender(nameof(OnTimerChanged))]
    public float NetworkedTimer { get; set; } = 0f;

    [Networked, OnChangedRender(nameof(OnTimerChanged))]
    public bool NetworkedTimerRunning { get; set; } = false;

    [Networked, OnChangedRender(nameof(OnGameOverChanged))]
    public bool NetworkedGameOver { get; set; } = false;

    [Networked, OnChangedRender(nameof(OnTimeMachineMoved))]
    public Vector3 NetworkedTimeMachinePos { get; set; }

    [Networked, OnChangedRender(nameof(OnTimeMachineMoved))]
    public Vector3 NetworkedTimeMachineRot { get; set; }

    // ─── Local State ──────────────────────────────────────────────────────────
    bool isInPresent = false;
    bool hasGarbagePicker = false;
    bool hasShovel = false;
    GameObject gameOverPanel;
    int lastTimeMachineIdx = -1;

    // Public read-only accessors (read from networked state)
    public bool IsInPresent => isInPresent;
    public bool IsGameOver => NetworkedGameOver;
    public bool TimerRunning => NetworkedTimerRunning;
    public float TimeRemaining => NetworkedTimer;
    public bool HasGarbagePicker => hasGarbagePicker;
    public bool HasShovel => hasShovel;
    public int TrashCollected => NetworkedTrashCollected;
    public int TreesPlanted => NetworkedTreesPlanted;
    public float CleanlinessPercent =>
        (totalTrashInPresent + totalTreesInPresent) > 0
            ? (float)(NetworkedTrashCollected + NetworkedTreesPlanted)
              / (totalTrashInPresent + totalTreesInPresent) * 100f
            : 0f;

    public enum EquippedTool { None, GarbagePicker, Shovel }
    public EquippedTool CurrentTool { get; private set; } = EquippedTool.None;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void Spawned()
    {
        isSpawned = true;

        if (gunObject == null) gunObject = GameObject.Find("Gun");
        if (timeMachineObject == null) timeMachineObject = GameObject.Find("TimeMachine");

        CreateGameOverUI();

        // New clients joining mid-session: apply current networked state locally
        ApplyWorldState(NetworkedWorldState);

        if (NetworkedGameOver && gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (HasStateAuthority)
        {
            NetworkedTimeMachinePos = new Vector3(1.57f, 0.18f, -2.50f);
            NetworkedTimeMachineRot = new Vector3(-90f, 0f, -160f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Only the State Authority (host) ticks the timer
        if (!isSpawned) return;
        if (!HasStateAuthority) return;

        if (NetworkedTimerRunning && isInPresent)
        {
            NetworkedTimer -= Runner.DeltaTime;
            if (NetworkedTimer <= 0f)
            {
                NetworkedTimer = 0f;
                NetworkedTimerRunning = false;
                Debug.Log("Time's up! Return to the Time Machine!");
            }
        }
    }

    void Update()
    {
        if (!isSpawned) return;

        // Tool switching is purely local
        if (isInPresent && ControllerMapping.Instance != null
            && ControllerMapping.Instance.GetSwitchToolDown())
        {
            SwitchTool();
            Debug.Log($"Switched to: {CurrentTool}");
        }

        // Restart input (local)
        if (NetworkedGameOver)
        {
            bool restart = ControllerMapping.Instance != null
                ? ControllerMapping.Instance.GetInteractDown()
                : Input.GetKeyDown(KeyCode.E);
            if (restart) RestartGame();
        }
    }

    // ─── Public Actions (called locally, authority changes networked state) ───

    public void CollectTrash()
    {
        if (!IsReady) return;
        if (!HasStateAuthority) { RPC_CollectTrash(); return; }
        NetworkedTrashCollected++;
        Debug.Log($"Trash: {NetworkedTrashCollected}/{totalTrashInPresent} ({CleanlinessPercent:F0}%)");
    }

    public void PlantTree()
    {
        if (!IsReady) return;
        if (!HasStateAuthority) { RPC_PlantTree(); return; }
        NetworkedTreesPlanted++;
        Debug.Log($"Trees planted: {NetworkedTreesPlanted}");
    }

    public void ActivateTimeMachine()
    {
        if (!IsReady) return;
        if (NetworkedGameOver) return;
        if (!HasStateAuthority) { RPC_ActivateTimeMachine(); return; }

        if (isInPresent)
            SetFutureWorldState();
        else
            SetPresentWorldState();
    }

    public void GameOver()
    {
        if (NetworkedGameOver) return;
        if (!HasStateAuthority) { RPC_TriggerGameOver(); return; }
        NetworkedGameOver = true;
        HapticFeedback.VibrateHit();
    }

    public void PickUpGarbagePicker() { hasGarbagePicker = true; Debug.Log("Garbage picker equipped!"); }
    public void PickUpShovel() { hasShovel = true; Debug.Log("Shovel equipped!"); }

    public void EquipGarbagePicker() { CurrentTool = EquippedTool.GarbagePicker; }
    public void EquipShovel() { CurrentTool = EquippedTool.Shovel; }
    public void SwitchTool()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayToolSwitch();
        if (CurrentTool == EquippedTool.GarbagePicker) CurrentTool = EquippedTool.Shovel;
        else if (CurrentTool == EquippedTool.Shovel) CurrentTool = EquippedTool.GarbagePicker;
    }

    // ─── RPCs (non-authority clients ask the host to do things) ──────────────

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    void RPC_CollectTrash() => NetworkedTrashCollected++;

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    void RPC_PlantTree() => NetworkedTreesPlanted++;

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    void RPC_ActivateTimeMachine() => ActivateTimeMachine();

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    void RPC_TriggerGameOver() => GameOver();

    // ─── State Authority: Set Networked State ─────────────────────────────────

    void SetPresentWorldState()
    {
        NetworkedWorldState = 4;
        NetworkedTimer = presentWorldTime;
        NetworkedTimerRunning = true;

        foreach (var zombie in FindObjectsOfType<ZombieScript>())
        {
            if (zombie.GetComponent<NetworkObject>() != null)
                Runner.Despawn(zombie.GetComponent<NetworkObject>());
        }
    }

    void SetFutureWorldState()
    {
        var spawner = FindObjectOfType<ZombieSpawner>();
        foreach (var zombie in FindObjectsOfType<ZombieScript>())
        {
            if (zombie.GetComponent<NetworkObject>() != null)
                Runner.Despawn(zombie.GetComponent<NetworkObject>());
        }
        if (spawner != null) spawner.ResetZombieCount();

        float pct = CleanlinessPercent;
        if (pct >= 100f) NetworkedWorldState = 3;
        else if (pct >= 60f) NetworkedWorldState = 2;
        else if (pct >= 30f) NetworkedWorldState = 1;
        else NetworkedWorldState = 0;

        NetworkedTimerRunning = false;

        // Pick a random safe position and sync it
        Vector3[] safePositions =
        {
            new Vector3(0.36f, 0.18f, -3.03f),
            new Vector3(0.99f, 0.18f, 18.19f),
            new Vector3(17.68f, 0.18f, 19.77f),
            new Vector3(21.73f, 0.18f, -5.92f),
            new Vector3(-20.62f, 0.18f, 20.72f),
            new Vector3(-20.49f, 0.18f, -20.62f),
            new Vector3(-2.76f, 0.18f, -20.30f),
        };

        Vector3[] safeRotations =
        {
            new Vector3(-90f, 0f, -160f),
            new Vector3(-90f, 0f, -160f),
            new Vector3(-90f, 0f, -160f),
            new Vector3(-90f, 0f, -160f),
            new Vector3(-90f, 0f, -160f),
            new Vector3(-90f, 0f, -160f),
            new Vector3(-90f, 0f, -160f),
        };

        int idx;
        do
        {
            idx = Random.Range(0, safePositions.Length);
        } while (idx == lastTimeMachineIdx);
        lastTimeMachineIdx = idx;

        NetworkedTimeMachinePos = safePositions[idx];
        NetworkedTimeMachineRot = safeRotations[idx];
    }

    // ─── OnChangedRender Callbacks (fire on ALL clients when state changes) ───

    void OnWorldStateChanged() => ApplyWorldState(NetworkedWorldState);
    void OnCleanlinessChanged() { /* UI update hook - add HUD refresh here */ }
    void OnTimerChanged() { /* UI update hook - timer display refresh here */ }
    void OnTimeMachineMoved()
    {
        if (timeMachineObject != null)
        {
            timeMachineObject.transform.position = NetworkedTimeMachinePos;
            timeMachineObject.transform.eulerAngles = NetworkedTimeMachineRot;
        }
    }
    void OnGameOverChanged()
    {
        if (NetworkedGameOver)
        {
            HapticFeedback.VibrateHit();
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
    }

    // ─── Local Visual Application (called on every client) ───────────────────

    void ApplyWorldState(int state)
    {
        SetAllWorldsInactive();

        if (state == 4)
        {
            isInPresent = true;
            if (presentWorld != null) presentWorld.SetActive(true);
            if (skyboxPresent != null) RenderSettings.skybox = skyboxPresent;
            DynamicGI.UpdateEnvironment();
            if (gunObject != null) gunObject.SetActive(false);

            Transform zombies = presentWorld?.transform.Find("Zombies");
            if (zombies != null) zombies.gameObject.SetActive(false);
        }
        else
        {
            isInPresent = false;
            if (gunObject != null) gunObject.SetActive(true);

            switch (state)
            {
                case 0:
                    if (futureApocalypse != null) futureApocalypse.SetActive(true);
                    if (skyboxApocalypse != null) RenderSettings.skybox = skyboxApocalypse;
                    DynamicGI.UpdateEnvironment();
                    break;
                case 1:
                    if (futureCleaner != null) futureCleaner.SetActive(true);
                    if (skyboxCleaner != null) RenderSettings.skybox = skyboxCleaner;
                    DynamicGI.UpdateEnvironment();
                    break;
                case 2:
                    if (futureGettingCleaner != null) futureGettingCleaner.SetActive(true);
                    if (skyboxGettingCleaner != null) RenderSettings.skybox = skyboxGettingCleaner;
                    DynamicGI.UpdateEnvironment();
                    break;
                case 3:
                    if (futureVeryClean != null) futureVeryClean.SetActive(true);
                    if (skyboxVeryClean != null) RenderSettings.skybox = skyboxVeryClean;
                    DynamicGI.UpdateEnvironment();
                    break;
            }

            // Apply synced time machine position & reset glow
            if (timeMachineObject != null)
            {
                timeMachineObject.transform.position = NetworkedTimeMachinePos;
                var tm = timeMachineObject.GetComponent<TimeMachineScript>();
                if (tm != null) tm.ResetGlow();
            }
        }
    }

    void SetAllWorldsInactive()
    {
        if (futureApocalypse != null) futureApocalypse.SetActive(false);
        if (futureCleaner != null) futureCleaner.SetActive(false);
        if (futureGettingCleaner != null) futureGettingCleaner.SetActive(false);
        if (futureVeryClean != null) futureVeryClean.SetActive(false);
        if (presentWorld != null) presentWorld.SetActive(false);
    }

    void RestartGame()
    {
        if (HasStateAuthority)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // ─── Game Over UI (local, each client has their own) ─────────────────────

    void CreateGameOverUI()
    {
        GameObject canvas = new GameObject("GameOverCanvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 200;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);
        var bg = gameOverPanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.5f, 0, 0, 0.8f);
        var bgRect = gameOverPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(gameOverPanel.transform, false);
        var text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "GAME OVER\n\nThe zombie got you!\n\nPress [A] to restart";
        text.fontSize = 36;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = Color.white;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        gameOverPanel.SetActive(false);
    }
}