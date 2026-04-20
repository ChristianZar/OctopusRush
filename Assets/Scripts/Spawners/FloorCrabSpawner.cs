using UnityEngine;

public class FloorCrabSpawner : MonoBehaviour
{
    [Header("Crab Prefab")]
    public GameObject crabPrefab;

    [Header("Spawn Range (Left/Right points on this floor tile)")]
    public Transform spawnLeft;
    public Transform spawnRight;

    [Header("How many crabs per tile (fallback if no player)")]
    public int minCrabs = 0;
    public int maxCrabs = 2;

    [Header("Spawn Tweaks")]
    public float edgePadding = 0.8f;   // keeps crabs away from floor seams/gaps
    public float spawnYOffset = 0.25f; // spawns slightly above the floor

    [Header("Difficulty by Distance (units ~= meters)")]
    public Transform player;              // drag Player here (or it auto-finds by tag)
    public float startDistance = 80f;     // crabs begin after this distance
    public float rampEndDistance = 260f;  // by this distance, can reach lateMax
    public int earlyMax = 2;              // early: up to 2 per tile
    public int lateMax = 5;               // late: up to 5 per tile
    public bool allowZeroEarly = true;    // if true: 0..max, else: 1..max (after start)

    private bool hasSpawned = false;
    private float startX = float.NaN;

    void OnEnable()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Set baseline X once (first time we have a player)
        if (player != null && float.IsNaN(startX))
            startX = player.position.x;

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

        // ---- Distance-based count ----
        int dynamicMin = minCrabs;
        int dynamicMax = maxCrabs;

        if (player != null && !float.IsNaN(startX))
        {
            float dist = player.position.x - startX;

            if (dist < startDistance)
            {
                // Before unlock distance: no crabs
                dynamicMin = 0;
                dynamicMax = 0;
            }
            else
            {
                // Ramp difficulty 0->1 from startDistance to rampEndDistance
                float t = Mathf.InverseLerp(startDistance, rampEndDistance, dist);
                t = Mathf.SmoothStep(0f, 1f, t);

                dynamicMax = Mathf.RoundToInt(Mathf.Lerp(earlyMax, lateMax, t));
                dynamicMax = Mathf.Clamp(dynamicMax, 0, lateMax);

                dynamicMin = allowZeroEarly ? 0 : 1;
                dynamicMin = Mathf.Clamp(dynamicMin, 0, dynamicMax);
            }
        }
        else
        {
            // fallback safety clamp
            dynamicMin = Mathf.Max(0, dynamicMin);
            dynamicMax = Mathf.Max(dynamicMin, dynamicMax);
        }

        int count = Random.Range(dynamicMin, dynamicMax + 1);

        // ---- Spawn crabs ----
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