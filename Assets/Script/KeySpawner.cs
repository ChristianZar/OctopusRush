using UnityEngine;

public class KeySpawner : MonoBehaviour
{
    public GameObject keyPrefab;

    [Header("How far ahead of the player the pattern starts")]
    public float spawnAhead = 6f;

    [Header("Distance between patterns")]
    public float patternDistance = 8f;

    [Header("Key pattern")]
    public int minCount = 3;
    public int maxCount = 7;
    public float spacing = 0.8f;

    [Header("Vertical range")]
    public float minY = -2f;
    public float maxY = 2f;

    private Transform player;
    private float nextSpawnX;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // First pattern will appear ahead (not on player)
        nextSpawnX = player.position.x + spawnAhead;
    }

    void Update()
    {
        if (player == null || keyPrefab == null) return;

        // If player has reached the point where we should spawn the next pattern:
        if (player.position.x + spawnAhead >= nextSpawnX)
        {
            // Force spawn point to be ahead of the player (never behind)
            float startX = Mathf.Max(nextSpawnX, player.position.x + spawnAhead);

            SpawnPattern(startX);

            nextSpawnX = startX + patternDistance;
        }
    }

   void SpawnPattern(float startX)
{
    int count = Random.Range(minCount, maxCount + 1);
    float baseY = Random.Range(minY, maxY);

    int pattern = Random.Range(0, 3); // 0=line, 1=arch, 2=V

    if (pattern == 0)
    {
        SpawnLine(startX, baseY, count);
    }
    else if (pattern == 1)
    {
        SpawnArch(startX, baseY, count, archHeight: 1.5f);
    }
    else
    {
        // V looks best with odd counts
        if (count % 2 == 0) count += 1;
        SpawnVShape(startX, baseY, count, depth: 1.0f);
    }
}


    void SpawnKey(float x, float y)
    {
        Instantiate(keyPrefab, new Vector3(x, y, 0), Quaternion.identity);
    }

    void SpawnLine(float startX, float baseY, int count)
{
    for (int i = 0; i < count; i++)
        SpawnKey(startX + i * spacing, baseY);
}

void SpawnArch(float startX, float baseY, int count, float archHeight = 1.5f)
{
    for (int i = 0; i < count; i++)
    {
        float t = (count == 1) ? 0.5f : (float)i / (count - 1); // 0..1
        float x = startX + i * spacing;
        float y = baseY + Mathf.Sin(t * Mathf.PI) * archHeight; // smooth arch
        SpawnKey(x, Mathf.Clamp(y, minY, maxY));
    }
}

void SpawnVShape(float startX, float baseY, int count, float depth = 1.2f)
{
    // count should be odd (e.g., 5, 7) for a nice V
    int mid = count / 2;

    for (int i = 0; i < count; i++)
    {
        float x = startX + i * spacing;
        float distFromMid = Mathf.Abs(i - mid);
        float y = baseY - (1f - (distFromMid / mid)) * depth; // V dip in center
        SpawnKey(x, Mathf.Clamp(y, minY, maxY));
    }
}

void SpawnCircle(float centerX, float centerY, int count, float radius = 1.2f)
{
    for (int i = 0; i < count; i++)
    {
        float ang = (Mathf.PI * 2f) * (i / (float)count);
        float x = centerX + Mathf.Cos(ang) * radius;
        float y = centerY + Mathf.Sin(ang) * radius;
        SpawnKey(x, Mathf.Clamp(y, minY, maxY));
    }
}

}
