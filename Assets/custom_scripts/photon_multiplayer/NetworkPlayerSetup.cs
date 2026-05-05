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

            // Attach local player features
            if (GetComponent<PlayerHealth>() == null)
                gameObject.AddComponent<PlayerHealth>();
            if (GetComponent<ObjectiveTracker>() == null)
                gameObject.AddComponent<ObjectiveTracker>();
            if (GetComponent<PlayerSafetyNet>() == null)
                gameObject.AddComponent<PlayerSafetyNet>();

        }
        else
        {
            localOnlyObjects.SetActive(false);
            remoteVisual.SetActive(true);

            // Add health to remote player so local player can see their HP
            if (GetComponent<PlayerHealth>() == null)
            {
                var hp = gameObject.AddComponent<PlayerHealth>();
                hp.isLocalPlayer = false;
            }
        }


    }
}