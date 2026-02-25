using UnityEngine;

public class FishSchoolSpawner : MonoBehaviour
{
    public GameObject fishPrefab;

    public int maxFish = 10;
    public int schoolSize = 5;

    public float spawnInterval = 4f;
    public float spawnAheadDistance = 12f; // spawns ahead of player
    public float yMin = -1f;
    public float yMax = 2f;

    public float xSpacing = 1.6f;
    public float ySpacing = 0.9f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        int count = GameObject.FindGameObjectsWithTag("Fish").Length;
        if (count >= maxFish) return;

        SpawnSchool();
    }

    void SpawnSchool()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        float spawnX = (player != null) ? player.transform.position.x + spawnAheadDistance : transform.position.x;

        float baseY = Random.Range(yMin, yMax);

        int existing = GameObject.FindGameObjectsWithTag("Fish").Length;
        int toSpawn = Mathf.Min(schoolSize, maxFish - existing);

        for (int i = 0; i < toSpawn; i++)
        {
            float x = spawnX + i * xSpacing + Random.Range(-0.2f, 0.2f);
            float y = baseY + Random.Range(-ySpacing, ySpacing); // ✅ FIXED

            Instantiate(fishPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        }
    }
}