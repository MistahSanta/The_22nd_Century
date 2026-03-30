using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

#nullable enable
public class InteractableObjectScript : MonoBehaviour
{
    public bool pointer_on_obj = false; // Track if pointer is touching the obj. 
    public UnityEvent? runButtonClickFunction; 
    public void PointerEnter()
    {
        pointer_on_obj = true;
    }

    public void OnPointerExit()
    {
        pointer_on_obj = false;
    }
    
    void Update()
    {
        if (pointer_on_obj == false) return;
        // Interact button is click so run the button function if there is one 
        if (Input.GetButtonDown("js9") || Input.GetButtonDown("js4") || Input.GetButtonDown("js10"))
        { // 'a' button on joystick or 'e' button keyboard

            if (runButtonClickFunction == null) return;

            runButtonClickFunction.Invoke();
        }
    }
}