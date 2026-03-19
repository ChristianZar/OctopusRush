using System.Collections.Generic;
using UnityEngine;

public class PufferSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject pufferPrefab;

    [Header("Camera")]
    public Camera cam;

    [Header("Spawn Timing")]
    public float spawnInterval = 4f;
    public int maxPuffersAlive = 5;

    [Header("Spawn Area")]
    public float spawnOffsetX = 2f;
    public float minYViewport = 0.2f;
    public float maxYViewport = 0.8f;

    [Header("Cleanup")]
    public float destroyBehindX = 4f;

    private float timer;
    private List<GameObject> pufferList = new List<GameObject>();

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        timer = spawnInterval;
    }

    void Update()
    {
        pufferList.RemoveAll(p => p == null);

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (pufferList.Count < maxPuffersAlive)
            {
                SpawnPuffer();
            }

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