using UnityEngine;

/// <summary>
/// Detects when player is near trash and looking at it.
/// Only works after player has garbage picker.
/// </summary>
public class TrashProximityDetector : MonoBehaviour
{
    float detectRange = 3f;
    float gazeAngle = 25f;
    bool isShowing = false;
    InteractableObjectScript interactable;

    void Start()
    {
        interactable = GetComponent<InteractableObjectScript>();
    }

    void Update()
    {
        // Only detect if player has garbage picker
        if (GameManager.Instance == null || !GameManager.Instance.HasGarbagePicker)
        {
            if (isShowing) Hide();
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        float dist = Vector3.Distance(cam.transform.position, transform.position);
        if (dist > detectRange)
        {
            if (isShowing) Hide();
            return;
        }

        Vector3 dir = (transform.position - cam.transform.position).normalized;
        float angle = Vector3.Angle(cam.transform.forward, dir);

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
        isShowing = true;
        if (interactable != null) interactable.PointerEnter();
    }

    void Hide()
    {
        isShowing = false;
        if (interactable != null) interactable.OnPointerExit();
    }
}
