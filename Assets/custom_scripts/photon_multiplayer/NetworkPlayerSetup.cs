using Fusion;
using UnityEngine;
using UnityEngine.XR.Management;

public class NetworkPlayerSetup : NetworkBehaviour
{
    [SerializeField] private GameObject localOnlyObjects; // XRCardboardRig, ControlsMenu, etc.
    [SerializeField] private GameObject remoteVisual;     // the capsule/mesh others see

    
    public override void Spawned()
    {
        
        if (HasInputAuthority)
        {
            localOnlyObjects.SetActive(true);   // enable our VR rig
            remoteVisual.SetActive(false);       // hide our own body

            Camera vrCam = GetComponentInChildren<Camera>(true);
            if (vrCam != null)
                vrCam.gameObject.SetActive(true);

            var eventSystem = GetComponentInChildren<UnityEngine.EventSystems.EventSystem>(true);
            if (eventSystem != null)
            {
                eventSystem.gameObject.SetActive(true);
            }
            
        }
        else
        {

            localOnlyObjects.SetActive(false);  // don't activate their VR rig on our machine
            remoteVisual.SetActive(true);   
           
        }

        
    }
}