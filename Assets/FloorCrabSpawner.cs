using UnityEngine;

public class FloorCrabSpawner : MonoBehaviour
{
    public GameObject crabPrefab;
    public Transform spawnLeft;
    public Transform spawnRight;

    public int minCrabs = 0;
    public int maxCrabs = 2;

    GameObject[] spawned;

    void Start()
    {
        SpawnCrabs();
    }

    public void SpawnCrabs()
    {
        // Delete old crabs
        if (spawned != null)
        {
            for (int i = 0; i < spawned.Length; i++)
            {
                if (spawned[i] != null)
                    Destroy(spawned[i]);
            }
        }

        int count = Random.Range(minCrabs, maxCrabs + 1);
        spawned = new GameObject[count];

        for (int i = 0; i < count; i++)
{
    float t = Random.Range(0f, 1f);
    Vector3 pos = Vector3.Lerp(spawnLeft.position, spawnRight.position, t);

    GameObject crab = Instantiate(crabPrefab, pos, Quaternion.identity);
    spawned[i] = crab;

    // ✅ give the crab its patrol range on THIS floor
    var patrol = crab.GetComponent<CrabPatrol>();
    if (patrol != null)
    {
        patrol.leftPoint = spawnLeft;
        patrol.rightPoint = spawnRight;
    }
}

    }
}
