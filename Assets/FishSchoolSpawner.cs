using UnityEngine;

public class FishSchoolSpawner : MonoBehaviour
{
    public GameObject fishPrefab;

    public int easyMaxFish  = 8;
    public int hardMaxFish  = 18;
    public int easySchoolSize = 3;
    public int hardSchoolSize = 7;

    public float easySpawnInterval = 5f;
    public float hardSpawnInterval = 2f;
    public float spawnAheadDistance = 12f;
    public float yMin = -1f;
    public float yMax = 2f;

    public float xSpacing = 1.6f;
    public float ySpacing = 0.9f;

    [Header("Difficulty Ramp")]
    public float easyDistance    = 30f;
    public float rampEndDistance = 200f;

    float timer;
    float startX;
    Transform playerT;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { playerT = p.transform; startX = p.transform.position.x; }
    }

    float DifficultyT()
    {
        if (playerT == null) return 0f;
        float t = Mathf.InverseLerp(easyDistance, rampEndDistance, playerT.position.x - startX);
        return Mathf.SmoothStep(0f, 1f, t);
    }

    void Update()
    {
        float t = DifficultyT();
        float spawnInterval = Mathf.Lerp(easySpawnInterval, hardSpawnInterval, t);

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        int existing = GameObject.FindGameObjectsWithTag("Fish").Length;
        int maxFish = Mathf.RoundToInt(Mathf.Lerp(easyMaxFish, hardMaxFish, t));
        if (existing >= maxFish) return;

        SpawnSchool(existing, maxFish, t);
    }

    void SpawnSchool(int existing, int maxFish, float t)
    {
        float spawnX = (playerT != null) ? playerT.position.x + spawnAheadDistance : transform.position.x;
        float baseY  = Random.Range(yMin, yMax);
        int schoolSize = Mathf.RoundToInt(Mathf.Lerp(easySchoolSize, hardSchoolSize, t));
        int toSpawn = Mathf.Min(schoolSize, maxFish - existing);

        for (int i = 0; i < toSpawn; i++)
        {
            float x = spawnX + i * xSpacing + Random.Range(-0.2f, 0.2f);
            float y = baseY + Random.Range(-ySpacing, ySpacing); // ✅ FIXED

            Instantiate(fishPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        }
    }
}