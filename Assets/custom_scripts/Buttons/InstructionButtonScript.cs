using UnityEditor;
using UnityEngine;

#nullable enable

public class InstructionButtonScript : MonoBehaviour, IButton
{
    public GameObject control_menu_panel;
    public Canvas setting_menu_canva;
    private  CharacterMovement char_move; 

    private void Start()
    {
        char_move = transform.root.GetComponentInChildren<CharacterMovement>();

        if (char_move == null)
        {
            Debug.Log("Unable to get char_move for Resume button script");
        }
    }
    public void Execute()
    { 
        // Show instruction menu and resume character move

        control_menu_panel.SetActive(true);
        setting_menu_canva.enabled = false;
        char_move.enabled = true;

        ControlsMenu menu = control_menu_panel.transform.parent.GetComponent<ControlsMenu>();
        menu.isVisible = true;
    }

    public void setHover(bool isHovering)
    {  // No need to implement anything for this button
        
    }
}



