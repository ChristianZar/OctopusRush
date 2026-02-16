using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 8f;
    public float waterDrag = 3f;

    [Header("Ink Ability")]
    public GameObject inkCloudPrefab;
    public float maxInk = 100f;
    public float inkDrainPerSecond = 40f;
    public float inkRechargePerSecond = 20f;

    [Header("Ink Cloud Spawn")]
    public float inkSpawnRate = 0.2f;
    public float inkForwardOffset = 1.5f;   // how far in front ink spawns
    public float inkUpOffset = 0.0f;        // optional vertical offset

    [Header("Ink Cooldown")]
    public float inkCooldown = 1.0f;
    private float inkCooldownTimer = 0f;

    [Header("Ink Speed Boost")]
    public bool speedBoostWhileInking = true;
    public float inkSpeedMultiplier = 1.6f; // ↑ make this higher so it’s noticeable (try 1.5–1.8)

    [Header("Ink Speed Boost Tuning")]
    public float boostRampUpTime = 0.25f;     // seconds to reach full boost
    public float boostRampDownTime = 0.35f;   // seconds to return to normal
    public float boostCooldown = 1.25f;       // seconds before boost can be used again
    public float minInkToStartBoost = 10f;    // must have at least this ink to start boosting

    private float boostBlend = 0f;            // 0..1 (smooth boost amount)
    private float boostCooldownTimer = 0f;
    private bool wasBoosting = false;

    [Header("Soft Ceiling (Screen Based)")]
    public Camera cam;
    public float topMargin = 0.6f;
    public float ceilingPush = 12f;

    // ===== Animation =====
    [Header("Animation Settings")]
    public Sprite[] idleAnimationFrames;
    public Sprite[] swimmingAnimationFrames;
    public float idleAnimationSpeed = 0.15f;
    public float swimmingAnimationSpeed = 0.08f;
    public bool animateFasterWhenInking = true;
    public float inkAnimationSpeedMultiplier = 2f;

    [Header("Sprite Flipping")]
    public bool enableFlipping = true;
    public float flipThreshold = 0.1f;

    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentFrameIndex = 0;
    private bool facingRight = true;

    // ===== Internals =====
    private float inkTimer;
    private float currentInk;
    private bool usingInk;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 velocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = waterDrag;

        currentInk = maxInk;

        if (cam == null) cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (idleAnimationFrames != null && idleAnimationFrames.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = idleAnimationFrames[0];
        }
    }

    void Update()
    {
        // Movement input
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        input = new Vector2(x, y).normalized;

        HandleFlipping();

        // Timers
        if (inkCooldownTimer > 0f) inkCooldownTimer -= Time.deltaTime;
        if (boostCooldownTimer > 0f) boostCooldownTimer -= Time.deltaTime;

        bool wantsInk = Input.GetKey(KeyCode.Space);

        // ===== INK USAGE =====
        if (wantsInk && currentInk > 0f && inkCooldownTimer <= 0f)
        {
            usingInk = true;
            inkTimer += Time.deltaTime;

            if (inkTimer >= inkSpawnRate)
            {
                SpawnInkCloud();
                inkTimer = 0f;
            }

            currentInk -= inkDrainPerSecond * Time.deltaTime;

            if (currentInk <= 0f)
            {
                currentInk = 0f;
                usingInk = false;
                inkCooldownTimer = inkCooldown;
                inkTimer = 0f;
            }
        }
        else
        {
            // if player released Space while using ink, force cooldown
            if (usingInk && !wantsInk)
                inkCooldownTimer = inkCooldown;

            usingInk = false;
            inkTimer = 0f;

            currentInk += inkRechargePerSecond * Time.deltaTime;
        }

        currentInk = Mathf.Clamp(currentInk, 0f, maxInk);

        // ===== BOOST LOGIC (smooth + cooldown) =====
        bool canBoost =
            speedBoostWhileInking &&
            boostCooldownTimer <= 0f &&
            currentInk >= minInkToStartBoost &&
            inkCooldownTimer <= 0f; // don’t boost during ink cooldown

        bool isBoostingNow = (usingInk && canBoost);

        float targetBlend = isBoostingNow ? 1f : 0f;

        float rampTime = (targetBlend > boostBlend) ? boostRampUpTime : boostRampDownTime;
        float rate = (rampTime <= 0.001f) ? 999f : (1f / rampTime);

        boostBlend = Mathf.MoveTowards(boostBlend, targetBlend, rate * Time.deltaTime);

        // When boost just ended, start cooldown
        if (wasBoosting && !isBoostingNow)
        {
            boostCooldownTimer = boostCooldown;
        }
        wasBoosting = isBoostingNow;

        UpdateAnimation();
    }

    void SpawnInkCloud()
    {
        if (inkCloudPrefab == null) return;

        float dir = Mathf.Sign(transform.localScale.x); // 1 right, -1 left

        Vector3 spawnPos = transform.position
                         + Vector3.right * dir * inkForwardOffset
                         + Vector3.up * inkUpOffset;

        Instantiate(inkCloudPrefab, spawnPos, Quaternion.identity);
    }

    void FixedUpdate()
    {
        // Smooth multiplier: 1.0 -> inkSpeedMultiplier based on boostBlend (0..1)
        float speedMult = Mathf.Lerp(1f, inkSpeedMultiplier, boostBlend);

        velocity = Vector2.Lerp(
            velocity,
            input * moveSpeed * speedMult,
            acceleration * Time.fixedDeltaTime
        );

        // Soft ceiling
        if (cam != null)
        {
            float topY = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y;
            float maxY = topY - topMargin;

            if (transform.position.y > maxY)
            {
                float overflow = transform.position.y - maxY;
                velocity.y -= ceilingPush * (1f + overflow) * Time.fixedDeltaTime;
                if (velocity.y > 0f) velocity.y = 0f;
            }
        }

        rb.linearVelocity = velocity;
    }

    void HandleFlipping()
    {
        if (!enableFlipping) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(horizontalInput) > flipThreshold)
        {
            if (horizontalInput > 0 && !facingRight) Flip();
            else if (horizontalInput < 0 && facingRight) Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void UpdateAnimation()
    {
        if (spriteRenderer == null) return;

        bool isMoving = input.magnitude > 0.1f;

        Sprite[] currentSet = isMoving ? swimmingAnimationFrames : idleAnimationFrames;
        float currentSpeed = isMoving ? swimmingAnimationSpeed : idleAnimationSpeed;

        if (currentSet == null || currentSet.Length == 0) return;

        if (usingInk && animateFasterWhenInking)
            currentSpeed /= inkAnimationSpeedMultiplier;

        animationTimer += Time.deltaTime;

        if (animationTimer >= currentSpeed)
        {
            animationTimer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % currentSet.Length;
            spriteRenderer.sprite = currentSet[currentFrameIndex];
        }
    }

    // Optional getters (if you use UI)
    public float GetCurrentInk() => currentInk;
    public float GetMaxInk() => maxInk;
    public bool IsUsingInk() => usingInk;
    public bool IsFacingRight() => facingRight;
}