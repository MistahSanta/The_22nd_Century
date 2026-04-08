using UnityEngine;

/// <summary>
/// Prevents player from falling into the void.
/// Also ensures there's always a solid floor under the player.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerSafetyNet : MonoBehaviour
{
    [Tooltip("If player falls below this Y, teleport back to spawn.")]
    public float minY = -3f;

    Vector3 lastSafePosition;
    CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        lastSafePosition = transform.position;

        // Create a backup floor if one doesn't exist
        if (GameObject.Find("SafetyFloor") == null)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "SafetyFloor";
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(500, 1, 500);
            floor.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    void Update()
    {
        // Remember last safe position (when grounded)
        if (cc != null && cc.isGrounded && transform.position.y > minY)
        {
            lastSafePosition = transform.position;
        }

        // Teleport back if fallen
        if (transform.position.y < minY)
        {
            if (cc != null) cc.enabled = false;
            transform.position = lastSafePosition + Vector3.up * 2f;
            if (cc != null) cc.enabled = true;
            Debug.Log("Player fell - teleported to last safe position.");
        }
    }
}
