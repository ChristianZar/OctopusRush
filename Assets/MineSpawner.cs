using System.Collections.Generic;
using UnityEngine;

public class MineSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject minePrefab;

    [Header("Camera")]
    public Camera cam;

    [Header("Spawn Timing")]
    public float minInterval   = 5f;
    public float maxInterval   = 10f;
    public int   maxClusters   = 3;     // max simultaneous clusters alive

    [Header("Cluster Size")]
    public int   minClusterSize = 3;
    public int   maxClusterSize = 4;

    [Header("Cluster Scatter")]
    public float spreadX    = 1.8f;  // max horizontal scatter from cluster centre
    public float spreadY    = 1.2f;  // max vertical scatter from ceiling anchor
    public float minGap     = 0.8f;  // minimum distance between any two mines

    [Header("Spawn Position")]
    public float ceilingOffsetY = 0.8f; // world units below the top edge of the screen
    public float spawnAheadX    = 2f;   // world units past the right edge

    [Header("Cleanup")]
    public float destroyBehindX = 4f;

    private float            timer;
    private List<List<MineBehavior>> clusters = new List<List<MineBehavior>>();

    void Start()
    {
        if (cam == null) cam = Camera.main;
        timer = Random.Range(minInterval, maxInterval);
    }

    void Update()
    {
        // Remove clusters where all mines are gone
        clusters.RemoveAll(c => c.TrueForAll(m => m == null));

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (clusters.Count < maxClusters)
                SpawnCluster();
            timer = Random.Range(minInterval, maxInterval);
        }

        CleanupOldMines();
    }

    void SpawnCluster()
    {
        if (minePrefab == null || cam == null) return;

        int   count   = Random.Range(minClusterSize, maxClusterSize + 1);
        float anchorY = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y - ceilingOffsetY;
        float anchorX = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x + spawnAheadX;

        List<Vector2>      positions = new List<Vector2>();
        List<MineBehavior> cluster   = new List<MineBehavior>();

        // Place each mine at a random scattered position, retry if too close to existing ones
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = Vector2.zero;
            bool    ok  = false;

            for (int attempt = 0; attempt < 20; attempt++)
            {
                pos = new Vector2(
                    anchorX + Random.Range(-spreadX, spreadX),
                    anchorY - Random.Range(0f, spreadY)   // always below ceiling
                );

                bool tooClose = false;
                foreach (Vector2 existing in positions)
                {
                    if (Vector2.Distance(pos, existing) < minGap)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose) { ok = true; break; }
            }

            if (!ok) continue; // skip if no valid spot found after retries

            positions.Add(pos);
            GameObject go = Instantiate(minePrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            MineBehavior mb = go.GetComponent<MineBehavior>();
            if (mb != null)
                cluster.Add(mb);
        }

        // Link every mine to all others in the cluster (excluding itself)
        foreach (MineBehavior mine in cluster)
            foreach (MineBehavior other in cluster)
                if (other != mine)
                    mine.linkedMines.Add(other);

        clusters.Add(cluster);
    }

    void CleanupOldMines()
    {
        if (cam == null) return;

        float leftX = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x - destroyBehindX;

        foreach (List<MineBehavior> cluster in clusters)
        {
            foreach (MineBehavior mine in cluster)
            {
                if (mine != null && mine.transform.position.x < leftX)
                    Destroy(mine.gameObject);
            }
        }
    }
}
