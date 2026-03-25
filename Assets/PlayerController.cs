using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float riseSpeed = 8f;
    public float fallGravity = 12f;
    public float maxFallSpeed = -10f;
    public float acceleration = 8f;

    [Header("Dash / Speed Boost")]
    public float normalForwardSpeed = 0f;   // keep 0 if player is screen-locked
    public float boostedRiseMultiplier = 1.5f;
    public float boostedFallMultiplier = 0.7f;

    [Header("Screen Lock")]
    public float screenLockX = 0.2f;

    [Header("Ink Ability")]
    public GameObject inkCloudPrefab;
    public float maxInk = 100f;
    public float inkDrainPerSecond = 40f;
    public float inkRechargePerSecond = 20f;

    [Header("Ink Cloud Spawn")]
    public float inkSpawnRate = 0.2f;
    public float inkBackwardOffset = 1.5f;
    public float inkUpOffset = 0.0f;

    [Header("Swim Ink Visual (W key)")]
    public GameObject swimInkPrefab;
    public float swimInkSpawnRate = 0.12f;
    public float swimInkBackwardOffset = 0.15f;
    public float swimInkDownOffset = 0.8f;

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
    public float bottomMargin = 0.5f;
    public float floorPush = 12f;

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

    private float inkTimer;
    private float currentInk;
    private bool usingInk;

    private float swimInkTimer;

    private Rigidbody2D rb;
    private Vector2 velocity;

    [Header("References")]
public CameraAutoScroll cameraAutoScroll;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;

        currentInk = maxInk;

        if (cam == null) cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (idleAnimationFrames != null && idleAnimationFrames.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = idleAnimationFrames[0];

        facingRight = true;
    }

    void Update()
    {
        if (inkCooldownTimer > 0f) inkCooldownTimer -= Time.deltaTime;
        if (boostCooldownTimer > 0f) boostCooldownTimer -= Time.deltaTime;

        bool wantsInk = Input.GetKey(KeyCode.Space);

        bool wantsSwimInk = Input.GetKey(KeyCode.W);

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

        bool canBoost =
            speedBoostWhileInking &&
            boostCooldownTimer <= 0f &&
            currentInk >= minInkToStartBoost &&
            inkCooldownTimer <= 0f;

        bool isBoostingNow = usingInk && canBoost;

        float targetBlend = isBoostingNow ? 1f : 0f;
        float rampTime = (targetBlend > boostBlend) ? boostRampUpTime : boostRampDownTime;
        float rate = (rampTime <= 0.001f) ? 999f : (1f / rampTime);

        boostBlend = Mathf.MoveTowards(boostBlend, targetBlend, rate * Time.deltaTime);

        if (wasBoosting && !isBoostingNow)
            boostCooldownTimer = boostCooldown;

        wasBoosting = isBoostingNow;
        if (cameraAutoScroll != null)
{
    cameraAutoScroll.SetBoosting(boostBlend > 0.05f);
}
      // ===== SWIM INK VISUAL (W key only) =====
if (wantsSwimInk)
{
    swimInkTimer += Time.deltaTime;

    if (swimInkTimer >= swimInkSpawnRate)
    {
        SpawnSwimInk();
        swimInkTimer = 0f;
    }
}
else
{
    swimInkTimer = 0f;
}

        UpdateAnimation();
    }

    void SpawnInkCloud()
    {
        if (inkCloudPrefab == null) return;

        // Spawn BEHIND the octopus
        float behindDir = facingRight ? -1f : 1f;

        Vector3 spawnPos = transform.position
                         + Vector3.right * behindDir * inkBackwardOffset
                         + Vector3.up * inkUpOffset;

        Instantiate(inkCloudPrefab, spawnPos, Quaternion.identity);
    }

    void SpawnSwimInk()
{
    if (swimInkPrefab == null) return;

    float behindDir = facingRight ? -1f : 1f;

    Vector3 spawnPos = transform.position
                     + Vector3.right * behindDir * swimInkBackwardOffset
                     + Vector3.down * swimInkDownOffset;

    Instantiate(swimInkPrefab, spawnPos, Quaternion.identity);
}

    void FixedUpdate()
    {
        if (cam == null) cam = Camera.main;

        bool risingInput = Input.GetKey(KeyCode.W);

        if (cam != null)
        {
            float lockedWorldX = cam.ViewportToWorldPoint(new Vector3(screenLockX, 0.5f, 0f)).x;
            Vector3 pos = transform.position;
            pos.x = lockedWorldX;
            transform.position = pos;
        }

        float riseMultiplier = Mathf.Lerp(1f, boostedRiseMultiplier, boostBlend);
        float fallMultiplier = Mathf.Lerp(1f, boostedFallMultiplier, boostBlend);

        velocity.x = normalForwardSpeed;

        if (risingInput)
        {
            float boostedRiseSpeed = riseSpeed * riseMultiplier;
            velocity.y = Mathf.Lerp(velocity.y, boostedRiseSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            float boostedGravity = fallGravity * fallMultiplier;
            velocity.y -= boostedGravity * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
        }

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

    public float GetCurrentInk() => currentInk;
    public float GetMaxInk() => maxInk;
    public bool IsUsingInk() => usingInk;
    public bool IsFacingRight() => facingRight;
    public bool IsInkOnCooldown() => inkCooldownTimer > 0f;
    public float GetInkCooldownNormalized() => Mathf.Clamp01(inkCooldownTimer / inkCooldown);
}