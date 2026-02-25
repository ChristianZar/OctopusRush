using UnityEngine;

public class ShieldOrbSpawner : MonoBehaviour
{
    public GameObject orbPrefab;

    [Header("Timing")]
    public float minDelay = 7f;
    public float maxDelay = 14f;

    [Header("Position")]
    public Transform reference;     // Main Camera
    public float spawnAhead = 12f;  // how far ahead of camera
    public float minY = -1.5f;      // playable area
    public float maxY = 2.0f;       // playable area

    private float timer;
    private float next;
    private GameObject currentOrb;

    void Start()
    {
        if (reference == null && Camera.main != null)
            reference = Camera.main.transform;

        RollNext();
    }

    void Update()
    {
        if (orbPrefab == null || reference == null) return;

        // only one orb at a time
        if (currentOrb != null) return;

        timer += Time.deltaTime;
        if (timer >= next)
        {
            Spawn();
            RollNext();
        }
    }

    void RollNext()
    {
        timer = 0f;
        next = Random.Range(minDelay, maxDelay);
    }

    void Spawn()
    {
        float targetY = Random.Range(minY, maxY);
        Vector3 pos = new Vector3(reference.position.x + spawnAhead, targetY, 0f);

        currentOrb = Instantiate(orbPrefab, pos, Quaternion.identity);
    }
}