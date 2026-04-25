using Fusion;
using UnityEngine;

public class NetworkPlayerSetup : NetworkBehaviour
{
    [SerializeField] private GameObject localOnlyObjects; // XRCardboardRig, ControlsMenu, etc.
    [SerializeField] private GameObject remoteVisual;     // the capsule/mesh others see

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            localOnlyObjects.SetActive(true);   // enable our VR rig
            remoteVisual.SetActive(false);       // hide our own body

            localOnlyObjects.transform.localPosition = Vector3.zero;
            
            
            // // This is OUR player on OUR device
            // if (localOnlyObjects != null)
            //     localOnlyObjects.SetActive(true);

            // if (remoteVisual != null)
            //     remoteVisual.SetActive(false); // hide our own body
        }
        else
        {

            localOnlyObjects.SetActive(false);  // don't activate their VR rig on our machine
            remoteVisual.SetActive(true);   

            // // This is someone else's player on OUR device
            // if (localOnlyObjects != null)
            //     localOnlyObjects.SetActive(false); // disable their VR rig on our machine

            // if (remoteVisual != null)
            //     remoteVisual.SetActive(true);
        }
    }
}