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
    private bool spawnedThisFull = false;

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

        // Spawn ONE chest when bar becomes full
        if (keyBar.IsFull() && !spawnedThisFull)
        {
            float y = Random.Range(minY, maxY);
            Vector3 spawnPos = new Vector3(
                player.position.x + spawnAhead,
                y,
                0
            );

            Instantiate(treasureChestPrefab, spawnPos, Quaternion.identity);
            spawnedThisFull = true;
        }

        // Allow another chest after keys are spent
        if (!keyBar.IsFull())
        {
            spawnedThisFull = false;
        }
    }
}
