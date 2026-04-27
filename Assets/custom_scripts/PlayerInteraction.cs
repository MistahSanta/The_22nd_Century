using UnityEngine;
using Fusion;
using System.Collections;

/// <summary>
/// Raycasts from camera center to detect interactable objects.
/// Only triggers when within the object's maxInteractDistance.
/// </summary>
public class PlayerInteraction : NetworkBehaviour
{
    public float rayRange = 50f;
    Camera mainCam = null;
    InteractableObjectScript currentTarget;
    public LayerMask interactableLayer;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority) return; // Don't set up camera for remote players
        
        // Use the already-found local camera instead of searching hierarchy
        // mainCam = LocalPlayerHolder.GetLocalCamera()?.GetComponent<Camera>();
        
        // if (mainCam == null)
        //     Debug.LogWarning("[PlayerInterction] No camera found via LocalPlayerHolder");

        StartCoroutine(WaitForCamera());
    }

    private IEnumerator WaitForCamera()
    {
        int attempts = 0;
        while (attempts < 20)
        {
            Transform camTransform = LocalPlayerHolder.GetLocalCamera();
            if (camTransform != null)
            {
                mainCam = camTransform.GetComponent<Camera>();
                Debug.Log("[PlayerInteraction] Camera ready");
                yield break;
            }
            yield return new WaitForSeconds(0.2f);
            attempts++;
        }
        Debug.LogError("[PlayerInteraction] Camera never found!");
    }

    void Update()
    {
        //if (!HasInputAuthority) return;

        // Find camera ONCE, scoped to THIS player's hierarchy
        if (mainCam == null)
        {
            // Search only within this player's own GameObject hierarchy
            var camTransform = LocalPlayerHolder.GetLocalCamera();
            if (camTransform == null) return; // Not ready yet, try next frame
            mainCam = camTransform.GetComponent<Camera>();
            
            Debug.Log($"[PI] Found camera: {mainCam.gameObject.name} under {gameObject.name}");
        }

        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * rayRange, Color.yellow); // For debugging
        LayerMask hardcodedMask = LayerMask.GetMask("Interactable");

        // Photon Fusion need to use gameObject current physic scene, not physics scene!!
        PhysicsScene physicsScene = gameObject.scene.GetPhysicsScene();

        if (physicsScene.Raycast(ray.origin, ray.direction, out hit, rayRange, hardcodedMask, QueryTriggerInteraction.Collide))
        {
            // Find the script on the gun/object
            InteractableObjectScript io = hit.collider.GetComponent<InteractableObjectScript>();

            if (io != null)
            {
                // If we just started looking at this specific object
                if (currentTarget != io)
                {
                    // Tell the old object we are gone
                    if (currentTarget != null) currentTarget.OnPointerExit();

                    // Tell the new object we are here
                    currentTarget = io;
                    currentTarget.PointerEnter(); // This sets io.isPlayerLooking = true
                }
            }
        }
        else
        {
            // If the ray hits nothing, clear the current target
            if (currentTarget != null)
            {
                currentTarget.OnPointerExit(); // This sets currentTarget.isPlayerLooking = false
                currentTarget = null;
            }
        }
    }

}
