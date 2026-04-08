using UnityEngine;

/// <summary>
/// Same as GunProximityDetector but for the garbage picker.
/// </summary>
public class GrabberProximityDetector : MonoBehaviour
{
    float detectRange = 5f;
    float gazeAngle = 30f;
    bool isShowing = false;
    TrashGrabberScript grabScript;
    InteractableObjectScript interactable;

    void Start()
    {
        grabScript = GetComponent<TrashGrabberScript>();
        interactable = GetComponent<InteractableObjectScript>();
    }

    void Update()
    {
        // Don't detect if already equipped
        if (GameManager.Instance != null && GameManager.Instance.HasGarbagePicker)
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

        Vector3 dirToObj = (transform.position - cam.transform.position).normalized;
        float angle = Vector3.Angle(cam.transform.forward, dirToObj);

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
