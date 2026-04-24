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

}