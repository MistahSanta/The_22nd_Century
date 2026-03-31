using UnityEngine;

public class ZombieScript : MonoBehaviour
{   
    private int zombie_health = 100;
    private Animator animator;

    public void take_damage(int bullet_damage)
    {
        
        Debug.Log("Zombie got hit!");
        zombie_health -= bullet_damage;
        
        if (zombie_health <= 0)
        {
            Debug.Log("zombie has die. play animation");
            animator.SetTrigger("Die");

            Destroy(gameObject, 1.1f);
            return;
        }

        animator.SetTrigger("Hit");


    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.Log("You forgot add animator to zombie!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
