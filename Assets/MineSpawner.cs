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

    [Header("Chain Layout")]
    public float linkDistance   = 1.3f;  // distance between consecutive mines in the chain
    public float linkJitter     = 0.25f; // random position noise per mine
    public float chainAngleRange = 55f;  // max tilt of the chain from horizontal (degrees)

    [Header("Spawn Position")]
    public float ceilingOffsetY = 0.8f; // world units below the top edge of the screen
    public float spawnAheadX    = 2f;   // world units past the right edge
    public float minY           = -3.5f;// lowest Y a mine can spawn

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
        float camZ    = Mathf.Abs(cam.transform.position.z);
        float topY    = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, camZ)).y - ceilingOffsetY;
        float rightX  = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, camZ)).x + spawnAheadX;

        // Chain starts near the top-right, slight X jitter
        Vector2 chainStart = new Vector2(
            rightX + Random.Range(-0.5f, 0.5f),
            topY   - Random.Range(0f, 0.6f)
        );

        // Chain direction: tilted at a random angle so mines hang diagonally
        float   angle = Random.Range(-chainAngleRange, chainAngleRange) * Mathf.Deg2Rad;
        Vector2 step  = new Vector2(Mathf.Cos(angle), -Mathf.Abs(Mathf.Sin(angle))) * linkDistance;

        List<MineBehavior> cluster = new List<MineBehavior>();

        for (int i = 0; i < count; i++)
        {
            Vector2 jitter = new Vector2(
                Random.Range(-linkJitter, linkJitter),
                Random.Range(-linkJitter, linkJitter)
            );
            Vector2 pos = chainStart + step * i + jitter;

            // Keep mines above a minimum Y
            pos.y = Mathf.Max(pos.y, minY);

            GameObject go = Instantiate(minePrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            MineBehavior mb = go.GetComponent<MineBehavior>();
            if (mb != null)
                cluster.Add(mb);
        }

        // Chain-detonate: each mine triggers the next (not all at once)
        for (int i = 0; i < cluster.Count; i++)
            for (int j = 0; j < cluster.Count; j++)
                if (i != j)
                    cluster[i].linkedMines.Add(cluster[j]);

        // Spawn chain visual connecting the mines in order
        if (cluster.Count > 1)
        {
            GameObject chainGO = new GameObject("MineChain");
            MineChain  chain   = chainGO.AddComponent<MineChain>();
            chain.Init(cluster);
        }

        clusters.Add(cluster);
    }

    void CleanupOldMines()
    {
        if (cam == null) return;

        float leftX = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, Mathf.Abs(cam.transform.position.z))).x - destroyBehindX;

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
