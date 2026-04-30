using UnityEngine;
using TMPro;
using Fusion;

/// <summary>
/// Floating controls help menu that toggles with the menu button.
/// Shows button mappings for all interactions.
/// Attach to a World Space Canvas.
/// </summary>
public class ControlsMenu : NetworkBehaviour
{
    [Header("UI")]
    public GameObject menuPanel;
    public TextMeshProUGUI controlsText;

    [Header("Follow Settings")]
    Transform playerCamera;
    public float followDistance = 3f;

    bool _isVisible = false;
    public bool isVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            if (value)
            {
                currentPage = 0;
                if (controlsText != null && pages != null)
                    controlsText.text = pages[0];
            }
        }
    }
    public GameObject setting_menu;

    int currentPage = 0;
    string[] pages;


    public override void Spawned()
    {
        // Only setup for local player
        if (!HasInputAuthority)
        {
            // Disable entirely for other players
            menuPanel?.SetActive(false);
            enabled = false;
            return;
        }

        // Player spawn in, so find the control menu associated with this player 
        playerCamera = transform.root.GetComponentInChildren<Camera>().transform;
        if (playerCamera == null)
        {
            Debug.LogError("Control menu cannot find player camera!");
            return;
        }
    }
    void Start()
    {
        if (controlsText != null)
        {
            controlsText.alignment = TMPro.TextAlignmentOptions.Center;
        }

        pages = new string[]
        {
            "<b>Welcome to The 22nd Century!</b>\n\n" +
            "<b>🎯 Goal</b>\n" +
            "Collect trash & plant trees in the Present World\n" +
            "to clean up the apocalyptic Future!\n" +
            "Keep traveling between worlds until you reach\n" +
            "the Very Clean Future!\n\n" +
            "<b>🎮 Controls</b>\n" +
            "Joystick - Move\n" +
            "Head Movement - Look / Aim\n" +
            "Top Button - Shoot\n" +
            "A Button - Interact\n" +
            "X Button - Switch Tool\n" +
            "Menu Button - Toggle This Menu\n\n" +
            "<b>Press [A] for Next Page</b>",

            "<b>🔫 Future World</b>\n" +
            "Gun is on the floor right below where you start\n" +
            "Shoot zombies and find the glowing Time Machine\n" +
            "(Time Machine spawns at a random location!)\n" +
            "Zombies spawn around you - watch your back!\n\n" +
            "<b>🌍 Present World (60 seconds!)</b>\n" +
            "Garbage Picker & Shovel are on the road\n" +
            "Garbage Picker → Look at trash + A to collect\n" +
            "Shovel → Look at glowing spots + A to plant trees\n" +
            "Press X to switch between tools\n" +
            "Collect as much as possible before time runs out!\n" +
            "Travel back to see how much the future improved!\n" +
            "Tip: Press Menu button anytime to view these instructions again!\n\n" +
            "<b>Press [A] to Close</b>"
        };

        if (controlsText != null) controlsText.text = pages[0];

        isVisible = true;
        if (menuPanel != null) menuPanel.SetActive(true);
        if (setting_menu != null)
        {
            SettingManagerScript script = setting_menu.GetComponent<SettingManagerScript>();
            if (script != null) script.menu_is_open = true;
        }

        if (playerCamera == null)
        {
            var cam = Camera.main;
            if (cam != null) playerCamera = cam.transform;
        }
    }

    void Update()
    {
        // Position in front of player when visible
        if (isVisible && playerCamera != null)
        {
            Vector3 targetPos = playerCamera.position
                + playerCamera.forward * 2f
                + Vector3.up * 0.15f;
            transform.position = targetPos;
            transform.rotation = Quaternion.LookRotation(transform.position - playerCamera.position);

            bool a_button = ControllerMapping.Instance != null
                ? ControllerMapping.Instance.GetInteractDown()
                : Input.GetKeyDown(KeyCode.E);
            if (a_button)
            {
                if (currentPage < pages.Length - 1)
                {
                    currentPage++;
                    controlsText.text = pages[currentPage];
                }
                else
                {
                    isVisible = false;
                    menuPanel.SetActive(false);
                    if (setting_menu != null)
                    {
                        SettingManagerScript script = setting_menu.GetComponent<SettingManagerScript>();
                        if (script != null) script.menu_is_open = false;
                    }
                    if (GameManager.Instance != null) GameManager.Instance.StartGame();
                }
            }
        }
    }
}
