using UnityEngine;

/// <summary>
/// Raycasts from camera center to detect interactable objects.
/// Only triggers when within the object's maxInteractDistance.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    public float rayRange = 50f;
    Transform mainCam = null;
    InteractableObjectScript currentTarget;



    void Update()
    {
        if (mainCam == null)
        { // Updated to work with multiplayer
            mainCam = LocalPlayerHolder.LocalCamera;
            Debug.Log("Setting first");
            if (mainCam == null)
            {
                mainCam = LocalPlayerHolder.GetLocalCamera();
                if (mainCam == null)
                {
                    Debug.Log("CAMERA IS TSILL NULL");
                    return;
                }
            }
            
            Debug.Log("Main Camera set for player!");
        }

        Debug.DrawRay(mainCam.position, mainCam.forward * rayRange, Color.red);
        
        Ray ray = new Ray(mainCam.position, mainCam.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, rayRange);

        InteractableObjectScript closest = null;
        float closestDist = float.MaxValue;

        foreach (RaycastHit h in hits)
        {
            InteractableObjectScript ia = h.collider.GetComponent<InteractableObjectScript>();
            if (ia == null) ia = h.collider.GetComponentInParent<InteractableObjectScript>();

            if (ia != null && h.distance < closestDist)
            {
                // Check if within this object's interaction range
                float playerDist = Vector3.Distance(transform.position, ia.transform.position);
                if (playerDist <= ia.maxInteractDistance)
                {
                    closest = ia;
                    closestDist = h.distance;
                }
            }
        }

        if (closest != null)
        {
            if (currentTarget != closest)
            {
                if (currentTarget != null) currentTarget.OnPointerExit();
                currentTarget = closest;
                currentTarget.PointerEnter();
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.OnPointerExit();
                currentTarget = null;
            }
        }
    }
}
