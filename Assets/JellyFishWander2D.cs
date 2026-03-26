using UnityEngine;

public class JellyfishWander2D : MonoBehaviour
{
    [Header("Drift")]
    public float speed = 0.5f;
    public float turnIntervalMin = 2f;
    public float turnIntervalMax = 5f;
    public float turnStrength = 0.4f;

    [Header("Float Wave")]
    public float floatAmountY = 0.3f;   // vertical wave amplitude
    public float floatSpeedY  = 1.1f;   // vertical wave frequency
    public float floatAmountX = 0.15f;  // side-to-side amplitude (different freq = Lissajous pattern)
    public float floatSpeedX  = 0.7f;

    [Header("Vertical Bounds")]
    public float minY = -4f;
    public float maxY =  4f;

    [Header("Cleanup")]
    public float destroyBehindCameraX = 2f;

    Vector2 dir;
    Vector3 driftPos;
    float nextTurnTime;
    float timeOffset;
    private SpriteRenderer sr;
    private Camera cam;

    void Start()
    {
        sr         = GetComponent<SpriteRenderer>();
        cam        = Camera.main;
        driftPos   = transform.position;
        timeOffset = Random.Range(0f, 10f);

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

        if (sr != null)
            sr.flipX = Random.value > 0.5f;

        ScheduleNextTurn();
    }

    void ScheduleNextTurn()
    {
        nextTurnTime = Time.time + Random.Range(turnIntervalMin, turnIntervalMax);
    }

    void Update()
    {
        // Occasionally nudge direction for organic wandering
        if (Time.time >= nextTurnTime)
        {
            Vector2 nudge = Random.insideUnitCircle.normalized * turnStrength;
            dir = (dir + nudge).normalized;
            ScheduleNextTurn();
        }

        // Advance logical drift position
        driftPos += (Vector3)(dir * speed * Time.deltaTime);

        // Bounce off Y bounds
        if (driftPos.y <= minY && dir.y < 0f) dir.y =  Mathf.Abs(dir.y);
        if (driftPos.y >= maxY && dir.y > 0f) dir.y = -Mathf.Abs(dir.y);
        driftPos.y = Mathf.Clamp(driftPos.y, minY, maxY);

        // Two sine waves at different frequencies layered on top of drift
        // → produces a Lissajous-style floating motion instead of pure up/down
        float t    = Time.time + timeOffset;
        float waveX = Mathf.Sin(t * floatSpeedX) * floatAmountX;
        float waveY = Mathf.Sin(t * floatSpeedY) * floatAmountY;

        transform.position = driftPos + new Vector3(waveX, waveY, 0f);

        // Face direction of travel
        if (sr != null && Mathf.Abs(dir.x) > 0.05f)
            sr.flipX = dir.x < 0f;

        if (cam == null) return;

        float camZ     = Mathf.Abs(cam.transform.position.z);
        float leftEdge  = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, camZ)).x;
        float rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, camZ)).x;

        // Destroy once drifted past left edge
        if (driftPos.x < leftEdge - destroyBehindCameraX)
        {
            Destroy(gameObject);
            return;
        }

        // Bounce off right edge
        if (driftPos.x > rightEdge + 1f)
        {
            driftPos.x = rightEdge + 1f;
            dir.x = -Mathf.Abs(dir.x);
        }
    }
}
