using System.Collections.Generic;
using UnityEngine;

public class ShrimpSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject shrimpPrefab;

    [Header("Camera")]
    public Camera cam;

    [Header("Spawn Timing")]
    public float easySpawnInterval = 5f;
    public float hardSpawnInterval = 1.5f;
    public int easyMaxAlive = 4;
    public int hardMaxAlive = 12;

    [Header("Difficulty Ramp")]
    public float easyDistance    = 60f;
    public float rampEndDistance = 250f;

    [Header("Spawn Position")]
    public float spawnOffsetX = 2f;
    public float minYViewport = 0.2f;
    public float maxYViewport = 0.8f;

    [Header("Cleanup")]
    public float destroyBehindX = 4f;

    private float timer;
    private float startX;
    private Transform playerT;
    private List<GameObject> shrimpList = new List<GameObject>();

    void Start()
    {
        if (cam == null) cam = Camera.main;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { playerT = p.transform; startX = p.transform.position.x; }

        timer = easySpawnInterval;
    }

    float DifficultyT()
    {
        if (playerT == null) return 0f;
        float t = Mathf.InverseLerp(easyDistance, rampEndDistance, playerT.position.x - startX);
        return Mathf.SmoothStep(0f, 1f, t);
    }

    void Update()
    {
        shrimpList.RemoveAll(s => s == null);

        float t = DifficultyT();
        float spawnInterval  = Mathf.Lerp(easySpawnInterval, hardSpawnInterval, t);
        int   maxShrimpAlive = Mathf.RoundToInt(Mathf.Lerp(easyMaxAlive, hardMaxAlive, t));

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (shrimpList.Count < maxShrimpAlive)
                SpawnShrimp();

            timer = spawnInterval;
        }

        CleanupOldShrimp();
    }

    void SpawnShrimp()
    {
        if (shrimpPrefab == null || cam == null) return;

        Vector3 spawnPoint = cam.ViewportToWorldPoint(
            new Vector3(1f, Random.Range(minYViewport, maxYViewport), Mathf.Abs(cam.transform.position.z))
        );

        spawnPoint.x += spawnOffsetX;
        spawnPoint.z = 0f;

        GameObject newShrimp = Instantiate(shrimpPrefab, spawnPoint, Quaternion.identity);
        shrimpList.Add(newShrimp);
    }

    void CleanupOldShrimp()
    {
        if (cam == null) return;

        Vector3 leftEdge = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, Mathf.Abs(cam.transform.position.z))
        );

        float destroyX = leftEdge.x - destroyBehindX;

        for (int i = shrimpList.Count - 1; i >= 0; i--)
        {
            if (shrimpList[i] == null) continue;

            if (shrimpList[i].transform.position.x < destroyX)
            {
                Destroy(shrimpList[i]);
                shrimpList.RemoveAt(i);
            }
        }
    }
}