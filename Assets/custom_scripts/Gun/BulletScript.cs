using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider obj)
    {
        // Check if we hit a zombie
        ZombieScript zombie = obj.GetComponent<ZombieScript>();
        if (zombie == null)
            zombie = obj.GetComponentInParent<ZombieScript>(true);

        if (zombie != null)
        {
            Debug.Log("Bullet hit zombie: " + obj.name);
            Destroy(gameObject);
            zombie.take_damage(50);
            return;
        }

        // Destroy bullet on hitting anything solid (not player, not trigger)
        if (!obj.isTrigger && obj.GetComponent<CharacterController>() == null)
        {
            Destroy(gameObject, 0.01f);
        }
    }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     // Fallback: also check regular collisions
    //     ZombieScript zombie = collision.gameObject.GetComponent<ZombieScript>();
    //     if (zombie == null)
    //         zombie = collision.gameObject.GetComponentInParent<ZombieScript>(true);

    //     if (zombie != null)
    //     {
    //         Debug.Log("Bullet collision hit zombie: " + collision.gameObject.name);
    //         zombie.take_damage(50);
    //         Destroy(gameObject, 0.05f);
    //     }
    // }
}
