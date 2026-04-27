using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.XR.Management;

public class LocalPlayerHolder : NetworkBehaviour
{
    [System.NonSerialized] private Transform _LocalCamera;

    [SerializeField] private GameObject localOnlyObjects; // same ref as NetworkPlayerSetup

    public override void Spawned()
    {
        if (!Object.HasInputAuthority) return;

        // Activate the rig first so camera is findable
        if (localOnlyObjects != null)
            localOnlyObjects.SetActive(true);

        StartCoroutine(AssignCameraNextFrame());
        StartCoroutine(InitializeXROnce());
    }

    private IEnumerator InitializeXROnce()
    {
        yield return null; // let rig activate first

        var manager = XRGeneralSettings.Instance?.Manager;
        if (manager == null) yield break;

        if (manager.activeLoader == null)
        {
            yield return manager.InitializeLoader();
            if (manager.activeLoader != null)
            {
                manager.StartSubsystems();
                Debug.Log("[XR] Started fresh");
            }
        }
        else
        {
            Debug.Log("[XR] Already running, skipping");
        }
    }

    private IEnumerator AssignCameraNextFrame()
    {
        yield return null; // wait one frame for XR rig to initialize
        yield return null; // wait a second frame to be safe

        AssignCamera();

        // If still null, keep trying
        if (_LocalCamera == null)
            StartCoroutine(RetryAssignCamera());
    }

    private IEnumerator RetryAssignCamera()
{
    int attempts = 0;
    while (_LocalCamera == null && attempts < 10)
    {
        yield return new WaitForSeconds(0.2f);
        AssignCamera();
        attempts++;
    }

    if (_LocalCamera == null)
        Debug.LogError("[LocalPlayerHolder] Failed to assign camera after retries!");
    else
        Debug.Log("[LocalPlayerHolder] Camera assigned after retry: " + _LocalCamera.name);
}

    private void AssignCamera()
    {
        // Search including inactive just in case
        Camera cam = GetComponentInChildren<Camera>(true);
        if (cam != null)
        {
            _LocalCamera = cam.transform;
            cam.tag = "MainCamera";
            Debug.Log("[LocalPlayerHolder] Camera assigned: " + cam.name);
            return;
        }

        Debug.LogWarning("[LocalPlayerHolder] Camera not found under player: " + gameObject.name);
    }

    public override void Render()
    {
        // Keep as a fallback only
        if (_LocalCamera != null) return;
        if (Object == null || !Object.HasInputAuthority) return;
        AssignCamera();
    }


    public static Transform GetLocalCamera()
    {
         foreach (var p in FindObjectsByType<LocalPlayerHolder>(FindObjectsSortMode.None))
        {
            if (p.Object != null && p.Object.HasInputAuthority)
                return p._LocalCamera;
        }
        return null;
    }

    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Clean up so a future respawn starts fresh
        if (Object.HasInputAuthority)
        {
            _LocalCamera = null;
        }
    }

}