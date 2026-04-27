using UnityEngine;
using System.Collections.Generic;

public class AnglerFishSpawner : MonoBehaviour
{
    public GameObject anglerFishPrefab;
    public Camera cam;

    [Header("Spawn Timing")]
    public float spawnEverySeconds = 3f;

    [Header("Spawn X Range")]
    public float minSpawnOffsetX = 8f;
    public float maxSpawnOffsetX = 11f;

    [Header("Spawn Y")]
    public float spawnY = -4f;

    [Header("Cleanup")]
    public float destroyBehindX = 8f;

    private float timer;
    private List<GameObject> anglers = new List<GameObject>();

    void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (anglerFishPrefab == null || cam == null) return;

        timer += Time.deltaTime;
        if (timer < spawnEverySeconds) return;
        timer = 0f;

        float camZ = Mathf.Abs(cam.transform.position.z);
        float rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, camZ)).x;

        float spawnOffsetX = Random.Range(minSpawnOffsetX, maxSpawnOffsetX);
        Vector3 pos = new Vector3(rightEdge + spawnOffsetX, spawnY, 0f);

        GameObject angler = Instantiate(anglerFishPrefab, pos, Quaternion.identity);
        anglers.Add(angler);

        CleanupOldAnglers();
    }

    void CleanupOldAnglers()
    {
        float leftX = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, Mathf.Abs(cam.transform.position.z))).x - destroyBehindX;

        for (int i = anglers.Count - 1; i >= 0; i--)
        {
            if (anglers[i] == null)
            {
                anglers.RemoveAt(i);
                continue;
            }

            if (anglers[i].transform.position.x < leftX)
            {
                Destroy(anglers[i]);
                anglers.RemoveAt(i);
            }
        }
    }
}