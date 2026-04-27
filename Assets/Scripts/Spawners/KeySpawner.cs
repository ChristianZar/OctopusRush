using UnityEngine;

public class KeySpawner : MonoBehaviour
{
    public GameObject keyPrefab;

    [Header("How far ahead of the camera right-edge the pattern starts")]
    public float spawnAhead = 4f;

    [Header("Distance between patterns")]
    public float patternDistance = 8f;

    [Header("Key pattern")]
    public int minCount = 3;
    public int maxCount = 7;
    public float spacing = 0.8f;

    [Header("Vertical range")]
    public float minY = -2f;
    public float maxY = 2f;

    private Camera cam;
    private float nextSpawnX;

    void Start()
    {
        cam = Camera.main;

        // Start spawning just off the right edge of the camera
        float rightEdge = CamRightEdge();
        nextSpawnX = rightEdge + spawnAhead;
    }

    void Update()
    {
        if (cam == null || keyPrefab == null) return;

        float rightEdge = CamRightEdge();

        if (rightEdge + spawnAhead >= nextSpawnX)
        {
            float startX = Mathf.Max(nextSpawnX, rightEdge + spawnAhead);
            SpawnPattern(startX);
            nextSpawnX = startX + patternDistance;
        }
    }

    float CamRightEdge()
    {
        return cam.transform.position.x + cam.orthographicSize * cam.aspect;
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
            float t = (count == 1) ? 0.5f : (float)i / (count - 1);
            float x = startX + i * spacing;
            float y = baseY + Mathf.Sin(t * Mathf.PI) * archHeight;
            SpawnKey(x, Mathf.Clamp(y, minY, maxY));
        }
    }

    void SpawnVShape(float startX, float baseY, int count, float depth = 1.2f)
    {
        int mid = count / 2;
        for (int i = 0; i < count; i++)
        {
            float x = startX + i * spacing;
            float distFromMid = Mathf.Abs(i - mid);
            float y = baseY - (1f - (distFromMid / mid)) * depth;
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
