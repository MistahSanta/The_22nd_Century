using UnityEngine;

#nullable enable

public class InstructionButtonScript : MonoBehaviour, IButton
{
    public GameObject control_menu_panel;
    public GameObject setting_menu;
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
        setting_menu.SetActive(false);
        char_move.enabled = true;

    }

    public void setHover(bool isHovering)
    {  // No need to implement anything for this button
        
    }
}



