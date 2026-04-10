using UnityEngine;

public class ShovelProximityDetector : MonoBehaviour
{
    float detectRange = 5f;
    float gazeAngle = 30f;
    bool isShowing = false;
    ShovelScript shovelScript;
    InteractableObjectScript interactable;

    void Start()
    {
        shovelScript = GetComponent<ShovelScript>();
        interactable = GetComponent<InteractableObjectScript>();
    }

    void Update()
    {
        // Don't detect if already equipped
        if (GameManager.Instance != null && GameManager.Instance.HasShovel)
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