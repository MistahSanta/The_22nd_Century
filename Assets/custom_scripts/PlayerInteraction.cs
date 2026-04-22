using UnityEngine;
using Fusion;

/// <summary>
/// Raycasts from camera center to detect interactable objects.
/// Only triggers when within the object's maxInteractDistance.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    public float rayRange = 50f;
    Transform mainCam = null;
    InteractableObjectScript currentTarget;
    public LayerMask interactableLayer;


void Update()
{
    //if (!HasInputAuthority) return;

    // Find camera ONCE, scoped to THIS player's hierarchy
    if (mainCam == null)
    {
        // Search only within this player's own GameObject hierarchy
        Transform xrRig = transform.Find("XRCardboardRig");
        Transform heightOffset = xrRig?.Find("HeightOffset");
        mainCam = heightOffset?.Find("Main Camera");

        if (mainCam == null)
        {
            Debug.LogWarning("[PI] No camera found in player hierarchy");
            return;
        }
        
        Debug.Log($"[PI] Found camera: {mainCam.gameObject.name} under {gameObject.name}");
    }

    Ray ray = new Ray(mainCam.position, mainCam.forward);
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
