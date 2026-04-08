using UnityEngine;

/// <summary>
/// Raycasts from camera center to detect interactable objects.
/// Only triggers when within the object's maxInteractDistance.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    public float rayRange = 50f;
    Camera mainCam;
    InteractableObjectScript currentTarget;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            return;
        }

        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
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
