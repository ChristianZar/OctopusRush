using System.Collections;
using UnityEngine;

public class MenuBubbleSpawner : MonoBehaviour
{
    [Header("Bubble Sprite")]
    public Sprite[] bubbleSprites;   // drag any/all sub-sprites from bubble.png here

    [Header("Spawn Rate")]
    public float minInterval = 0.25f;
    public float maxInterval = 0.7f;

    [Header("Bubble Size")]
    public float minSize = 0.4f;
    public float maxSize = 1.4f;

    [Header("Rise Speed")]
    public float minSpeed = 0.6f;
    public float maxSpeed = 2.2f;

    [Header("Horizontal Sway")]
    public float swayAmount = 0.25f;
    public float swaySpeed  = 1.8f;

    [Header("Opacity")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.55f;

    [Header("Sorting")]
    public string sortingLayerName = "Background";
    public int    sortingOrder     = 2;

    private Camera   cam;
    private float    timer;
    private Sprite[] loadedSprites;

    void Start()
    {
        cam = Camera.main;
        timer = Random.Range(minInterval, maxInterval);

        loadedSprites = bubbleSprites;

        // Pre-fill the screen so it doesn't look empty on first open
        for (int i = 0; i < 12; i++)
            SpawnBubble(prePlace: true);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnBubble();
            timer = Random.Range(minInterval, maxInterval);
        }
    }

    void SpawnBubble(bool prePlace = false)
    {
        if (cam == null || loadedSprites == null || loadedSprites.Length == 0) return;

        // For orthographic 2D cameras, use z=0 for depth and distance=10 for bounds
        float dist = Mathf.Abs(cam.transform.position.z);

        float screenLeft  = cam.ViewportToWorldPoint(new Vector3(0f, 0f, dist)).x;
        float screenRight = cam.ViewportToWorldPoint(new Vector3(1f, 0f, dist)).x;
        float screenBot   = cam.ViewportToWorldPoint(new Vector3(0f, 0f, dist)).y;
        float screenTop   = cam.ViewportToWorldPoint(new Vector3(0f, 1f, dist)).y;

        float x = Random.Range(screenLeft, screenRight);
        float y = prePlace
            ? Random.Range(screenBot, screenTop)
            : screenBot - Random.Range(0.2f, 1.0f);

        GameObject bubble = new GameObject("MenuBubble");
        bubble.transform.position = new Vector3(x, y, 0f);   // z=0 for 2D visibility

        float size = Random.Range(minSize, maxSize);
        bubble.transform.localScale = new Vector3(size, size, 1f);

        SpriteRenderer sr = bubble.AddComponent<SpriteRenderer>();
        sr.sprite            = loadedSprites[Random.Range(0, loadedSprites.Length)];
        sr.sortingLayerName  = sortingLayerName;
        sr.sortingOrder      = sortingOrder;

        float alpha = Random.Range(minAlpha, maxAlpha);
        sr.color = new Color(1f, 1f, 1f, alpha);

        float speed      = Random.Range(minSpeed, maxSpeed);
        float swayOffset = Random.Range(0f, Mathf.PI * 2f);
        float destroyY   = screenTop + 0.5f;

        StartCoroutine(RiseBubble(bubble, sr, speed, swayOffset, alpha, destroyY));
    }

    IEnumerator RiseBubble(GameObject bubble, SpriteRenderer sr,
                           float speed, float swayOffset, float startAlpha, float destroyY)
    {
        if (bubble == null) yield break;

        float elapsed  = 0f;
        float originX  = bubble.transform.position.x;
        float fadeRange = (destroyY - bubble.transform.position.y) * 0.25f;
        fadeRange = Mathf.Max(fadeRange, 0.8f);

        while (bubble != null)
        {
            float y = bubble.transform.position.y;
            if (y >= destroyY) break;

            elapsed += Time.deltaTime;

            float swayX = Mathf.Sin(elapsed * swaySpeed + swayOffset) * swayAmount;
            bubble.transform.position = new Vector3(
                originX + swayX,
                y + speed * Time.deltaTime,
                bubble.transform.position.z
            );

            // Fade out as bubble approaches top
            float distToTop = destroyY - bubble.transform.position.y;
            if (distToTop < fadeRange)
            {
                Color c = sr.color;
                c.a = startAlpha * Mathf.Clamp01(distToTop / fadeRange);
                sr.color = c;
            }

            yield return null;
        }

        if (bubble != null)
            Destroy(bubble);
    }
}
