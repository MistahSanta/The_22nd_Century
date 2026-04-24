using UnityEngine;

#nullable enable

public class QuitButtonScript : MonoBehaviour, IButton
{

    public void Execute()
    { 
        Application.Quit();
    }

    public void setHover(bool isHovering)
    {  // No need to implement anything for this button
        
    }
}