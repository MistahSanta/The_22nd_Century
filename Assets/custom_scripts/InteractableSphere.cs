using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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


public class InteractableSphere : MonoBehaviour
{
    bool pointer_enter_obj = false; // Track if pointer is touching the obj.
    public GameObject sphere; 
    
    private FloorScript floor; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        floor = GameObject.FindAnyObjectByType<FloorScript>();  

        if (floor == null)
        {
            Debug.Log("Unable to find Floor Object!");
        } 
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

    // Update is called once per frame
    void Update()
    {
        if (pointer_enter_obj)
        {   
            // Handle Joystick button presses on Linux OS as well as PC since I map those buttons to be the same 
            if (Input.GetButtonDown("js3"))
            { // 'y' button on joystick and keyboard

                // Send the lastDeletedObj to the floor script to handle restoration if the user wants
                floor.lastDeletedObj = sphere;
                sphere.SetActive(false); 
            } 
        }
    }
}
