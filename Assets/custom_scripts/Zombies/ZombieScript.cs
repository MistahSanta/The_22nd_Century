using UnityEngine;
using UnityEngine.AI;

public class ZombieScript : MonoBehaviour
{
    private int zombie_health = 100;
    private Animator animator;
    private bool isDead = false;
    private NavMeshAgent agent; 
    [Header("Movement")]
    public float moveSpeed = 0.5f;
    public float rotateSpeed = 3f;

    Transform player;
    float groundY;
    

    public void take_damage(int bullet_damage)
    {
        if (isDead) return;
        Debug.Log("Zombie got hit! Damage: " + bullet_damage + " HP left: " + (zombie_health - bullet_damage));
        zombie_health -= bullet_damage;
        HapticFeedback.VibrateHit();

        // Flash red on hit
        TurnRed();

        if (zombie_health <= 0)
        {
            isDead = true;
            Debug.Log("Zombie died!");
            if (animator != null) animator.SetTrigger("Die");
            agent.isStopped = true;
            // Fade out and destroy
            Destroy(gameObject, 1f);
            return;
        }

        if (animator != null) animator.SetTrigger("Hit");
    }

    void TurnRed()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                mat.color = Color.red;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.red * 0.5f);
            }
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        groundY = transform.position.y;
        moveSpeed = 0.5f;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Make sure collider exists and is big enough
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider>();
        // Make collider big relative to zombie size
        box.center = new Vector3(0, 1.5f, 0);
        box.size = new Vector3(2f, 3f, 2f);

        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.Log("Agent is not set in zombie!");   
        }

        
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsInPresent) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (agent.isOnNavMesh) {
            agent.SetDestination(player.position);
        } else {
            Debug.LogError(gameObject.name + " is NOT on the NavMesh!");
        }
        // Vector3 direction = player.position - transform.position;
        // direction.y = 0;

        // if (direction.magnitude > 0.1f)
        // {
        //     Quaternion targetRotation = Quaternion.LookRotation(direction);
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        //     Vector3 move = direction.normalized * moveSpeed * Time.deltaTime;
        //     move.y = 0;
        //     transform.position += move;

        //     Vector3 pos = transform.position;
        //     pos.y = groundY;
        //     transform.position = pos;
        // }

        // // Game over when zombie reaches player
        // if (direction.magnitude < 1.2f)
        // {
        //     if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        //         GameManager.Instance.GameOver();
        // }
    }
}
