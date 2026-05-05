using UnityEngine;

/// <summary>
/// Enforces map boundaries for ALL players every frame.
/// Attach to GameManager or any persistent scene object.
/// </summary>
public class GlobalBoundaryEnforcer : MonoBehaviour
{
    float minX = -22f, maxX = 24f;
    float minZ = -20f, maxZ = 24f;

    void LateUpdate()
    {
        // Clamp all tagged players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
            ClampPosition(p.transform);

        // Also clamp all CharacterControllers
        foreach (var cc in FindObjectsByType<CharacterController>(FindObjectsSortMode.None))
            ClampPosition(cc.transform);
    }

    void ClampPosition(Transform t)
    {
        Vector3 pos = t.position;
        bool clamped = false;

        if (pos.x < minX) { pos.x = minX; clamped = true; }
        if (pos.x > maxX) { pos.x = maxX; clamped = true; }
        if (pos.z < minZ) { pos.z = minZ; clamped = true; }
        if (pos.z > maxZ) { pos.z = maxZ; clamped = true; }

        if (clamped)
        {
            var cc = t.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            t.position = pos;
            if (cc != null) cc.enabled = true;
        }
    }
}
