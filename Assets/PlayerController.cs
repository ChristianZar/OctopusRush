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

    // ========== SPRITE FLIPPING ==========
    [Header("Sprite Flipping")]
    [Tooltip("Enable sprite flipping based on movement direction")]
    public bool enableFlipping = true;
    
    [Tooltip("Flip sprite when moving left")]
    public bool flipWhenMovingLeft = true;
    
    [Tooltip("Minimum horizontal speed to trigger flip")]
    public float flipThreshold = 0.1f;

    // ========== ANIMATION SETTINGS ==========
    [Header("Idle Animation (8 frames)")]
    [Tooltip("Assign all 8 idle animation frames")]
    public Sprite[] idleFrames; // Size: 8
    
    [Tooltip("Idle animation speed (seconds per frame)")]
    public float idleAnimationSpeed = 0.12f;
    
    [Header("Swim Animation (4 frames)")]
    [Tooltip("Assign all 4 swim animation frames")]
    public Sprite[] swimFrames; // Size: 4
    
    [Tooltip("Swim animation speed (seconds per frame)")]
    public float swimAnimationSpeed = 0.08f;
    
    [Header("Animation Settings")]
    [Tooltip("Speed threshold to trigger swim animation")]
    public float swimSpeedThreshold = 0.1f;
    
    [Tooltip("Animate faster when using ink")]
    public bool animateFasterWhenInking = true;
    
    [Tooltip("Ink animation speed multiplier")]
    public float inkAnimationMultiplier = 1.5f;
    
    // Animation private variables
    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentFrameIndex = 0;
    private Sprite[] currentAnimation;
    private bool facingRight = true; // Track which way we're facing

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
        
        // Set initial animation to idle
        if (idleFrames != null && idleFrames.Length > 0)
        {
            currentAnimation = idleFrames;
            spriteRenderer.sprite = idleFrames[0];
            Debug.Log($"Animation initialized: {idleFrames.Length} idle frames, {swimFrames?.Length ?? 0} swim frames");
        }
        else
        {
            Debug.LogWarning("No idle frames assigned to PlayerController!");
        }

        // Ensure sprite starts facing right (not flipped)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            facingRight = true;
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

        // ========== UPDATE SPRITE FLIPPING ==========
        UpdateSpriteFlipping();

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

<<<<<<< Updated upstream:Assets/PlayerController.cs
=======
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Handle collision
            Debug.Log("Collided with obstacle!");
        }
    }

    /// <summary>
    /// Updates sprite flipping based on horizontal movement direction
    /// </summary>
    void UpdateSpriteFlipping()
    {
        if (!enableFlipping || spriteRenderer == null) return;

        // Check horizontal input
        float horizontalInput = input.x;

        // Only flip if movement is above threshold
        if (Mathf.Abs(horizontalInput) > flipThreshold)
        {
            if (horizontalInput > 0) // Moving right
            {
                // Face right (not flipped)
                if (!facingRight)
                {
                    Flip();
                }
            }
            else if (horizontalInput < 0) // Moving left
            {
                // Face left (flipped)
                if (facingRight && flipWhenMovingLeft)
                {
                    Flip();
                }
            }
        }
    }

    /// <summary>
    /// Flips the sprite horizontally
    /// </summary>
    void Flip()
    {
        facingRight = !facingRight;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
        
        // Debug info
        // Debug.Log($"Flipped! Now facing: {(facingRight ? "Right" : "Left")}");
    }

>>>>>>> Stashed changes:Assets/Script/PlayerController.cs
    /// <summary>
    /// Updates the octopus sprite animation
    /// Switches between idle (8 frames) and swim (4 frames)
    /// </summary>
    void UpdateAnimation()
    {
        // Skip if no sprite renderer
        if (spriteRenderer == null) return;

        // Determine if player is moving
        bool isMoving = input.magnitude > swimSpeedThreshold;

        // Choose animation based on movement
        Sprite[] targetAnimation;
        float targetSpeed;

        if (isMoving)
        {
            // Use swim animation
            targetAnimation = swimFrames;
            targetSpeed = swimAnimationSpeed;
        }
        else
        {
            // Use idle animation
            targetAnimation = idleFrames;
            targetSpeed = idleAnimationSpeed;
        }

        // Apply ink speed boost if active
        if (usingInk && animateFasterWhenInking)
        {
            targetSpeed = targetSpeed / inkAnimationMultiplier;
        }

        // Check if we need to switch animations
        if (currentAnimation != targetAnimation)
        {
            currentAnimation = targetAnimation;
            currentFrameIndex = 0; // Reset to first frame
            animationTimer = 0f;
            
            if (currentAnimation != null && currentAnimation.Length > 0)
            {
                spriteRenderer.sprite = currentAnimation[0];
            }
        }

        // Skip if no frames in current animation
        if (currentAnimation == null || currentAnimation.Length == 0) return;

        // Update animation timer
        animationTimer += Time.deltaTime;

        // Time to change frame?
        if (animationTimer >= targetSpeed)
        {
            animationTimer = 0f;

            // Move to next frame
            currentFrameIndex++;

            // Loop back to start
            if (currentFrameIndex >= currentAnimation.Length)
            {
                currentFrameIndex = 0;
            }

            // Update the sprite
            spriteRenderer.sprite = currentAnimation[currentFrameIndex];
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

    /// <summary>
    /// Public method to check if currently swimming (for debugging)
    /// </summary>
    public bool IsSwimming()
    {
        return currentAnimation == swimFrames;
    }

    /// <summary>
    /// Public method to get current animation name (for debugging)
    /// </summary>
    public string GetCurrentAnimation()
    {
        if (currentAnimation == idleFrames) return "Idle";
        if (currentAnimation == swimFrames) return "Swim";
        return "None";
    }

    /// <summary>
    /// Public method to check which way sprite is facing (for debugging)
    /// </summary>
    public bool IsFacingRight()
    {
        return facingRight;
    }

    /// <summary>
    /// Public method to manually set facing direction (optional)
    /// </summary>
    public void SetFacingDirection(bool faceRight)
    {
        if (facingRight != faceRight)
        {
            Flip();
        }
    }
}