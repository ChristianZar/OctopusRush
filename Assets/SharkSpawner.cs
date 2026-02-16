using UnityEngine;

public class SharkSpawner : MonoBehaviour
{
    public GameObject sharkPrefab;

    [Header("Target to spawn ahead of (Player or Camera)")]
    public Transform target;          // drag Player_Octopus or Main Camera here

    public float spawnAhead = 20f;    // how far in front of target
    public float minY = -1.5f;
    public float maxY = 2f;

    public float minDistance = 30f;    // spacing between sharks
    public float spawnInterval = 15f;

    private float lastSpawnX;

    void Start()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        // Start spawning a little ahead of the target
        float baseX = (target != null) ? target.position.x : transform.position.x;
        lastSpawnX = baseX + spawnAhead;

        InvokeRepeating(nameof(SpawnShark), 1f, spawnInterval);
    }

    void SpawnShark()
    {
        if (sharkPrefab == null || target == null) return;

        // where "in front" is right now
        float desiredX = target.position.x + spawnAhead;

        // enforce spacing (don't spawn too close)
        if (desiredX < lastSpawnX + minDistance)
            return;

        float spawnX = desiredX;
        float spawnY = Random.Range(minY, maxY);

        Vector3 pos = new Vector3(spawnX, spawnY, 0f);
        GameObject shark = Instantiate(sharkPrefab, pos, Quaternion.identity);

if (shark.transform.position.x > target.position.x)
{
    shark.transform.localScale = new Vector3(-1, 1, 1);
}


        lastSpawnX = spawnX;
    }
}
