using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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
    float timer = 0f;
    bool timerRunning = false;
    public bool TimerRunning => timerRunning;
    public float TimeRemaining => timer;

    [Header("Shared Objects (hidden in Present)")]
    public GameObject gunObject;
    public GameObject timeMachineObject;

    [Header("Cleanliness")]
    public int totalTrashInPresent = 5; // Only need 5 out of 45 to reach 100%
    public int totalTreesInPresent = 3;
    int trashCollected = 0;
    int treesPlanted = 0;

    bool isInPresent = false;
    bool isGameOver = false;
    bool hasGarbagePicker = false;
    bool hasShovel = false;
    GameObject gameOverPanel;

    public bool HasGarbagePicker => hasGarbagePicker;
    public void PickUpGarbagePicker() { hasGarbagePicker = true; Debug.Log("Garbage picker equipped!"); }

    public bool HasShovel => hasShovel;
    public void PickUpShovel() { hasShovel = true; Debug.Log("Shovel equipped!"); }

    public enum EquippedTool { None, GarbagePicker, Shovel }
    public EquippedTool CurrentTool { get; private set; } = EquippedTool.None;

    public bool IsInPresent => isInPresent;
    public bool IsGameOver => isGameOver;
    public float CleanlinessPercent => (totalTrashInPresent + totalTreesInPresent) > 0 ? (float)(trashCollected + treesPlanted) / (totalTrashInPresent + totalTreesInPresent) * 100f : 0f;
    public int TrashCollected => trashCollected;
    public int TreesPlanted => treesPlanted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-find gun and TM if not assigned
        if (gunObject == null) gunObject = GameObject.Find("Gun");
        if (timeMachineObject == null) timeMachineObject = GameObject.Find("TimeMachine");

        ShowFutureWorld();
        CreateGameOverUI();
    }

    void Update()
    {
        if (timerRunning && isInPresent)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                timerRunning = false;
                // disable interactions
                Debug.Log("Time's up! Return to the Time Machine!");
            }
        }

        if (isInPresent && ControllerMapping.Instance != null && ControllerMapping.Instance.GetSwitchToolDown())
        {
            SwitchTool();
            Debug.Log($"Switched to: {CurrentTool}");
        }

        if (isGameOver)
        {
            bool restart = ControllerMapping.Instance != null
                ? ControllerMapping.Instance.GetInteractDown()
                : Input.GetKeyDown(KeyCode.E);
            if (restart) RestartGame();
        }
    }

    public void CollectTrash()
    {
        trashCollected++;
        Debug.Log($"Trash collected: {trashCollected}/{totalTrashInPresent} ({CleanlinessPercent:F0}%)");

        // If all trash collected, enable TimeMachine glow
        if (CleanlinessPercent >= 100f)
            Debug.Log("All trash collected! Time Machine is ready!");
    }

    public void PlantTree()
    {
        treesPlanted++;
        Debug.Log($"Trees planted: {treesPlanted}");
    }

    public void ActivateTimeMachine()
    {
        if (isGameOver) return;

        if (isInPresent)
        {
            ShowFutureWorld();
        }
        else
        {
            ShowPresentWorld();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        HapticFeedback.VibrateHit();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    void RestartGame()
    {
        isGameOver = false;
        trashCollected = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ShowPresentWorld()
    {
        isInPresent = true;

        timer = presentWorldTime;
        timerRunning = true;

        SetAllWorldsInactive();
        if (presentWorld != null)
        {
            presentWorld.SetActive(true);
            Debug.Log($"Present world activated. Children: {presentWorld.transform.childCount}");
            foreach (Transform child in presentWorld.transform)
                Debug.Log($"  Child: {child.name} active={child.gameObject.activeSelf}");
        }
        if (skyboxPresent != null) RenderSettings.skybox = skyboxPresent;

        // Hide gun in present
        if (gunObject != null) gunObject.SetActive(false);

        // Hide zombies in present world
        if (presentWorld != null)
        {
            Transform zombies = presentWorld.transform.Find("Zombies");
            if (zombies != null) zombies.gameObject.SetActive(false);
        }
    }

    void ShowFutureWorld()
    {
        isInPresent = false;
        SetAllWorldsInactive();

        // Show gun in future
        if (gunObject != null) gunObject.SetActive(true);

        float pct = CleanlinessPercent;

        if (pct >= 100f && futureVeryClean != null)
        {
            futureVeryClean.SetActive(true);
            if (skyboxVeryClean != null) RenderSettings.skybox = skyboxVeryClean;
        }
        else if (pct >= 60f && futureGettingCleaner != null)
        {
            futureGettingCleaner.SetActive(true);
            if (skyboxGettingCleaner != null) RenderSettings.skybox = skyboxGettingCleaner;
        }
        else if (pct >= 30f && futureCleaner != null)
        {
            futureCleaner.SetActive(true);
            if (skyboxCleaner != null) RenderSettings.skybox = skyboxCleaner;
        }
        else if (futureApocalypse != null)
        {
            futureApocalypse.SetActive(true);
            if (skyboxApocalypse != null) RenderSettings.skybox = skyboxApocalypse;
        }

        Vector3[] safePositions = new Vector3[]
        {
            new Vector3(-20f, 0f, 25f),
            new Vector3(2f, 0f, 15f),
            new Vector3(4f, 0f, -20f),
            new Vector3(-10f, 0f, -20f),
            new Vector3(3f, 0f, -3f),
            new Vector3(23f, 0f, 8f),
            new Vector3(-20f, 0f, 3f),
        };

        if (timeMachineObject != null)
        {
            int randomIndex = Random.Range(0, safePositions.Length);
            timeMachineObject.transform.position = safePositions[randomIndex];

            var tm = timeMachineObject.GetComponent<TimeMachineScript>();
            if (tm != null) tm.ResetGlow();
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

    public void EquipGarbagePicker()
    {
        CurrentTool = EquippedTool.GarbagePicker;
    }

    public void EquipShovel()
    {
        CurrentTool = EquippedTool.Shovel;
    }

    public void SwitchTool()
    {
        if (CurrentTool == EquippedTool.GarbagePicker)
            CurrentTool = EquippedTool.Shovel;
        else if (CurrentTool == EquippedTool.Shovel)
            CurrentTool = EquippedTool.GarbagePicker;
    }
}
