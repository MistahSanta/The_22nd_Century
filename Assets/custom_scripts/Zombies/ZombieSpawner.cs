using Fusion;
using UnityEngine;

public class ZombieSpawner : NetworkBehaviour
{
    [SerializeField] NetworkObject zombiePrefab;
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] int maxZombies = 10;

    float spawnTimer = 0f;

    [Networked] int currentZombieCount { get; set; }

    public override void FixedUpdateNetwork()
    {
        Debug.Log($"HasStateAuthority: {HasStateAuthority}, World state: {GameManager.Instance?.NetworkedWorldState}");

        if (!HasStateAuthority) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsInPresent) return;
        Debug.Log($"Passed IsInPresent check, pct: {GameManager.Instance.CleanlinessPercent}");

        float pct = GameManager.Instance.CleanlinessPercent;

        int maxAllowed = GetMaxZombiesBasedOnCleanliness();
        Debug.Log($"pct: {pct}, maxAllowed: {maxAllowed}, currentCount: {currentZombieCount}");

        spawnTimer += Runner.DeltaTime;
        if (spawnTimer >= spawnInterval && currentZombieCount < maxAllowed)
        {
            spawnTimer = 0f;
            SpawnZombie();
        }
    }

    int GetMaxZombiesBasedOnCleanliness()
    {
        if (GameManager.Instance.IsInPresent) return 0;

        float pct = GameManager.Instance.CleanlinessPercent;

        if (pct >= 100f) return 0;  // Very Clean - No zombies at all
        else if (pct >= 60f) return 3;
        else if (pct >= 30f) return 6;
        else return maxZombies;  // Apocalypse - Full of zombies
    }

    void SpawnZombie()
    {
        Transform player = LocalPlayerHolder.GetLocalCamera();
        if (player == null) return;

        Vector2 randomCircle = Random.insideUnitCircle.normalized * 15f;
        Vector3 spawnPos = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            spawnPos = hit.position;
        else
            return;

        Runner.Spawn(zombiePrefab, spawnPos, Quaternion.identity);
        currentZombieCount++;
    }

    public void ZombieDied()
    {
        currentZombieCount--;
        if (currentZombieCount < 0) currentZombieCount = 0;
    }

    public void ResetZombieCount()
    {
        currentZombieCount = 0;
    }
}