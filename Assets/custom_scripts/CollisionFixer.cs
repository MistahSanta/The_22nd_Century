using UnityEngine;

/// <summary>
/// Fixes oversized colliders on trees and other environment objects.
/// Also adds boundary walls to prevent falling off map edge.
/// Attach to a persistent scene object.
/// </summary>
public class CollisionFixer : MonoBehaviour
{
    // Tight boundary around actual map content
    public float mapMinX = -20f;
    public float mapMaxX = 22f;
    public float mapMinZ = -18f;
    public float mapMaxZ = 22f;
    public float wallHeight = 15f;

    void Start()
    {
        FixTreeColliders();
        CreateBoundaryWalls();
        EnableEdgeFog();
    }

    void EnableEdgeFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 10f;
        RenderSettings.fogEndDistance = 25f;
        RenderSettings.fogColor = new Color(0.35f, 0.4f, 0.45f, 1f);
    }

    void FixTreeColliders()
    {
        // Find all objects with "tree" or "Tree" in name and fix oversized colliders
        foreach (var col in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
        {
            string name = col.gameObject.name.ToLower();
            if (name.Contains("tree") || name.Contains("shrub") || name.Contains("plant"))
            {
                // Only fix if collider is unreasonably large
                if (col.size.magnitude > 10f)
                {
                    // Fit to renderer bounds
                    Renderer r = col.GetComponent<Renderer>();
                    if (r == null) r = col.GetComponentInChildren<Renderer>();
                    if (r != null)
                    {
                        col.center = col.transform.InverseTransformPoint(r.bounds.center);
                        col.size = r.bounds.size * 0.8f;
                    }
                    else
                    {
                        col.size = Vector3.one * 2f;
                    }
                }
            }
        }

        // Also fix CapsuleColliders on trees
        foreach (var col in FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None))
        {
            string name = col.gameObject.name.ToLower();
            if (name.Contains("tree") || name.Contains("shrub"))
            {
                if (col.radius > 3f)
                    col.radius = 0.5f;
            }
        }
    }

    void CreateBoundaryWalls()
    {
        if (GameObject.Find("BoundaryWalls") != null) return;

        GameObject parent = new GameObject("BoundaryWalls");
        float sizeX = mapMaxX - mapMinX;
        float sizeZ = mapMaxZ - mapMinZ;
        float centerX = (mapMinX + mapMaxX) / 2f;
        float centerZ = (mapMinZ + mapMaxZ) / 2f;

        // 4 walls along map edges
        CreateWall(parent, "WallNorth", new Vector3(centerX, wallHeight/2, mapMaxZ), new Vector3(sizeX, wallHeight, 1));
        CreateWall(parent, "WallSouth", new Vector3(centerX, wallHeight/2, mapMinZ), new Vector3(sizeX, wallHeight, 1));
        CreateWall(parent, "WallEast", new Vector3(mapMaxX, wallHeight/2, centerZ), new Vector3(1, wallHeight, sizeZ));
        CreateWall(parent, "WallWest", new Vector3(mapMinX, wallHeight/2, centerZ), new Vector3(1, wallHeight, sizeZ));
    }

    void CreateWall(GameObject parent, string name, Vector3 pos, Vector3 size)
    {
        // Invisible collision wall
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent.transform);
        wall.transform.position = pos;
        BoxCollider col = wall.AddComponent<BoxCollider>();
        col.size = size;

        // Visible fog wall (semi-transparent quad)
        GameObject fogWall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fogWall.name = name + "_Fog";
        fogWall.transform.SetParent(wall.transform, false);
        fogWall.transform.localPosition = Vector3.zero;

        // Scale quad to match wall
        float width = Mathf.Max(size.x, size.z);
        fogWall.transform.localScale = new Vector3(width, size.y, 1);

        // Rotate to face inward
        if (size.x > size.z)
            fogWall.transform.localRotation = Quaternion.identity; // N/S walls face Z
        else
            fogWall.transform.localRotation = Quaternion.Euler(0, 90, 0); // E/W walls face X

        // Remove collider on fog quad
        var quadCol = fogWall.GetComponent<Collider>();
        if (quadCol != null) Object.Destroy(quadCol);

        // Semi-transparent fog material
        var renderer = fogWall.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.4f, 0.45f, 0.5f, 0.4f);
            renderer.material = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
