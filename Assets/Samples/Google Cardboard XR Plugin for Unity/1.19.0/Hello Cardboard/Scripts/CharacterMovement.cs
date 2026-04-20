using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CharacterMovement : NetworkBehaviour
{
    private NetworkCharacterController _cc;
    public float speed = 5f;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // Normalize the input to ensure diagonal movement isn't faster
            Vector3 inputDir = data.Direction.normalized;

            Vector3 moveVect = CalculateMoveDirection(inputDir);
            
            _cc.Move(moveVect * speed * Runner.DeltaTime);
        }
    }

    private Vector3 CalculateMoveDirection(Vector3 inputDir)
    {
        // Use the Transform of the NetworkObject or a stable reference
        // In VR, it's often better to move relative to the "Forward" of the XR Rig
        Transform camTransform = Camera.main.transform;
        
        Vector3 forward = camTransform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = camTransform.right;
        right.y = 0;
        right.Normalize();

        return (forward * inputDir.z) + (right * inputDir.x);
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
