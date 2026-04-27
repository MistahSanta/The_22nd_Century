using UnityEngine;
using UnityEngine.UI;

public class SettingManagerScript : MonoBehaviour
{
    public Button[] buttons; // Button order in the setting menu. 0 = top menu and stuff below = greater number
    private int current_button_index = 0;
    private float selection_delay = 0.19f; 
    private float next_select_time = 0f;
    private CharacterMovement char_move;
    private Canvas setting_menu_panel;
    public bool menu_is_open = false;

    void Start()
    {
        menu_is_open = false;
        // Highlight the top menu - resume 
        Image top_button =  buttons[0].gameObject.GetComponent<Image>();
        top_button.color = Color.yellow;

        char_move = transform.root.GetComponentInChildren<CharacterMovement>();
        if (char_move == null)
        {
            Debug.Log("Unable to find charactermovement script for setting menu");
        }

        setting_menu_panel = GetComponent<Canvas>();

        if (setting_menu_panel == null)
        {
            Debug.Log("Unable to find setting_menu_panel in settingmangaer script!");
        }

    }

    void HighlightNextButton(int direction)
    {
        if (direction != 1 && direction != -1 )
        {
            Debug.Log("Direction is not 1 or -1, could be not intended! Direction given" + direction.ToString());
        }

        // Keep the index within the index of the buttons
        int button_index = (current_button_index + direction + buttons.Length) % buttons.Length; 

        // unlight previous button first 
        Image prev_button_image = buttons[current_button_index].GetComponent<Image>();
        Image button_image = buttons[button_index].GetComponent<Image>();

        button_image.color = Color.yellow;
        prev_button_image.color = Color.white;
        current_button_index = button_index;        
    }
    // Update is called once per frame
    void Update()
    {
    
        bool menu = ControllerMapping.Instance != null
                ? ControllerMapping.Instance.GetMenuDown()
                : Input.GetKeyDown(KeyCode.M);
        
        if (menu)
        {
            
            menu_is_open = true;
            char_move.enabled = false;
            setting_menu_panel.enabled = true;
            Debug.Log("Menu button is pressed. Openning");
        }

        if (menu_is_open == false ) return;
    
        // Update runs too fast so add a delay to slow it down 
        float vertical_input = Input.GetAxisRaw("Horizontal"); // For my setup, horizontal is up and down?? 
        // For any input, we just go to either 1 or -1 and not worry about floating precision 
        // since it is not needed 
        if ( Time.time >= next_select_time )
        {
            float joystick_threshold = 0.6f;
            if (vertical_input > joystick_threshold)
            {
                HighlightNextButton(1);
                next_select_time = Time.time + selection_delay;

            } else if (vertical_input < -joystick_threshold)
            {
                HighlightNextButton(-1);
                next_select_time = Time.time + selection_delay;
            }
        }

        bool a_button = ControllerMapping.Instance != null
                ? ControllerMapping.Instance.GetInteractDown()
                : Input.GetKeyDown(KeyCode.E);
        if (a_button)
        { // 'a' button on joystick or 'e' button keyboard
            // Run button
            IButton cur_button = buttons[current_button_index].GetComponent<IButton>();
            cur_button.Execute(); 


            if (current_button_index == 2)
            {   // reset highlight
                HighlightNextButton(-1);
                HighlightNextButton(-1);  
            }

        }
  
        
    }
}
