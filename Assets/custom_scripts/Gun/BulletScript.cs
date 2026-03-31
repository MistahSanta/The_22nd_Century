using UnityEngine;

public class BulletScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider obj)
    {
        // First, check if we collided with a zombie by trying to grab the zombie script
        ZombieScript zombie = obj.GetComponentInParent<ZombieScript>(true);

        if (zombie != null)
        {
            zombie.take_damage(50);
            Destroy(gameObject, 0.05f); // to prevent bullet from going through the zombie
        }

    }

}
