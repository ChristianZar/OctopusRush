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

        // If bar is NOT full, reset cycle so chest can spawn next time it becomes full
        if (!keyBar.IsFull())
        {
            chestOpenedThisCycle = false;
            return;
        }

        // Bar IS full:
        // If chest isn't opened yet this cycle, ensure one chest exists (respawn if missing)
        if (!chestOpenedThisCycle && currentChest == null)
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
}
