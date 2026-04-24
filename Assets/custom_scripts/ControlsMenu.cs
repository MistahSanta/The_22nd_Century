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
    public float followDistance = 2f;

    public bool isVisible = false;
    public GameObject setting_menu;

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
        playerCamera = transform.parent.GetComponentInChildren<Camera>().transform;
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
            controlsText.text =
                "<b>CONTROLS</b>\n\n" +
                "<b>Movement</b>\n" +
                "  Joystick Pad - Move\n" +
                "  Head Movement - Look / Aim\n\n" +
                "<b>Actions</b>\n" +
                "  Top Button (B) - Shoot Gun\n" +
                "  A Button - Interact / Pick Up\n" +
                "  Menu Button - Toggle This Menu\n\n" +
                "<b>Interactions</b>\n" +
                "  Look at object + A - Pick up gun / grabber\n" +
                "  Look at trash + A - Collect trash\n" +
                "  Look at portal + A - Time travel\n\n" +
                "<b>Physical Gestures</b>\n" +
                "  Jump (move up fast) - Jump in game\n" +
                "  Crouch (move down fast) - Crouch in game";
        }

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle menu with menu button (js11) or M key
        // bool menu = ControllerMapping.Instance != null
        //     ? ControllerMapping.Instance.GetMenuDown()
        //     : Input.GetKeyDown(KeyCode.M);
        // if (menu)
        // {
        //     isVisible = !isVisible;
        //     if (menuPanel != null)
        //         menuPanel.SetActive(isVisible);
        // }

        // Position in front of player when visible
        if (isVisible && playerCamera != null)
        {
            Vector3 targetPos = playerCamera.position
                + playerCamera.forward * followDistance
                + Vector3.up * 0.2f;
            transform.position = targetPos;
            transform.rotation = Quaternion.LookRotation(transform.position - playerCamera.position);
        
            bool menu = ControllerMapping.Instance != null
                ? ControllerMapping.Instance.GetMenuDown()
                : Input.GetKeyDown(KeyCode.M);
            if (menu)
            {
                isVisible = false;
                
                
            }
        }
    }
}
