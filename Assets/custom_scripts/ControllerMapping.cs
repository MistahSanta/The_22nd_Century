using UnityEngine;

/// <summary>
/// Controller button mappings based on actual hardware testing.
/// X=js2, Y=js3, A=js10, B=js5, Top=H(js0), Menu=js9/js13/js11
/// </summary>
public class ControllerMapping : MonoBehaviour
{
    public static ControllerMapping Instance { get; private set; }

    [Header("Shoot (Top Button)")]
    public string shootButton1 = "js0";  // keyboard H / top trigger

    [Header("Interact (A Button)")]
    public string interactButton1 = "js10"; // controller A
    public string interactButton2 = "js4";  // keyboard E fallback

    [Header("Menu (Menu Button)")]
    public string menuButton1 = "js9";   // menu toggle 1
    public string menuButton3 = "js11";
    public string menuButton2 = "js13";  // menu toggle 2

    [Header("jump button")]
    public string jumpButton = "js3";  

    [Header("Switch Tool (X Button)")]
    public string switchButton = "js2";      // controller X

    [Header("Other")]
    public string buttonB = "js5";       // controller B

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
        return SafeGetButtonDown(menuButton1) || SafeGetButtonDown(menuButton2) || SafeGetButtonDown(menuButton3)
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
