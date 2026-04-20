using UnityEngine;

public class SharkSpawner : MonoBehaviour
{
    public GameObject sharkPrefab;

    [Header("Camera (auto-found if empty)")]
    public Camera cam;

    [Header("Spawn Area")]
    public float spawnOffsetX = 3f;   // units past the RIGHT edge of the screen
    public float minY = -1.5f;
    public float maxY = 2f;

    [Header("Difficulty by Distance (units ~= meters)")]
    public float easyDistance = 60f;        // first 60 units = easy
    public float rampEndDistance = 250f;    // by 250 units = hardest

    [Header("Spacing (minDistance)")]
    public float easyMinDistance = 35f;
    public float hardMinDistance = 14f;

    [Header("Spawn Rate (seconds)")]
    public float easySpawnInterval = 6f;
    public float hardSpawnInterval = 2.0f;

    [Header("Cleanup")]
    public float destroyBehindX = 6f;

    private float lastSpawnX;
    private float startX;
    private float timer;
    private Transform player;
    private System.Collections.Generic.List<GameObject> sharks = new System.Collections.Generic.List<GameObject>();

    void Start()
    {
        if (cam == null) cam = Camera.main;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { player = p.transform; startX = p.transform.position.x; }

        float rightEdge = cam != null
            ? cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z))).x
            : 20f;
        lastSpawnX = rightEdge + spawnOffsetX;

        timer = 0f;
    }

    void Update()
    {
        if (sharkPrefab == null || cam == null) return;

        // ---- Difficulty scaling based on distance traveled ----
        float dist = player != null ? player.position.x - startX : 0f;

        // t = 0 (easy) until easyDistance, then ramps to 1 by rampEndDistance
        float t = Mathf.InverseLerp(easyDistance, rampEndDistance, dist);
        t = Mathf.SmoothStep(0f, 1f, t);

        float currentMinDistance = Mathf.Lerp(easyMinDistance, hardMinDistance, t);
        float currentSpawnInterval = Mathf.Lerp(easySpawnInterval, hardSpawnInterval, t);

        // ---- Timer-based spawning so interval can change ----
        timer += Time.deltaTime;
        if (timer < currentSpawnInterval) return;

        // try spawn, then reset timer (even if we didn't spawn, to avoid spam checks)
        timer = 0f;

        SpawnShark(currentMinDistance);
        CleanupOldSharks();
    }

    void SpawnShark(float currentMinDistance)
    {
        // Always spawn past the right edge of the camera
        float rightEdge = cam.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;
        float desiredX = rightEdge + spawnOffsetX;

        // enforce spacing
        if (desiredX < lastSpawnX + currentMinDistance)
            return;

        float spawnX = desiredX;
        float spawnY = Random.Range(minY, maxY);

        Vector3 pos = new Vector3(spawnX, spawnY, 0f);
        GameObject shark = Instantiate(sharkPrefab, pos, Quaternion.identity);

        // Always face left (toward player) since shark spawns to the right
        shark.transform.localScale = new Vector3(-1, 1, 1);

        sharks.Add(shark);
        lastSpawnX = spawnX;
    }

    void CleanupOldSharks()
    {
        if (cam == null) return;
        float leftX = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, Mathf.Abs(cam.transform.position.z))).x - destroyBehindX;

        for (int i = sharks.Count - 1; i >= 0; i--)
        {
            if (sharks[i] == null) { sharks.RemoveAt(i); continue; }
            if (sharks[i].transform.position.x < leftX)
            {
                Destroy(sharks[i]);
                sharks.RemoveAt(i);
            }
        }
    }
}