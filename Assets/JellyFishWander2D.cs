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

    [Header("Bounds (optional)")]
    public bool useBounds = true;
    public Vector2 minBounds = new Vector2(-8f, -4f);
    public Vector2 maxBounds = new Vector2( 8f,  4f);

    Vector2 dir = Vector2.right;
    float nextTurnTime;
    Vector3 startPos;
    float bobOffset;
        private SpriteRenderer sr; 

   void Start()
{
    sr = GetComponent<SpriteRenderer>();   // NEW

    startPos = transform.position;
    bobOffset = Random.Range(0f, 10f);

    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
    dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

    // random starting direction
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

        // face direction (optional)
        if (dir.x != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir.x >= 0 ? 1 : -1);
            transform.localScale = s;
        }

        // keep inside bounds
        if (useBounds)
        {
            Vector3 p = transform.position;

            if (p.x < minBounds.x) { p.x = minBounds.x; dir.x = Mathf.Abs(dir.x); }
            if (p.x > maxBounds.x) { p.x = maxBounds.x; dir.x = -Mathf.Abs(dir.x); }
            if (p.y < minBounds.y) { p.y = minBounds.y; dir.y = Mathf.Abs(dir.y); }
            if (p.y > maxBounds.y) { p.y = maxBounds.y; dir.y = -Mathf.Abs(dir.y); }

            transform.position = p;
        }

        if (sr != null)
{
    if (Mathf.Abs(dir.x) > 0.05f)
        sr.flipX = dir.x < 0f;
}
    }
}