using UnityEngine;

public class TreasureChestSpawner : MonoBehaviour
{
    public GameObject treasureChestPrefab;

    [Header("Spawn Settings")]
    public float spawnAhead = 10f;
    public float minY = -2f;
    public float maxY = 2f;

    [Header("Despawn Settings")]
    public float despawnBehind = 15f;

    private Camera cam;
    private KeyBarUI keyBar;

    private GameObject currentChest;
    private bool chestOpenedThisCycle = false;

    void Start()
    {
        cam = Camera.main;
        keyBar = FindFirstObjectByType<KeyBarUI>();
    }

    void Update()
    {
        if (cam == null || keyBar == null || treasureChestPrefab == null)
            return;

        float camLeft = cam.transform.position.x - cam.orthographicSize * cam.aspect;

        if (currentChest != null && currentChest.transform.position.x < camLeft - despawnBehind)
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
        float rightEdge = cam.transform.position.x + cam.orthographicSize * cam.aspect;
        Vector3 spawnPos = new Vector3(rightEdge + spawnAhead, y, 0);

        currentChest = Instantiate(treasureChestPrefab, spawnPos, Quaternion.identity);

        TreasureChest chest = currentChest.GetComponent<TreasureChest>();
        if (chest != null)
            chest.spawner = this;
    }

    public void NotifyChestOpened()
    {
        chestOpenedThisCycle = true;

        if (keyBar != null)
            keyBar.ResetKeys();

        currentChest = null;
    }

    public void NotifyChestDespawned(TreasureChest chest)
    {
        if (currentChest != null && chest != null && chest.gameObject == currentChest)
            currentChest = null;
    }
}
