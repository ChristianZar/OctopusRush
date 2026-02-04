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
    private float inkTimer;

    private float currentInk;
    private bool usingInk;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 velocity;

    public GameObject inkPrefab;
    public Transform inkSpawnPoint;

    [Header("Ink Cooldown")]
    public float inkCooldown = 1.0f;
    private float inkCooldownTimer = 0f;

    [Header("Soft Ceiling (Screen Based)")]
    public Camera cam;
    public float topMargin = 0.6f;
    public float ceilingPush = 12f;

    // ========== ANIMATION SETTINGS ==========
    [Header("Animation Settings")]
    [Tooltip("Octopus animation frames (use octopus_clean_frame1/2/3)")]
    public Sprite[] animationFrames;
    
    [Tooltip("Animation speed when idle (higher = faster)")]
    public float idleAnimationSpeed = 0.15f;
    
    [Tooltip("Animation speed when moving (higher = faster)")]
    public float moveAnimationSpeed = 0.08f;
    
    [Tooltip("Animate faster when moving")]
    public bool animateFasterWhenMoving = true;
    
    [Tooltip("Animate even faster when using ink")]
    public bool animateFasterWhenInking = true;
    
    [Tooltip("Ink animation speed multiplier")]
    public float inkAnimationSpeedMultiplier = 2f;
    
    // Animation private variables
    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentFrameIndex = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = waterDrag;

        currentInk = maxInk;

        if (cam == null) cam = Camera.main;

        // Get SpriteRenderer for animation
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerController needs a SpriteRenderer component for animation!");
        }
        
        // Set initial sprite if we have animation frames
        if (animationFrames != null && animationFrames.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = animationFrames[0];
            Debug.Log($"Animation initialized with {animationFrames.Length} frames");
        }
        else
        {
            Debug.LogWarning("No animation frames assigned to PlayerController!");
        }
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

            // Ink ran out -> force cooldown
            if (currentInk <= 0f)
            {
                currentInk = 0f;
                usingInk = false;
                inkCooldownTimer = inkCooldown;
                inkTimer = 0f;
                Debug.Log("INK EMPTY -> COOLDOWN");
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

        // ========== UPDATE ANIMATION ==========
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Smooth underwater movement
        velocity = Vector2.Lerp(
            velocity,
            input * moveSpeed,
            acceleration * Time.fixedDeltaTime
        );

        // ===== SOFT CEILING (screen-based) =====
        if (cam != null)
        {
            float topY = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y;
            float maxY = topY - topMargin;

            if (transform.position.y > maxY)
            {
                float overflow = transform.position.y - maxY;

                // push down smoothly
                velocity.y -= ceilingPush * (1f + overflow) * Time.fixedDeltaTime;

                // optional: if player is holding UP, cancel upward movement near ceiling
                if (velocity.y > 0f) velocity.y = 0f;
            }
        }

        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// Updates the octopus sprite animation
    /// Animates tentacles smoothly through the frames!
    /// </summary>
    void UpdateAnimation()
    {
        // Skip if no animation frames or no sprite renderer
        if (animationFrames == null || animationFrames.Length == 0) return;
        if (spriteRenderer == null) return;

        // Determine if player is moving
        bool isMoving = input.magnitude > 0.1f;

        // Calculate current animation speed based on state
        float currentAnimSpeed = idleAnimationSpeed;

        // Moving? Use faster animation
        if (isMoving && animateFasterWhenMoving)
        {
            currentAnimSpeed = moveAnimationSpeed;
        }

        // Using ink? Animate even faster!
        if (usingInk && animateFasterWhenInking)
        {
            currentAnimSpeed = moveAnimationSpeed / inkAnimationSpeedMultiplier;
        }

        // Update animation timer
        animationTimer += Time.deltaTime;

        // Time to change frame?
        if (animationTimer >= currentAnimSpeed)
        {
            animationTimer = 0f;

            // Move to next frame
            currentFrameIndex++;

            // Loop back to start
            if (currentFrameIndex >= animationFrames.Length)
            {
                currentFrameIndex = 0;
            }

            // Update the sprite
            spriteRenderer.sprite = animationFrames[currentFrameIndex];
        }
    }

    /// <summary>
    /// Public method to get current ink amount (for UI)
    /// </summary>
    public float GetCurrentInk()
    {
        return currentInk;
    }

    /// <summary>
    /// Public method to get max ink (for UI)
    /// </summary>
    public float GetMaxInk()
    {
        return maxInk;
    }

    /// <summary>
    /// Public method to check if using ink (for UI/effects)
    /// </summary>
    public bool IsUsingInk()
    {
        return usingInk;
    }

    /// <summary>
    /// Public method to check if ink is on cooldown (for UI)
    /// </summary>
    public bool IsInkOnCooldown()
    {
        return inkCooldownTimer > 0f;
    }

    /// <summary>
    /// Public method to get cooldown timer (for UI)
    /// </summary>
    public float GetInkCooldownTimer()
    {
        return inkCooldownTimer;
    }
}