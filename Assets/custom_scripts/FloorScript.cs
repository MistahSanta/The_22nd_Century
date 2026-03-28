
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEditor;

/*
Keymapping for Linux computer: 
    Joystick:
        a- 
        x- 
        y- 
        b- b button has no mapping in linux (i talked with TA about this )
        pointer_button- 
    Keyboard: 
        A - js9
        x - js2 
        y - js3
        b - js1 
    android: 
        a- 
        x-
        y-
        b- 
        pointer_button- 

*/


public class FloorScript : MonoBehaviour
{
    bool pointer_enter_obj = false; // Track if pointer is touching the obj.
    public GameObject player; 
    // Global variable that interactive GameObject scripts will set when a GameObject is deleted.
    // Maybe there is a race condition, but the user cannot deleted 2 object at the same time, so this will 
    // be okay
    public GameObject lastDeletedObj = null;  
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update the boolean 'pointer_enter_obj' to true
    public void handlePointerEntry()
    {
        pointer_enter_obj = true;

    }

    // Update the boolean 'pointer_enter_obj' to false
    public void handlePointerExit()
    {
        pointer_enter_obj = false;
    }
    
    Vector3? GetXRPointerLocation()
    {
        // Couldn't figure out a way to grab the XR pointer location, so 
        // use a raycast light to estamate that location by shooting a light ray where the player is looking 
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit pointer_location; 

        if (Physics.Raycast(ray, out pointer_location, 50f))
        { // Teleport the player 
            return pointer_location.point;
        } 
        return null; 
    }
    // Update is called once per frame
    void Update()
    {
        if (pointer_enter_obj)
        {   
            // Handle Joystick button presses on Linux OS as well as PC since I map those buttons to be the same 
            if (Input.GetButtonDown("js9") || Input.GetButtonDown("js0"))
            { // 'a' button on joystick or 'z' button keyboard
                
                // Disable character controller. It interfere with teleportation. 
                CharacterController char_controller = player.GetComponent<CharacterController>();
                char_controller.enabled = false; 

                Vector3? pointer_location = GetXRPointerLocation();

                if (pointer_location.HasValue)
                {
                    Vector3 adjusted_y = pointer_location.Value;
                    adjusted_y.y = 1.08f;
                    player.transform.position = adjusted_y;
                }
                
                char_controller.enabled = true;
            }
            else if (Input.GetButtonDown("js3"))
            { // 'y' button on joystick and keyboard
                Debug.Log("Y button pressed");
                
                Vector3? pointer_location = GetXRPointerLocation();

                if (pointer_location.HasValue)
                {   
                    if (lastDeletedObj == null) return; 

                    // Teleport gameobject
                    lastDeletedObj.transform.position = pointer_location.Value;
                    Vector3 tempPos = lastDeletedObj.transform.position;
                    tempPos.y = 1.0f;

                    lastDeletedObj.transform.position = tempPos;
                    // Make visible again 
                    lastDeletedObj.SetActive(true);

                    lastDeletedObj = null;
                }
            } 
        }
    }
}
