using Fusion;
using UnityEngine;

public class LocalPlayerHolder : NetworkBehaviour
{
    // public static Transform LocalCamera;

    //[SerializeField] private GameObject localOnlyObjects; // same ref as NetworkPlayerSetup

    // public override void Spawned()
    // {
    //     if (!Object.HasInputAuthority) return;

    //     // Activate the rig first so camera is findable
    //     if (localOnlyObjects != null)
    //         localOnlyObjects.SetActive(true);

    //     AssignCamera();
    // }

    // private void AssignCamera()
    // {
    //     // Search including inactive just in case
    //     Camera cam = GetComponentInChildren<Camera>(true);
    //     if (cam != null)
    //     {
    //         LocalCamera = cam.transform;
    //         Debug.Log("[LocalPlayerHolder] Camera assigned: " + cam.name);
    //         return;
    //     }

    //     Debug.LogWarning("[LocalPlayerHolder] Camera not found under player: " + gameObject.name);
    // }

    // public override void Render()
    // {
    //     // Keep as a fallback only
    //     if (LocalCamera != null) return;
    //     if (Object == null || !Object.HasInputAuthority) return;
    //     AssignCamera();
    // }

    // public override void Despawned(NetworkRunner runner, bool hasState)
    // {
    //     if (Object.HasInputAuthority)
    //         LocalCamera = null;
    // }
    
    
    public static Transform LocalCamera;

    public override void Render()
    {
        if (LocalCamera != null) return;
        if (Object == null || !Object.HasInputAuthority) return;

        // Search including inactive objects
        Camera cam = GetComponentInChildren<Camera>(true); // true = include inactive
        if (cam != null)
        {
            LocalCamera = cam.transform;
            Debug.Log("Local VR Camera found: " + cam.gameObject.name);
            return;
        }

        // Fallback: search ALL cameras in scene
        Camera[] allCams = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in allCams)
        {
            // Make sure it belongs to this player object
            if (c.transform.IsChildOf(transform))
            {
                LocalCamera = c.transform;
                Debug.Log("Fallback camera found: " + c.gameObject.name);
                return;
            }
        }

        Debug.LogWarning("Still can't find camera under: " + gameObject.name);
    }
    public static Transform GetLocalCamera()
    {
        if (LocalCamera == null || LocalCamera.gameObject == null)
        {
            // Re-find it if it was destroyed or lost
            var players = FindObjectsByType<LocalPlayerHolder>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p.Object != null && p.Object.HasInputAuthority)
                {
                    LocalCamera = p.GetComponentInChildren<Camera>()?.transform;
                }
            }
        }
        return LocalCamera;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Clean up so a future respawn starts fresh
        if (Object.HasInputAuthority)
        {
            LocalCamera = null;
        }
    }

}