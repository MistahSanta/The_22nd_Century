using UnityEngine;

/// <summary>
/// Detects when player is nearby and looking at the gun.
/// Shows pickup prompt without relying on raycast hitting the collider.
/// </summary>
public class GunProximityDetector : MonoBehaviour
{
    float detectRange = 5f;
    float gazeAngle = 30f; // degrees
    bool isShowing = false;
    GunScript gunScript;
    InteractableObjectScript interactable;

    void Start()
    {
        gunScript = GetComponent<GunScript>();
        interactable = GetComponent<InteractableObjectScript>();
    }

    void Update()
    {
        if (gunScript == null) return;
        // Don't detect after pickup
        if (gunScript.IsEquipped()) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        float dist = Vector3.Distance(cam.transform.position, transform.position);
        if (dist > detectRange)
        {
            if (isShowing) Hide();
            return;
        }

        // Check if camera is looking at gun
        Vector3 dirToGun = (transform.position - cam.transform.position).normalized;
        float angle = Vector3.Angle(cam.transform.forward, dirToGun);

        if (angle < gazeAngle)
        {
            if (!isShowing) Show();
        }
        else
        {
            if (isShowing) Hide();
        }
    }

    void Show()
    {
        if (isShowing) return;
        isShowing = true;
        if (interactable != null)
            interactable.PointerEnter();
    }

    void Hide()
    {
        if (!isShowing) return;
        isShowing = false;
        if (interactable != null)
            interactable.OnPointerExit();
    }

    void OnDisable()
    {
        Hide();
    }

    // OnGUI handled by InteractableObjectScript
}
