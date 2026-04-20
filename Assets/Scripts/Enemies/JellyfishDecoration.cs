using UnityEngine;
using UnityEngine.Rendering.Universal;

public class JellyfishDecoration : MonoBehaviour
{
    [Header("Bobbing")]
    public float bobHeight = 0.4f;
    public float bobSpeed = 1.2f;
    public float bobOffset = 0f;

    [Header("Pulsing Glow")]
    public float minAlpha = 0.5f;
    public float maxAlpha = 1.0f;
    public float pulseSpeed = 1.5f;

    private Vector3 startPosition;
    private SpriteRenderer sr;
    private float timeOffset;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        timeOffset = Random.Range(0f, Mathf.PI * 2f);
        bobSpeed += Random.Range(-0.3f, 0.3f);
        pulseSpeed += Random.Range(-0.3f, 0.3f);
    }

    void Update()
    {
        float t = Time.time + timeOffset;

        // Bobbing
        float newY = startPosition.y + Mathf.Sin(t * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Pulsing alpha
        if (sr != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(t * pulseSpeed) + 1f) / 2f);
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}