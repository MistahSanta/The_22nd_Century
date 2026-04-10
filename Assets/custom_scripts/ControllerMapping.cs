using UnityEngine;

/// <summary>
/// Controller button mappings based on actual hardware testing.
/// X=js3, Y=js5, A=js11, B=js10, Top=H(js7), Menu=js9/js13
/// </summary>
public class ControllerMapping : MonoBehaviour
{
    public static ControllerMapping Instance { get; private set; }

    [Header("Shoot (Top Button)")]
    public string shootButton1 = "js7";  // keyboard H / top trigger

    [Header("Interact (A Button)")]
    public string interactButton1 = "js11"; // controller A
    public string interactButton2 = "js4";  // keyboard E fallback

    [Header("Menu (Menu Button)")]
    public string menuButton1 = "js9";   // menu toggle 1
    public string menuButton2 = "js13";  // menu toggle 2

    [Header("Jump (Y Button)")]
    public string jumpButton = "js5";    // controller Y

    [Header("Switch Tool (B Button)")]
    public string switchButton = "js10";      // controller B

    [Header("Other")]
    public string buttonX = "js3";       // controller X

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public bool GetShootDown()
    {
        return SafeGetButtonDown(shootButton1) || Input.GetKeyDown(KeyCode.H);
    }

    public bool GetInteractDown()
    {
        return SafeGetButtonDown(interactButton1) || SafeGetButtonDown(interactButton2)
            || Input.GetKeyDown(KeyCode.E);
    }

    public bool GetMenuDown()
    {
        return SafeGetButtonDown(menuButton1) || SafeGetButtonDown(menuButton2)
            || Input.GetKeyDown(KeyCode.M);
    }

    public bool GetJumpDown()
    {
        return SafeGetButtonDown(jumpButton) || Input.GetKeyDown(KeyCode.Space);
    }

    public bool GetSwitchToolDown()
    {
        return SafeGetButtonDown(switchButton) || Input.GetKeyDown(KeyCode.T);
    }

    bool SafeGetButtonDown(string btn)
    {
        if (string.IsNullOrEmpty(btn)) return false;
        try { return Input.GetButtonDown(btn); }
        catch { return false; }
    }
}
