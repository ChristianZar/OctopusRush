using System.Collections;
using UnityEngine;

public class RockWarningAndDrop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject shadowPrefab;
    [SerializeField] private Camera cam;
    [SerializeField] private ScoreManager scoreManager;   // <-- NEW (drag in Inspector)

    [Header("Shake Settings")]
    [SerializeField] private float warningShakeDuration = 0.45f;
    [SerializeField] private float warningShakeMagnitude = 0.10f;
    [SerializeField] private float delayBeforeDrop = 0.15f;

    [Header("Spawn Settings")]
    [SerializeField] private float topMargin = 0.5f;     
    [SerializeField] private float groundOffset = 0.5f;  
    [SerializeField] private int rocksToSpawn = 3;        // base amount

    [Header("Shadow Settings")]
    [SerializeField] private float shadowDuration = 0.7f;

    [Header("Debug / Test")]
    [SerializeField] private float triggerCooldown = 0.2f;
    private bool canTrigger = true;

    [Header("Rock Sprites (pick 3)")]
    [SerializeField] private Sprite[] rockSprites;
    [SerializeField] private int rockOrderInLayer = 20;

    [Header("Auto Difficulty (Meters Scaling)")]
    [SerializeField] private bool autoStart = true;

    [SerializeField] private float startDelay = 2f;          // wait before first rocks
    [SerializeField] private float intervalAt0m = 7f;        // slower at start
    [SerializeField] private float intervalAt1000m = 3.5f;   // faster later
    [SerializeField] private float metersForMax = 1000f;     // meters where it reaches max difficulty

    [SerializeField] private int extraRocksMax = 3;          // +0..+3 rocks as meters increase

    private float nextTriggerTime = -1f;

    private void Start()
    {
        if (cam == null) cam = Camera.main;

        // If you forget to drag ScoreManager, try to find it automatically
        if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();

        if (autoStart)
            nextTriggerTime = Time.time + startDelay;
    }

    private void Update()
    {
        if (!autoStart) return;
        if (Time.time < nextTriggerTime) return;

        float meters = (scoreManager != null) ? scoreManager.GetMeters() : 0f;

        // 0..1 difficulty based on meters
        float t = Mathf.Clamp01(meters / Mathf.Max(1f, metersForMax));

        // Interval shrinks as meters increases
        float interval = Mathf.Lerp(intervalAt0m, intervalAt1000m, t);

        // Rocks increase a bit as meters increases
        int extra = Mathf.RoundToInt(Mathf.Lerp(0f, extraRocksMax, t));
        rocksToSpawn = Mathf.Max(1, 3 + extra); // base 3 + extra (safe)

        TriggerRockFall();

        // schedule next trigger
        nextTriggerTime = Time.time + interval;
    }

    public void TriggerRockFall()
    {
        if (!canTrigger) return;
        StartCoroutine(TriggerCooldown());
        StartCoroutine(WarningThenDrop());
    }

    private IEnumerator TriggerCooldown()
    {
        canTrigger = false;
        yield return new WaitForSecondsRealtime(triggerCooldown);
        canTrigger = true;
    }

    private IEnumerator WarningThenDrop()
    {
        if (rockPrefab == null || shadowPrefab == null || cam == null) yield break;

        if (cameraShake != null)
            cameraShake.Shake(warningShakeDuration, warningShakeMagnitude);

        yield return new WaitForSecondsRealtime(warningShakeDuration + delayBeforeDrop);

        DropRocks();
    }

    private void DropRocks()
    {
        if (rockPrefab == null || shadowPrefab == null || cam == null) return;

        float zDist = Mathf.Abs(cam.transform.position.z);

        Vector3 leftTop = cam.ViewportToWorldPoint(new Vector3(0f, 1f, zDist));
        Vector3 rightTop = cam.ViewportToWorldPoint(new Vector3(1f, 1f, zDist));
        Vector3 leftBottom = cam.ViewportToWorldPoint(new Vector3(0f, 0f, zDist));

        float minX = leftTop.x;
        float maxX = rightTop.x;

        float groundY = leftBottom.y + groundOffset;
        float spawnY = leftTop.y + topMargin;

        for (int i = 0; i < rocksToSpawn; i++)
        {
            float randomX = Random.Range(minX, maxX);
            StartCoroutine(SpawnWithShadow(randomX, groundY, spawnY));
        }
    }

    private IEnumerator SpawnWithShadow(float x, float groundY, float spawnY)
    {
        GameObject shadow = Instantiate(shadowPrefab, new Vector2(x, groundY), Quaternion.identity);

        float randomDelay = Random.Range(0f, 0.4f);
        yield return new WaitForSecondsRealtime(shadowDuration + randomDelay);

        GameObject rock = Instantiate(rockPrefab, new Vector2(x, spawnY), Quaternion.identity);

        rock.transform.position = new Vector3(rock.transform.position.x, rock.transform.position.y, 0f);

        var sr = rock.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
            sr.sortingOrder = rockOrderInLayer;

            if (rockSprites != null && rockSprites.Length > 0)
                sr.sprite = rockSprites[Random.Range(0, rockSprites.Length)];
        }

        var rb = rock.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            if (rb.gravityScale <= 0f) rb.gravityScale = 4f;
        }

        Destroy(shadow);
    }
}
