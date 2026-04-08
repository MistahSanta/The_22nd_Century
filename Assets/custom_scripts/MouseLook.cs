using UnityEngine;

/// <summary>
/// Mouse look for editor testing ONLY.
/// Automatically disabled on phone/device (VR headset handles look).
/// </summary>
public class MouseLook : MonoBehaviour
{
    public float sensitivity = 2f;
    float rotationX = 0f;
    float rotationY = 0f;
    Transform cam;

    void Start()
    {
        // Only enable in Editor, disable on device
        if (Application.isMobilePlatform || SystemInfo.supportsGyroscope)
        {
            enabled = false;
            return;
        }

        cam = Camera.main?.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (cam == null)
        {
            cam = Camera.main?.transform;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }

        if (Cursor.lockState != CursorLockMode.Locked) return;

        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
        rotationY = Mathf.Clamp(rotationY, -80f, 80f);

        transform.rotation = Quaternion.Euler(0, rotationX, 0);
        cam.localRotation = Quaternion.Euler(rotationY, 0, 0);
    }
}
