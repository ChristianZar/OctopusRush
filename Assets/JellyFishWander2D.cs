using UnityEngine;

public class JellyfishWander2D : MonoBehaviour
{
    [Header("Drift")]
    public float speed = 0.4f;
    public float turnIntervalMin = 1.5f;
    public float turnIntervalMax = 4.0f;
    public float turnStrength = 0.35f; // how much it changes direction

    [Header("Bobbing")]
    public float bobSpeed = 1.2f;
    public float bobAmount = 0.18f;

    [Header("Vertical Bounds")]
    public float minY = -4f;
    public float maxY =  4f;

    [Header("Cleanup")]
    public float destroyBehindCameraX = 2f;  // world units behind the left edge before destroying

    Vector2 dir = Vector2.right;
    float nextTurnTime;
    Vector3 startPos;
    float bobOffset;
    private SpriteRenderer sr;
    private Camera cam;

    void Start()
    {
        sr  = GetComponent<SpriteRenderer>();
        cam = Camera.main;

        startPos  = transform.position;
        bobOffset = Random.Range(0f, 10f);

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

        if (sr != null)
            sr.flipX = (Random.value > 0.5f);

        ScheduleNextTurn();
    }

    void ScheduleNextTurn()
    {
        nextTurnTime = Time.time + Random.Range(turnIntervalMin, turnIntervalMax);
    }

    void Update()
    {
        // occasionally nudge direction
        if (Time.time >= nextTurnTime)
        {
            Vector2 nudge = Random.insideUnitCircle.normalized * turnStrength;
            dir = (dir + nudge).normalized;
            ScheduleNextTurn();
        }

        // drift
        Vector3 pos = transform.position;
        pos += (Vector3)(dir * speed * Time.deltaTime);

        // bob (vertical sine wave)
        float bob = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobAmount;
        pos.y += bob * Time.deltaTime; // subtle; doesn't explode

        transform.position = pos;

        // face direction
        if (sr != null && Mathf.Abs(dir.x) > 0.05f)
            sr.flipX = dir.x < 0f;

        if (cam != null)
        {
            float leftEdge  = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
            float rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;

            // Destroy if scrolled off the left side
            if (transform.position.y < minY - destroyBehindCameraX)
            {
                Destroy(gameObject);
                return;
            }

            if (transform.position.x < leftEdge - destroyBehindCameraX)
            {
                Destroy(gameObject);
                return;
            }

            // Bounce off right edge and Y bounds
            Vector3 p = transform.position;
            if (p.x > rightEdge + 1f) { p.x = rightEdge + 1f; dir.x = -Mathf.Abs(dir.x); }
            if (p.y < minY) { p.y = minY; dir.y =  Mathf.Abs(dir.y); }
            if (p.y > maxY) { p.y = maxY; dir.y = -Mathf.Abs(dir.y); }
            transform.position = p;
        }
    }
}