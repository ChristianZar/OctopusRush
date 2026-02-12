using UnityEngine;

public class FloorCrabSpawner : MonoBehaviour
{
    [Header("Crab Prefab")]
    public GameObject crabPrefab;

    [Header("Spawn Range (Left/Right points on this floor tile)")]
    public Transform spawnLeft;
    public Transform spawnRight;

    [Header("How many crabs per tile")]
    public int minCrabs = 0;
    public int maxCrabs = 2;

    [Header("Spawn Tweaks")]
    public float edgePadding = 0.8f;   // keeps crabs away from floor seams/gaps
    public float spawnYOffset = 0.25f; // spawns slightly above the floor

    private bool hasSpawned = false;

    void OnEnable()
    {
        // When a tile is enabled/recycled, spawn once
        SpawnCrabs();
    }

    public void SpawnCrabs()
    {
        if (hasSpawned) return;
        if (crabPrefab == null || spawnLeft == null || spawnRight == null) return;

        // Prevent invalid ranges
        float leftX = spawnLeft.position.x + edgePadding;
        float rightX = spawnRight.position.x - edgePadding;
        if (rightX <= leftX) return;

        int count = Random.Range(minCrabs, maxCrabs + 1);

        for (int i = 0; i < count; i++)
        {
            float xPos = Random.Range(leftX, rightX);
            Vector3 spawnPos = new Vector3(xPos, spawnLeft.position.y + spawnYOffset, 0f);

            // IMPORTANT: do NOT parent to the floor tile (prevents teleporting)
            GameObject crab = Instantiate(crabPrefab, spawnPos, Quaternion.identity);

            // IMPORTANT: assign patrol points so every crab moves
            CrabPatrol patrol = crab.GetComponent<CrabPatrol>();
            if (patrol != null)
            {
                patrol.leftPoint = spawnLeft;
                patrol.rightPoint = spawnRight;
            }
        }

        hasSpawned = true;
    }

    public void ResetSpawner()
    {
        hasSpawned = false;
    }
}
