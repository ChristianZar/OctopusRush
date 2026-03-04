using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float riseSpeed = 8f;            // upward speed when W is held
    public float fallGravity = 12f;         // downward pull when W is released
    public float maxFallSpeed = -10f;       // terminal velocity downward
    public float acceleration = 8f;

    [Header("Screen Lock")]
    public float screenLockX = 0.2f;       // viewport X position to lock player (0=left, 1=right)

    [Header("Ink Ability")]
    public GameObject inkCloudPrefab;
    public float maxInk = 100f;
    public float inkDrainPerSecond = 40f;
    public float inkRechargePerSecond = 20f;

    [Header("Ink Cloud Spawn")]
    public float inkSpawnRate = 0.2f;
    public float inkForwardOffset = 1.5f;
    public float inkUpOffset = 0.0f;

    [Header("Ink Cooldown")]
    public float inkCooldown = 1.0f;
    private float inkCooldownTimer = 0f;

    [Header("Ink Speed Boost")]
    public bool speedBoostWhileInking = true;
    public float inkSpeedMultiplier = 1.6f;

    [Header("Ink Speed Boost Tuning")]
    public float boostRampUpTime = 0.25f;
    public float boostRampDownTime = 0.35f;
    public float boostCooldown = 1.25f;
    public float minInkToStartBoost = 10f;

    private float boostBlend = 0f;
    private float boostCooldownTimer = 0f;
    private bool wasBoosting = false;

    [Header("Soft Ceiling (Screen Based)")]
    public Camera cam;
    public float topMargin = 0.6f;
    public float ceilingPush = 12f;

    [Header("Soft Floor (Screen Based)")]
    public float bottomMargin = 0.5f;       // how far from bottom edge to stop
    public float floorPush = 12f;           // how hard to push away from floor

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

    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentFrameIndex = 0;
    private bool facingRight = true;

    // ===== Internals =====
    private float inkTimer;
    private float currentInk;
    private bool usingInk;

    private Rigidbody2D rb;
    private Vector2 velocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;       // we handle gravity manually
        rb.linearDamping = 0f;      // no drag — we control velocity directly

        currentInk = maxInk;

        if (cam == null) cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (idleAnimationFrames != null && idleAnimationFrames.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = idleAnimationFrames[0];

        // Always face right in auto-run
        facingRight = true;
    }

    void Update()
    {
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
            if (usingInk && !wantsInk)
                inkCooldownTimer = inkCooldown;

            usingInk = false;
            inkTimer = 0f;

            currentInk += inkRechargePerSecond * Time.deltaTime;
        }

        currentInk = Mathf.Clamp(currentInk, 0f, maxInk);

        // ===== BOOST LOGIC =====
        bool canBoost =
            speedBoostWhileInking &&
            boostCooldownTimer <= 0f &&
            currentInk >= minInkToStartBoost &&
            inkCooldownTimer <= 0f;

        bool isBoostingNow = (usingInk && canBoost);

        float targetBlend = isBoostingNow ? 1f : 0f;
        float rampTime = (targetBlend > boostBlend) ? boostRampUpTime : boostRampDownTime;
        float rate = (rampTime <= 0.001f) ? 999f : (1f / rampTime);

        boostBlend = Mathf.MoveTowards(boostBlend, targetBlend, rate * Time.deltaTime);

        if (wasBoosting && !isBoostingNow)
            boostCooldownTimer = boostCooldown;

        wasBoosting = isBoostingNow;

        UpdateAnimation();
    }

    void SpawnInkCloud()
    {
        if (inkCloudPrefab == null) return;

        // Always spawn behind the player (we auto-run right, so ink goes left)
        float dir = facingRight ? 1f : -1f;

        Vector3 spawnPos = transform.position
                         + Vector3.right * dir * inkForwardOffset
                         + Vector3.up * inkUpOffset;

        Instantiate(inkCloudPrefab, spawnPos, Quaternion.identity);
    }

    void FixedUpdate()
    {
        bool risingInput = Input.GetKey(KeyCode.W);

        // ===== HORIZONTAL: lock player to fixed screen X position =====
        if (cam != null)
        {
            float lockedWorldX = cam.ViewportToWorldPoint(new Vector3(screenLockX, 0.5f, 0f)).x;
            Vector3 pos = transform.position;
            pos.x = lockedWorldX;
            transform.position = pos;
        }
        velocity.x = 0f;

        // ===== VERTICAL: rise on W, fall when released =====
        if (risingInput)
        {
            // Smooth rise toward riseSpeed
            velocity.y = Mathf.Lerp(velocity.y, riseSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Apply downward gravity manually
            velocity.y -= fallGravity * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
        }

        // ===== SOFT CEILING =====
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

            // ===== SOFT FLOOR =====
            float bottomY = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f)).y;
            float minY = bottomY + bottomMargin;

            if (transform.position.y < minY)
            {
                float underflow = minY - transform.position.y;
                velocity.y += floorPush * (1f + underflow) * Time.fixedDeltaTime;
                if (velocity.y < 0f) velocity.y = 0f;
            }
        }

        rb.linearVelocity = velocity;
    }

    void UpdateAnimation()
    {
        if (spriteRenderer == null) return;

        // Always swimming — camera scrolls, player is always in motion
        Sprite[] currentSet = swimmingAnimationFrames;
        float currentSpeed = swimmingAnimationSpeed;

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

    // Optional getters
    public float GetCurrentInk() => currentInk;
    public float GetMaxInk() => maxInk;
    public bool IsUsingInk() => usingInk;
    public bool IsFacingRight() => facingRight;
}