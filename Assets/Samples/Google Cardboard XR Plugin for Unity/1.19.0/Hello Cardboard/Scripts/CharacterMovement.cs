using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting.FullSerializer;
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


            Vector3 inputDir = data.Direction.normalized;

            Vector3 moveVect;

            if (LocalPlayerHolder.GetLocalCamera() != null) 
            {
                float headsetYaw = data.CameraYaw;
                Quaternion flatRotation = Quaternion.Euler(0, headsetYaw, 0);

                Vector3 forward = flatRotation * Vector3.forward;
                Vector3 right   = flatRotation * Vector3.right;

                moveVect = (forward * inputDir.z) + (right * inputDir.x);
            
            
            } else
            {
                moveVect = new Vector3(inputDir.x, 0, inputDir.z);

            }

            _cc.Move(moveVect * speed * Runner.DeltaTime);


        }         
    }
}