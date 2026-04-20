using System.Collections.Generic;
using UnityEngine;

public class PufferSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject pufferPrefab;

    [Header("Camera")]
    public Camera cam;

    [Header("Spawn Timing")]
    public float easySpawnInterval = 6f;
    public float hardSpawnInterval = 2.5f;
    public int easyMaxAlive = 3;
    public int hardMaxAlive = 7;

    [Header("Difficulty Ramp")]
    public float easyDistance    = 60f;
    public float rampEndDistance = 250f;

    [Header("Spawn Area")]
    public float spawnOffsetX = 2f;
    public float minYViewport = 0.2f;
    public float maxYViewport = 0.8f;

    [Header("Cleanup")]
    public float destroyBehindX = 4f;

    private float timer;
    private float startX;
    private Transform playerT;
    private List<GameObject> pufferList = new List<GameObject>();

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
        pufferList.RemoveAll(p => p == null);

        float t = DifficultyT();
        float spawnInterval = Mathf.Lerp(easySpawnInterval, hardSpawnInterval, t);
        int   maxPuffersAlive = Mathf.RoundToInt(Mathf.Lerp(easyMaxAlive, hardMaxAlive, t));

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (pufferList.Count < maxPuffersAlive)
                SpawnPuffer();

            timer = spawnInterval;
        }

        CleanupOldPuffers();
    }

    void SpawnPuffer()
    {
        if (pufferPrefab == null || cam == null) return;

        Vector3 spawnPoint = cam.ViewportToWorldPoint(
            new Vector3(1f, Random.Range(minYViewport, maxYViewport), Mathf.Abs(cam.transform.position.z))
        );

        spawnPoint.x += spawnOffsetX;
        spawnPoint.z = 0f;

        GameObject newPuffer = Instantiate(pufferPrefab, spawnPoint, Quaternion.identity);
        pufferList.Add(newPuffer);
    }

    void CleanupOldPuffers()
    {
        if (cam == null) return;

        Vector3 leftEdge = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, Mathf.Abs(cam.transform.position.z))
        );

        float destroyX = leftEdge.x - destroyBehindX;

        for (int i = pufferList.Count - 1; i >= 0; i--)
        {
            if (pufferList[i] == null) continue;

            if (pufferList[i].transform.position.x < destroyX)
            {
                Destroy(pufferList[i]);
                pufferList.RemoveAt(i);
            }
        }
    }
}