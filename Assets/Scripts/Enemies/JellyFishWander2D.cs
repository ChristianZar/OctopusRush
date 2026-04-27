using UnityEngine;

public class JellyfishWander2D : MonoBehaviour
{
    [Header("Drift")]
    public float speed = 0.5f;
    public float turnIntervalMin = 2f;
    public float turnIntervalMax = 5f;
    public float turnStrength = 0.4f;

    [Header("Float Wave")]
    public float floatAmountY = 0.3f;
    public float floatSpeedY = 1.1f;
    public float floatAmountX = 0.15f;
    public float floatSpeedX = 0.7f;

    [Header("Vertical Bounds")]
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Cleanup")]
    public float destroyBehindCameraX = 2f;

    private Vector2 dir;
    private Vector3 driftPos;
    private float nextTurnTime;
    private float timeOffset;
    private SpriteRenderer sr;
    private Camera cam;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        driftPos = transform.position;
        timeOffset = Random.Range(0f, 10f);

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;
        else
            dir.Normalize();

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
        if (HasInvalidVector2(dir) || HasInvalidVector3(driftPos))
        {
            ResetSafeState();
        }

        if (Time.time >= nextTurnTime)
        {
            Vector2 random = Random.insideUnitCircle;

            // Only normalize if the vector is not tiny
            Vector2 nudge = Vector2.zero;
            if (random.sqrMagnitude > 0.0001f)
            {
                nudge = random.normalized * turnStrength;
            }

            Vector2 newDir = dir + nudge;

            if (newDir.sqrMagnitude > 0.0001f)
                dir = newDir.normalized;
            else
                dir = Vector2.right;

            ScheduleNextTurn();
        }

        driftPos += (Vector3)(dir * speed * Time.deltaTime);

        if (driftPos.y <= minY && dir.y < 0f) dir.y = Mathf.Abs(dir.y);
        if (driftPos.y >= maxY && dir.y > 0f) dir.y = -Mathf.Abs(dir.y);

        driftPos.y = Mathf.Clamp(driftPos.y, minY, maxY);

        float t = Time.time + timeOffset;
        float waveX = Mathf.Sin(t * floatSpeedX) * floatAmountX;
        float waveY = Mathf.Sin(t * floatSpeedY) * floatAmountY;

        Vector3 finalPos = driftPos + new Vector3(waveX, waveY, 0f);

        if (HasInvalidVector3(finalPos))
        {
            ResetSafeState();
            return;
        }

        transform.position = finalPos;

        if (sr != null && Mathf.Abs(dir.x) > 0.05f)
            sr.flipX = dir.x < 0f;

        if (cam == null) return;

        float camZ = Mathf.Abs(cam.transform.position.z);
        float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, camZ)).x;
        float rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, camZ)).x;

        if (driftPos.x < leftEdge - destroyBehindCameraX)
        {
            Destroy(gameObject);
            return;
        }

        if (driftPos.x > rightEdge + 1f)
        {
            driftPos.x = rightEdge + 1f;
            dir.x = -Mathf.Abs(dir.x);
        }
    }

    void ResetSafeState()
    {
        driftPos = transform.position;

        if (HasInvalidVector3(driftPos))
            driftPos = Vector3.zero;

        dir = Vector2.right;
    }

    bool HasInvalidVector2(Vector2 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y) ||
               float.IsInfinity(v.x) || float.IsInfinity(v.y);
    }

    bool HasInvalidVector3(Vector3 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
               float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
    }
}