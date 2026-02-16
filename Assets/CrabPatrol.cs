using UnityEngine;

public class CrabPatrol : MonoBehaviour
{
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Random Movement")]
    public float minSpeed = 1.5f;
    public float maxSpeed = 3.0f;
    public float randomStartDelay = 0.4f;

    [Header("Jumping (Unlock at 400m)")]
    public bool enableJumping = true;
    public float jumpUnlockDistance = 0f;     // ✅ TEST: set 0
    public float jumpCooldown = 1.0f;         // ✅ TEST: faster
    public float jumpForceY = 7.5f;
    public float jumpForceX = 3.5f;
    public float jumpTriggerRangeX = 999f;    // ✅ TEST: always in range

    [Header("Ground Check")]
    public float groundCheckDistance = 0.6f;  // ✅ TEST: bigger
    public LayerMask groundLayer;             // set to Ground in inspector

    private float speed;
    private bool goingRight;
    private SpriteRenderer sr;
    private float delayTimer;

    private Rigidbody2D rb;
    private Transform player;
    private float jumpTimer;

    private static bool runStartSet = false;
    private static float runStartX = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        speed = Random.Range(minSpeed, maxSpeed);
        goingRight = Random.value > 0.5f;
        delayTimer = Random.Range(0f, randomStartDelay);
        jumpTimer = Random.Range(0f, jumpCooldown);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            if (!runStartSet)
            {
                runStartSet = true;
                runStartX = player.position.x;
            }
        }
    }

   void FixedUpdate()
{
    if (!rb || leftPoint == null || rightPoint == null) return;

    if (delayTimer > 0f)
    {
        delayTimer -= Time.fixedDeltaTime;
        return;
    }

    float targetX = goingRight ? rightPoint.position.x : leftPoint.position.x;
    float dirX = Mathf.Sign(targetX - rb.position.x);

    // Move X using velocity, keep Y from physics
#if UNITY_6000_0_OR_NEWER
    rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
#else
    rb.velocity = new Vector2(dirX * speed, rb.velocity.y);
#endif

    if (sr) sr.flipX = (dirX < 0);

    if (Mathf.Abs(rb.position.x - targetX) < 0.1f)
        goingRight = !goingRight;

    if (enableJumping)
        HandleJumping();
}

    void HandleJumping()
    {
        if (player == null || rb == null) return;

        if (jumpTimer > 0f)
        {
            jumpTimer -= Time.fixedDeltaTime;
            return;
        }

        float distance = player.position.x - runStartX;
        if (distance < jumpUnlockDistance)
        {
            Debug.Log("Jump blocked: distance=" + distance);
            return;
        }

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > jumpTriggerRangeX)
        {
            Debug.Log("Jump blocked: range dx=" + dx);
            return;
        }

        bool grounded = IsGrounded();
        Debug.Log("Grounded? " + grounded);

        if (!grounded)
        {
            Debug.Log("Jump blocked: not grounded");
            return;
        }

        float dir = Mathf.Sign(dx == 0 ? 1 : dx);
        rb.linearVelocity = new Vector2(dir * jumpForceX, jumpForceY);

        Debug.Log("✅ CRAB JUMP!");
        jumpTimer = jumpCooldown;

        Debug.Log("Distance: " + distance);
Debug.Log("IsGrounded: " + IsGrounded());
    }

 bool IsGrounded()
{
    // Start from bottom of the crab collider
    float extraHeight = 0.05f;
    Bounds bounds = GetComponent<Collider2D>().bounds;

    Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + extraHeight);

    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

    Debug.DrawRay(origin, Vector2.down * groundCheckDistance, hit.collider ? Color.green : Color.red);

    return hit.collider != null;
}
}