using UnityEngine;

public class ShieldOrbSpawner : MonoBehaviour
{
    public GameObject orbPrefab;

    [Header("Timing")]
    public float minDelay = 7f;
    public float maxDelay = 14f;

    [Header("Reference")]
    public Transform reference;

    [Header("Spawn Area Around Camera")]
    public float spawnAhead = 14f;
    public float spawnBehind = 4f;
    public float minY = -3.5f;
    public float maxY = 4.5f;

    [Header("Spawn Sides")]
    public bool spawnFromTop = true;
    public bool spawnFromRight = true;
    public bool spawnFromBottom = false;
    public bool spawnFromLeft = false;

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
    float randomX = reference.position.x + Random.Range(10f, 18f);
    float topY = maxY + 3f;

    Vector3 pos = new Vector3(randomX, topY, 0f);

    currentOrb = Instantiate(orbPrefab, pos, Quaternion.identity);

    ShieldOrbPowerUp orb = currentOrb.GetComponent<ShieldOrbPowerUp>();
    if (orb != null)
    {
        orb.reference = reference;
        orb.targetY = Random.Range(minY, maxY);
    }
}

    int GetRandomSide()
    {
        System.Collections.Generic.List<int> sides = new System.Collections.Generic.List<int>();

        if (spawnFromTop) sides.Add(0);
        if (spawnFromRight) sides.Add(1);
        if (spawnFromBottom) sides.Add(2);
        if (spawnFromLeft) sides.Add(3);

        if (sides.Count == 0) return 1;

        return sides[Random.Range(0, sides.Count)];
    }
}