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

        // Show damage number only in Future (not Present)
        try
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsInPresent)
                ShowDamageNumber(bullet_damage);
        }
        catch { ShowDamageNumber(bullet_damage); }

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

    void ShowDamageNumber(int damage)
    {
        GameObject dmgObj = new GameObject("DamageNum");
        dmgObj.transform.position = transform.position + Vector3.up * 2f;

        Canvas c = dmgObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingOrder = 200;

        RectTransform cRect = dmgObj.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(60, 25);
        cRect.localScale = new Vector3(0.008f, 0.008f, 0.008f);

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(dmgObj.transform, false);
        var text = txt.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "-" + damage;
        text.fontSize = 20;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = Color.red;
        text.fontStyle = TMPro.FontStyles.Bold;
        txt.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 25);

        // Float up and destroy (backup: auto-destroy after 1.5s)
        Destroy(dmgObj, 1.5f);
        StartCoroutine(FloatDamageNumber(dmgObj));
    }

    System.Collections.IEnumerator FloatDamageNumber(GameObject obj)
    {
        float timer = 0.8f;
        Vector3 startPos = obj.transform.position;
        Camera cam = Camera.main;

        while (timer > 0 && obj != null)
        {
            timer -= Time.deltaTime;
            obj.transform.position = startPos + Vector3.up * (0.8f - timer) * 1f;

            if (cam != null)
                obj.transform.rotation = Quaternion.LookRotation(
                    obj.transform.position - cam.transform.position);

            yield return null;
        }

        if (obj != null) Destroy(obj);
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

    float attackCooldown = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.Log("Agent is not set in zombie!");
        }
    }

    void Update()
    {
        if (isDead) return;
        if (attackCooldown > 0) attackCooldown -= Time.deltaTime;

        // Distance-based damage — find ALL players by tag and by PlayerHealth
        if (attackCooldown <= 0)
        {
            // Find all PlayerHealth components in scene
            var allHealth = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
            foreach (var health in allHealth)
            {
                if (health == null) continue;
                float dist = Vector3.Distance(transform.position, health.transform.position);
                if (dist < 2.5f && !health.IsInvincible())
                {
                    health.TakeDamage();
                    attackCooldown = 2f;
                    Debug.Log("Zombie attacked player! Distance: " + dist);
                    return;
                }
            }

            // Also try tagged players without PlayerHealth yet
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in players)
            {
                float dist = Vector3.Distance(transform.position, p.transform.position);
                if (dist < 2.5f)
                {
                    // Try PlayerScript (Jonathan's version)
                    var ps = p.GetComponent<PlayerScript>();
                    if (ps != null)
                    {
                        ps.take_damage(50);
                        attackCooldown = 2f;
                        HapticFeedback.VibrateHit();
                        return;
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        DamagePlayer(other.transform);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        DamagePlayer(collision.transform);
    }

    void DamagePlayer(Transform other)
    {
        // Check if we hit a player
        var health = other.GetComponent<PlayerHealth>();
        if (health == null) health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) health = other.root.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage();
        }
    }
}
