using UnityEngine;

public class ShieldOrbSpawner : MonoBehaviour
{
    public GameObject orbPrefab;

    [Header("Timing")]
    public float minDelay = 7f;
    public float maxDelay = 14f;

    [Header("Position")]
    public Transform reference;
    public float spawnAhead = 12f;
    public float minY = -1.5f;
    public float maxY = 2.0f;

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

        // Clear destroyed reference
        if (currentOrb == null)
        {
            timer += Time.deltaTime;

            if (timer >= next)
            {
                Spawn();
                RollNext();
            }
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