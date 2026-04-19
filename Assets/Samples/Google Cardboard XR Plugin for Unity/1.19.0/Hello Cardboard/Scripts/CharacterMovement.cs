using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CharacterMovement : NetworkBehaviour
{
    private NetworkCharacterController _cc;
    CharacterController charCntrl;
    [Tooltip("The speed at which the character will move.")]
    public float speed = 5f;
    [Tooltip("The camera representing where the character is looking.")]
    public GameObject cameraObj;
    [Tooltip("Should be checked if using the Bluetooth Controller to move. If using keyboard, leave this unchecked.")]
    public bool joyStickMode;


    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        
        // GetInput retrieves the data sent from the local player to the server
        if (GetInput(out NetworkInputData data))
        {
            Debug.Log("Input received: " + data.Direction);
            Vector3 moveVect = CalculateMoveDirection(data.Direction);
            
            // In Fusion, we use the NetworkCharacterController's Move method
            _cc.Move(moveVect * speed * Runner.DeltaTime);
        }
    }

    private Vector3 CalculateMoveDirection(Vector3 inputDir)
    {
        if (cameraObj == null) return Vector3.zero;

        Vector3 cameraLook = cameraObj.transform.forward;
        cameraLook.y = 0f;
        cameraLook = cameraLook.normalized;

        Vector3 forwardVect = cameraLook;
        Vector3 rightVect = Vector3.Cross(forwardVect, Vector3.up).normalized * -1;

        Vector3 moveVect = (rightVect * inputDir.x) + (forwardVect * inputDir.z);
        return moveVect.normalized;
    }

    // Start is called before the first frame update
    // void Start()
    // {
    //     charCntrl = GetComponent<CharacterController>();
    // }

    // Update is called once per frame
    // void Update()
    // {
    //     //Get horizontal and Vertical movements
    //     float horComp = Input.GetAxis("Horizontal");
    //     float vertComp = Input.GetAxis("Vertical");

    //     if (joyStickMode)
    //     {
    //         horComp = Input.GetAxis("Vertical");
    //         vertComp = Input.GetAxis("Horizontal") * -1;
    //     }

    //     Vector3 moveVect = Vector3.zero;

    //     //Get look Direction
    //     Vector3 cameraLook = cameraObj.transform.forward;
    //     cameraLook.y = 0f;
    //     cameraLook = cameraLook.normalized;

    //     Vector3 forwardVect = cameraLook;
    //     Vector3 rightVect = Vector3.Cross(forwardVect, Vector3.up).normalized * -1;

    //     moveVect += rightVect * horComp;
    //     moveVect += forwardVect * vertComp;

    //     moveVect *= speed;
     

    //     charCntrl.SimpleMove(moveVect);


    // }
}
