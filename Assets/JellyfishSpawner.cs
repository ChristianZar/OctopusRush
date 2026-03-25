using UnityEngine;

public class JellyfishSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject jellyfishPrefab;

    [Header("How far ahead of the player the group starts")]
    public float spawnAhead = 10f;

    [Header("Distance between groups")]
    public float easyGroupDistance = 16f;
    public float hardGroupDistance = 8f;

    [Header("Group size")]
    public int easyMinCount = 2;
    public int easyMaxCount = 3;
    public int hardMinCount = 3;
    public int hardMaxCount = 6;
    public float xSpacing = 1.6f;

    [Header("Vertical range")]
    public float minY = -3f;
    public float maxY = 3f;

    [Header("Difficulty Ramp")]
    public float easyDistance   = 60f;
    public float rampEndDistance = 250f;

    private Transform player;
    private float nextSpawnX;
    private float startX;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startX = player.position.x;
        nextSpawnX = player.position.x + spawnAhead;
    }

    void Update()
    {
        if (player == null || jellyfishPrefab == null) return;

        float t = DifficultyT();
        float groupDistance = Mathf.Lerp(easyGroupDistance, hardGroupDistance, t);

        if (player.position.x + spawnAhead >= nextSpawnX)
        {
            float gx = Mathf.Max(nextSpawnX, player.position.x + spawnAhead);
            SpawnGroup(gx, t);
            nextSpawnX = gx + groupDistance;
        }
    }

    float DifficultyT()
    {
        if (player == null) return 0f;
        float dist = player.position.x - startX;
        float t = Mathf.InverseLerp(easyDistance, rampEndDistance, dist);
        return Mathf.SmoothStep(0f, 1f, t);
    }

    void SpawnGroup(float gx, float t)
    {
        int minC = Mathf.RoundToInt(Mathf.Lerp(easyMinCount, hardMinCount, t));
        int maxC = Mathf.RoundToInt(Mathf.Lerp(easyMaxCount, hardMaxCount, t));
        int count = Random.Range(minC, maxC + 1);
        float baseY = Random.Range(minY, maxY);

        int pattern = Random.Range(0, 3); // 0=line, 1=arch, 2=V

        if (pattern == 0)
            SpawnLine(gx, baseY, count);
        else if (pattern == 1)
            SpawnArch(gx, baseY, count, archHeight: 1.5f);
        else
        {
            if (count % 2 == 0) count += 1;
            SpawnVShape(gx, baseY, count, depth: 1.0f);
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