using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 8f;
    public float waterDrag = 3f;

    [Header("Ink Ability")]
    public GameObject inkCloudPrefab;        // <-- fixed name
    public float maxInk = 100f;
    public float inkDrainPerSecond = 40f;
    public float inkRechargePerSecond = 20f;

    [Header("Ink Cloud Spawn")]
    public float inkSpawnRate = 0.2f;        // spawn every 0.2 seconds
    private float inkTimer;

    private float currentInk;
    private bool usingInk;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 velocity;

    public GameObject inkPrefab;
    public Transform inkSpawnPoint;

[Header("Ink Cooldown")]
public float inkCooldown = 1.0f;   // seconds you must wait after stopping ink
private float inkCooldownTimer = 0f;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = waterDrag;

        currentInk = maxInk;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
    Debug.Log("SPACE PRESSED");

        // Movement input
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        input = new Vector2(x, y).normalized;

      // INK COOLDOWN TIMER
if (inkCooldownTimer > 0f)
{
    inkCooldownTimer -= Time.deltaTime;
}

// Check input
bool wantsInk = Input.GetKey(KeyCode.Space);

// ===== INK USAGE =====
if (wantsInk && currentInk > 0f && inkCooldownTimer <= 0f)
{
    usingInk = true;

    inkTimer += Time.deltaTime;

    if (inkTimer >= inkSpawnRate)
    {
        Instantiate(inkCloudPrefab, transform.position, Quaternion.identity);
        inkTimer = 0f;
    }

    currentInk -= inkDrainPerSecond * Time.deltaTime;

    // 🔴 INK RAN OUT → FORCE COOLDOWN
    if (currentInk <= 0f)
    {
        currentInk = 0f;
        usingInk = false;
        inkCooldownTimer = inkCooldown;
        inkTimer = 0f;
        Debug.Log("INK EMPTY → COOLDOWN");
    }
}
else
{
    // If player released Space while using ink
    if (usingInk && !wantsInk)
    {
        inkCooldownTimer = inkCooldown;
    }

    usingInk = false;
    inkTimer = 0f;

    currentInk += inkRechargePerSecond * Time.deltaTime;
}

// Clamp ink
currentInk = Mathf.Clamp(currentInk, 0f, maxInk);

    }

    void FixedUpdate()
    {
        // Smooth underwater movement
        velocity = Vector2.Lerp(
            velocity,
            input * moveSpeed,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = velocity;
    }
}
