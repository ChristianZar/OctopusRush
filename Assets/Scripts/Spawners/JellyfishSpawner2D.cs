using UnityEngine;

public class JellyfishSpawner2D : MonoBehaviour
{
    [Header("Prefab + Camera")]
    public GameObject jellyfishPrefab;
    public Camera cam;

    [Header("Spawn control")]
    public float spawnEverySeconds = 1.5f;
    public int maxAlive = 20;

    [Header("Spawn position (viewport)")]
    public float spawnRightMin = 1.05f;
    public float spawnRightMax = 1.25f;
    public float spawnYMin = 0.15f;
    public float spawnYMax = 0.85f;

    [Header("Despawn when behind camera")]
    public float despawnLeftViewportX = -0.30f;

    private float timer;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (jellyfishPrefab == null) return;
        if (cam == null) return;

        DespawnBehindCamera();

        timer += Time.deltaTime;
        if (timer >= spawnEverySeconds && transform.childCount < maxAlive)
        {
            timer = 0f;
            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        float z = -cam.transform.position.z; // 2D camera usually at z = -10

        float vx = Random.Range(spawnRightMin, spawnRightMax);
        float vy = Random.Range(spawnYMin, spawnYMax);

        Vector3 pos = cam.ViewportToWorldPoint(new Vector3(vx, vy, z));
        pos.z = 0f;

        Instantiate(jellyfishPrefab, pos, Quaternion.identity, transform);
    }

    private void DespawnBehindCamera()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform t = transform.GetChild(i);
            Vector3 v = cam.WorldToViewportPoint(t.position);

            if (v.x < despawnLeftViewportX)
                Destroy(t.gameObject);
        }
    }
}