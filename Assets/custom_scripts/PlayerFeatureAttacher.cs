using UnityEngine;

/// <summary>
/// Continuously scans for player objects and attaches PlayerHealth + ObjectiveTracker.
/// Works with both scene players and Photon-spawned network players.
/// Attach to a persistent scene object (e.g. GameManager).
/// </summary>
public class PlayerFeatureAttacher : MonoBehaviour
{
    float checkTimer = 0;

    void Update()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer > 0) return;
        checkTimer = 1f; // Check every second

        // Strategy 1: Find by tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
            AttachTo(p);

        // Strategy 2: Find by CharacterController
        var controllers = FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
        foreach (var cc in controllers)
            AttachTo(cc.gameObject);

        // Strategy 3: Find by CharacterMovement script
        var movements = FindObjectsByType<CharacterMovement>(FindObjectsSortMode.None);
        foreach (var cm in movements)
            AttachTo(cm.gameObject);
    }

    void AttachTo(GameObject player)
    {
        if (player == null) return;

        if (player.GetComponent<PlayerHealth>() == null)
        {
            player.AddComponent<PlayerHealth>();
            Debug.Log("[Attacher] PlayerHealth added to: " + player.name);
        }

        if (player.GetComponent<ObjectiveTracker>() == null)
        {
            player.AddComponent<ObjectiveTracker>();
            Debug.Log("[Attacher] ObjectiveTracker added to: " + player.name);
        }
    }
}
