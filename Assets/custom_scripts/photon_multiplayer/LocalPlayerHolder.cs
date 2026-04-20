using Fusion;
using UnityEngine;

public class LocalPlayerHolder : NetworkBehaviour
{
    public static Transform LocalCamera;
    
    // Render runs every frame. We'll use it to "catch" the camera 
    // the moment the player exists.
    public override void Render()
    {
        // If we already found it, stop looking
        if (LocalCamera != null) return;

        // If this specific object is the one YOU control
        if (Object != null && Object.HasInputAuthority)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                LocalCamera = cam.transform;
                Debug.Log("Success: Local VR Camera caught in Render loop!");
            }else
            {
                Debug.LogWarning("<color=yellow>HOLDER:</color> I have authority, but I can't find a Camera child!");
            }
        }
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

}