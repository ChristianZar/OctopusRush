using UnityEngine;

public class JellyfishSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject jellyfishPrefab;

    [Header("How far ahead of the player the group starts")]
    public float spawnAhead = 10f;

    [Header("Distance between groups")]
    public float groupDistance = 12f;

    [Header("Group size")]
    public int minCount = 2;
    public int maxCount = 5;
    public float xSpacing = 1.6f;

    [Header("Vertical range")]
    public float minY = -3f;
    public float maxY = 3f;

    private Transform player;
    private float nextSpawnX;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        nextSpawnX = player.position.x + spawnAhead;
    }

    void Update()
    {
        if (player == null || jellyfishPrefab == null) return;

        if (player.position.x + spawnAhead >= nextSpawnX)
        {
            float startX = Mathf.Max(nextSpawnX, player.position.x + spawnAhead);
            SpawnGroup(startX);
            nextSpawnX = startX + groupDistance;
        }
    }

    void SpawnGroup(float startX)
    {
        int count = Random.Range(minCount, maxCount + 1);
        float baseY = Random.Range(minY, maxY);

        int pattern = Random.Range(0, 3); // 0=line, 1=arch, 2=V

        if (pattern == 0)
            SpawnLine(startX, baseY, count);
        else if (pattern == 1)
            SpawnArch(startX, baseY, count, archHeight: 1.5f);
        else
        {
            if (count % 2 == 0) count += 1;
            SpawnVShape(startX, baseY, count, depth: 1.0f);
        }
    }

    void SpawnJellyfish(float x, float y)
    {
        GameObject jelly = Instantiate(jellyfishPrefab, new Vector3(x, y, 0f), Quaternion.identity);

        var script = jelly.GetComponent<JellyfishDecoration>();
        if (script != null)
            script.bobOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void SpawnLine(float startX, float baseY, int count)
    {
        for (int i = 0; i < count; i++)
            SpawnJellyfish(startX + i * xSpacing, baseY);
    }

    void SpawnArch(float startX, float baseY, int count, float archHeight = 1.5f)
    {
        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0.5f : (float)i / (count - 1);
            float x = startX + i * xSpacing;
            float y = baseY + Mathf.Sin(t * Mathf.PI) * archHeight;
            SpawnJellyfish(x, Mathf.Clamp(y, minY, maxY));
        }
    }

    void SpawnVShape(float startX, float baseY, int count, float depth = 1.2f)
    {
        int mid = count / 2;
        for (int i = 0; i < count; i++)
        {
            float x = startX + i * xSpacing;
            float distFromMid = Mathf.Abs(i - mid);
            float y = baseY - (1f - (distFromMid / mid)) * depth;
            SpawnJellyfish(x, Mathf.Clamp(y, minY, maxY));
        }
    }
}