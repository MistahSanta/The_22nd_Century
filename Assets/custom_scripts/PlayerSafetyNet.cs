using UnityEngine;

/// <summary>
/// Hard boundary clamp. Cannot walk outside map no matter what.
/// </summary>
public class PlayerSafetyNet : MonoBehaviour
{
    // Tight map boundaries
    static float minX = -22f, maxX = 24f;
    static float minZ = -20f, maxZ = 24f;
    static float minY = -3f;

    Vector3 lastSafePos;

    void Start()
    {
        lastSafePos = transform.position;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        // Clamp to boundary
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        if (pos != transform.position)
            transform.position = pos;

        // Fall recovery
        if (transform.position.y < minY)
            transform.position = lastSafePos + Vector3.up * 2f;
        else
            lastSafePos = transform.position;
    }

    // Static method — any script can call this to clamp any position
    public static Vector3 ClampToMap(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        return pos;
    }
}
