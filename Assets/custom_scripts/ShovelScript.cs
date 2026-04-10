using UnityEngine;

// Planting tree tool
public class ShovelScript : MonoBehaviour
{
    bool isEquipped = false;
    public Transform mainCamera;

    public void SetEquipped()
    {
        isEquipped = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PickUpShovel();
            GameManager.Instance.EquipShovel();
        }

        foreach (Light l in GetComponentsInChildren<Light>())
            l.enabled = false;

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

        Debug.Log("Shovel equipped!");
    }

    void LateUpdate()
    {
        if (!isEquipped || mainCamera == null) return;

        bool isActive = GameManager.Instance != null &&
                        GameManager.Instance.CurrentTool == GameManager.EquippedTool.Shovel;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = isActive;

        if (!isActive) return;

        // Follow camera on right side
        Vector3 target = mainCamera.position
            + mainCamera.forward * 0.5f
            + mainCamera.right * 0.25f
            - mainCamera.up * 0.3f;
        transform.position = Vector3.Lerp(transform.position, target, 8f * Time.deltaTime);
        transform.rotation = mainCamera.rotation;
    }
}
