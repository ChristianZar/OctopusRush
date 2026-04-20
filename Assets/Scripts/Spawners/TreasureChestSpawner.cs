using UnityEngine;

public class TreasureChestSpawner : MonoBehaviour
{
    public GameObject treasureChestPrefab;

    [Header("Spawn Settings")]
    public float spawnAhead = 10f;
    public float minY = -2f;
    public float maxY = 2f;

    private Transform player;
    private KeyBarUI keyBar;

    private GameObject currentChest;          // track the chest
    private bool chestOpenedThisCycle = false; // stop spawning after opened until keys fill again
    [Header("Despawn Settings")]
public float despawnBehind = 15f;  // how far behind player before we remove chest

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        keyBar = FindFirstObjectByType<KeyBarUI>();
    }

void Update()
{
    if (player == null || keyBar == null || treasureChestPrefab == null)
        return;

    // ✅ Despawn if player passed it too far
    if (currentChest != null && currentChest.transform.position.x < player.position.x - despawnBehind)
    {
        Destroy(currentChest);
        currentChest = null;
    }

    if (!keyBar.IsFull())
    {
        chestOpenedThisCycle = false;
        return;
    }

    if (!chestOpenedThisCycle && (currentChest == null || !currentChest))
    {
        SpawnChest();
    }
}

    void SpawnChest()
    {
        float y = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(player.position.x + spawnAhead, y, 0);

        currentChest = Instantiate(treasureChestPrefab, spawnPos, Quaternion.identity);

        // Connect chest back to this spawner so it can notify on open
        TreasureChest chest = currentChest.GetComponent<TreasureChest>();
        if (chest != null)
        {
            chest.spawner = this;
        }
    }

    // ✅ THIS is what your TreasureChest.cs is trying to call
    public void NotifyChestOpened()
    {
        chestOpenedThisCycle = true;

        // Chest opened => bar resets, and no more chest spawns until bar fills again
        if (keyBar != null)
            keyBar.ResetKeys();

        currentChest = null;
    }

    public void NotifyChestDespawned(TreasureChest chest)
{
    // Only clear if this is the chest we were tracking
    if (currentChest != null && chest != null && chest.gameObject == currentChest)
    {
        currentChest = null;
    }
}
}
