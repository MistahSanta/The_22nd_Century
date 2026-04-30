using Fusion;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class ZombieScript : NetworkBehaviour
{
    private int zombie_health = 100;
    private Animator animator;
    private bool isDead = false;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] float detectionRange = 15f;
    Transform closestPlayer;

    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();

        if (!HasStateAuthority)
        {
            return;
        }

        if (agent != null) agent.enabled = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        FindClosestPlayer();

        if (closestPlayer != null)
        {
            agent.isStopped = false;
            animator.SetTrigger("Walk");
            agent.SetDestination(closestPlayer.position);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetTrigger("Idle");
            //Debug.Log("No player found for zombie!");
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDist = Mathf.Infinity;
        closestPlayer = null;

        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < closestDist && dist < detectionRange)
            {
                closestDist = dist;
                closestPlayer = player.transform;
            }
        }

        if (closestPlayer == null)
            Debug.Log("Cant find player object!");
    }

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
            var spawner = FindObjectOfType<ZombieSpawner>();
            if (spawner != null) spawner.ZombieDied();
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
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.Log("Agent is not set in zombie!");
        }


    }
}
