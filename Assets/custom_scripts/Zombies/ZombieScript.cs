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
        if (!HasStateAuthority) 
        {
            agent.enabled = false; // disable nav on proxy clients
            return;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        FindClosestPlayer();

        if (closestPlayer != null)
        {
            agent.isStopped = false;
            animator.SetTrigger("Walk");
            agent.SetDestination(closestPlayer.position);
        }else
        {   
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetTrigger("Idle");
            //Debug.Log("No player found for zombie!");
        }
    }

    void FindClosestPlayer()
    {
        PlayerRef[] players = Runner.ActivePlayers.ToArray();
        float closestDist = Mathf.Infinity;
        closestPlayer = null;

        foreach (PlayerRef playerRef in players)
        {
            if (Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObj))
            {
                float dist = Vector3.Distance(transform.position, playerObj.transform.position);
                if (dist < closestDist && dist < detectionRange)
                {

                    closestDist = dist;
                    closestPlayer = playerObj.transform;
                }else
                {
                    //Debug.Log("Cant find player object!");
                }
            }
        }
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
