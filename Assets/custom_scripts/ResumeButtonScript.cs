using UnityEngine;

#nullable enable

public class ResumeButtonScript : MonoBehaviour, IButton
{
    public Canvas setting_menu_panel;
    public void Execute()
    { 
        // Grab the topmost and recursively grab the needed character controller script
        //SettingManagerScript script = GetComponentInParent<SettingManagerScript>(true);
        CharacterMovement char_move = transform.root.GetComponentInChildren<CharacterMovement>();
        if (char_move != null )
        {
            setting_menu_panel.enabled = false;
            char_move.enabled = true;
        }
        else
        {
            Debug.Log("unable to find char_move for resume button!");
        }
    }

    public void setHover(bool isHovering)
    {  // No need to implement anything for this button
        
    }
}