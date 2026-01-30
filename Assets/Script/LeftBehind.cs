using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LeftBehindDrain : MonoBehaviour
{
    [Header("References (optional)")]
    public Camera cam;                 // leave empty to use Camera.main
    public PlayerHealth health;         // leave empty to auto-find on player

    [Header("How far inside the camera the player must stay")]
    public float safeLeftMargin = 1.5f; // world units from left edge

    [Header("Damage")]
    public float damagePerSecond = 1f;  // e.g., 1 HP per second

    [Header("Optional push forward when behind")]
    public bool pushForward = false;
    public float minForwardSpeed = 2.5f;

    private Rigidbody2D rb;
    private float damageAccumulator = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (cam == null) cam = Camera.main;
        if (health == null) health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (cam == null || health == null || health.IsDead()) return;

        // left edge of camera in world space
        float leftEdgeX = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x;
        float minAllowedX = leftEdgeX + safeLeftMargin;

        bool behind = transform.position.x < minAllowedX;

        if (behind)
        {
            // Accumulate fractional damage and apply as whole HP
            damageAccumulator += damagePerSecond * Time.deltaTime;

            int dmg = Mathf.FloorToInt(damageAccumulator);
            if (dmg > 0)
            {
                health.TakeDamage(dmg);
                damageAccumulator -= dmg;
            }

            // Optional “screen pressure”
            if (pushForward)
            {
                rb.linearVelocity = new Vector2(Mathf.Max(rb.linearVelocity.x, minForwardSpeed), rb.linearVelocity.y);
            }
        }
        else
        {
            // Don’t carry over a huge backlog if player recovers
            damageAccumulator = Mathf.Min(damageAccumulator, 0.5f);
        }
    }
}
