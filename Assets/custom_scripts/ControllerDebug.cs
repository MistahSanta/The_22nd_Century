using UnityEngine;

/// <summary>
/// Debug tool - disabled by default. Enable in Inspector to see button presses.
/// </summary>
public class ControllerDebug : MonoBehaviour
{
    public bool showDebug = false;
    string lastButton = "";
    float showTimer = 0;

    void Update()
    {
        if (!showDebug) return;
        for (int i = 0; i < 20; i++)
        {
            string btnName = "js" + i;
            try { if (Input.GetButtonDown(btnName)) { lastButton = btnName; showTimer = 3f; } } catch { }
        }
        if (showTimer > 0) showTimer -= Time.deltaTime;
    }

    void OnGUI()
    {
        if (!showDebug || showTimer <= 0) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 30;
        style.normal.textColor = Color.yellow;
        GUI.Label(new Rect(20, 20, 400, 40), "Button: " + lastButton, style);
    }
}
