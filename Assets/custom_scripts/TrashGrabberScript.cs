using UnityEngine;

/// <summary>
/// Garbage picker tool. Pick up then use to collect trash.
/// </summary>
public class TrashGrabberScript : MonoBehaviour
{
    bool isEquipped = false;
    public Transform mainCamera;

    public void SetEquipped()
    {
        isEquipped = true;
        if (GameManager.Instance != null)
            GameManager.Instance.PickUpGarbagePicker();

        // Turn off glow light (keep model visible in hand)
        foreach (Light l in GetComponentsInChildren<Light>())
            l.enabled = false;

        // Remove emission glow
        try
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                foreach (Material mat in r.materials)
                {
                    mat.DisableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                }
        }
        catch { }

        Debug.Log("Garbage picker equipped!");
    }

    void LateUpdate()
    {
        if (!isEquipped || mainCamera == null) return;

        // Follow camera on left side
        Vector3 target = mainCamera.position
            + mainCamera.forward * 0.5f
            - mainCamera.right * 0.25f
            - mainCamera.up * 0.3f;
        transform.position = Vector3.Lerp(transform.position, target, 8f * Time.deltaTime);
        transform.rotation = mainCamera.rotation;
    }
}
