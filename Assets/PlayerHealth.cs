using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    public int fishPerHeal = 3;
    private int fishEatenCounter = 0;

    public System.Action<float> OnHealthChanged;

    [Header("Death Visuals")]
    public Sprite aliveSprite;   // optional
    public Sprite deadSprite;    // your "X eyes + blood" sprite later
    public float deathDelay = 1.5f;

    private bool isDead = false;
    private SpriteRenderer sr;
    private GameManager gameManager; // NEW: Reference to GameManager
    private DamageFX damageFX;


    void Awake()
{
    sr = GetComponent<SpriteRenderer>();
    damageFX = GetComponent<DamageFX>();
}


    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(1f);

        if (sr != null && aliveSprite != null)
            sr.sprite = aliveSprite;
        
        // NEW: Find GameManager
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void TakeDamage(int amount)
{
    if (isDead) return;

    // ✅ SHIELD: ignore damage if bubble shield is active
    var shield = GetComponent<ShieldSystem>();
    if (shield != null && shield.IsShieldActive)
    {
        // optional: play a "blocked" effect instead of blood
        // damageFX?.SpawnShieldHit();
        return;
    }

    currentHealth -= amount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    damageFX?.SpawnBlood();

    float healthPercent = (float)currentHealth / maxHealth;
    OnHealthChanged?.Invoke(healthPercent);

    Debug.Log("Octopus HP: " + currentHealth);

    if (currentHealth <= 0)
    {
        Die();
    }
}

    public void Heal(int amount)
{
    if (isDead) return;                    // don’t heal a dead player
    if (amount <= 0) return;

    int before = currentHealth;

    currentHealth += amount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    if (currentHealth != before)
    {
        float healthPercent = (float)currentHealth / maxHealth;
        OnHealthChanged?.Invoke(healthPercent);
        Debug.Log("Healed! Octopus HP: " + currentHealth);
    }
}

    void Die()
    {
        // Save score
        var sm = FindFirstObjectByType<ScoreManager>();
        if (sm != null) sm.SaveLastRun();

        isDead = true;
        Debug.Log("Octopus Died");

        // Swap sprite to dead (X eyes + blood)
        if (sr != null && deadSprite != null)
            sr.sprite = deadSprite;

        // Disable movement/ink controls
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = false;

        // Enable gravity so the body falls
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.bodyType = RigidbodyType2D.Dynamic; // make sure physics is active
            rb.gravityScale = 3f;                  // fall speed (tweak: 2-6)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // no spinning
        }

        // Stop camera scroll
        var camScroll = Camera.main.GetComponent<CameraAutoScroll>();
        if (camScroll != null) camScroll.enabled = false;

        // NEW: Instead of auto-reloading, let GameManager handle it
        // The GameManager will detect IsDead() and show game over screen
        // So we DON'T call StartCoroutine(GameOverDelay()) anymore
        
        // Only auto-reload if there's no GameManager (fallback)
        if (gameManager == null)
        {
            Debug.LogWarning("No GameManager found - using fallback auto-reload");
            StartCoroutine(GameOverDelay());
        }
    }

    // OLD: This is now only used as fallback if no GameManager exists
    IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // NEW: Public method to respawn player (called by GameManager on Continue)
    public void Respawn()
    {
        Debug.Log("PlayerHealth: Respawning player");
        
        // Reset health
        currentHealth = maxHealth;
        isDead = false;
        
        // Notify UI
        OnHealthChanged?.Invoke(1f);
        
        // Restore alive sprite
        if (sr != null && aliveSprite != null)
        {
            sr.sprite = aliveSprite;
        }
        
        // Reset physics
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f; // Back to underwater (no gravity)
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // Re-enable controls (GameManager will also do this, but doesn't hurt)
        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            shooting.enabled = true;
        }
        
        // Re-enable camera scroll
        var camScroll = Camera.main.GetComponent<CameraAutoScroll>();
        if (camScroll != null)
        {
            camScroll.enabled = true;
        }
        
        Debug.Log($"Player respawned with {currentHealth}/{maxHealth} HP");
    }

    public bool IsDead() => isDead;
    
    // NEW: Public method to get current health percentage
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    public void OnFishEaten()
{
    if (isDead) return;
    if (currentHealth >= maxHealth) return;

    fishEatenCounter++;
    if (fishEatenCounter >= fishPerHeal)
    {
        fishEatenCounter = 0;
        Heal(1);
    }
}
}