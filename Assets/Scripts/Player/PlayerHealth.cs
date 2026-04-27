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
    public Sprite aliveSprite;
    public Sprite deadSprite;
    public float deathDelay = 1.5f;

    [Header("Survival Drain")]
    public bool useHealthDrain = true;
    public float secondsPerDrain = 1f;
    private float drainTimer = 0f;

    [Header("Hit Feedback")]
    [SerializeField] private float shakeDuration   = 0.15f;
    [SerializeField] private float shakeMagnitude  = 0.10f;
    [SerializeField] private float hitStopDuration = 0.05f;

    private bool isDead = false;
    private SpriteRenderer sr;
    private GameManager gameManager;
    private DamageFX damageFX;
    private CameraShake camShake;
    private Coroutine hitStopRoutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        damageFX = GetComponent<DamageFX>();
        if (Camera.main != null)
            camShake = Camera.main.GetComponent<CameraShake>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(1f);

        if (sr != null && aliveSprite != null)
            sr.sprite = aliveSprite;

        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        var shield = GetComponent<ShieldSystem>();
        if (shield != null && shield.IsShieldActive)
        {
            AchievementManager.Instance?.ReportDamageBlocked();
            return;
        }

        AchievementManager.Instance?.ReportEnemyDamageTaken();
        int prevHealth = currentHealth;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        damageFX?.SpawnBlood();
        AudioManager.Instance?.PlayPlayerDamage();
        camShake?.Shake(shakeDuration, shakeMagnitude);
        if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
        hitStopRoutine = StartCoroutine(HitStop());

        float healthPercent = (float)currentHealth / maxHealth;
        OnHealthChanged?.Invoke(healthPercent);

        if (currentHealth <= 0)
        {
            AchievementManager.Instance?.ReportDyingWithHP(prevHealth);
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int before = currentHealth;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth != before)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            OnHealthChanged?.Invoke(healthPercent);
            AudioManager.Instance?.PlayHeal();

            if (currentHealth == maxHealth)
                AchievementManager.Instance?.ReportHealedToFull();
        }
    }

    void Die()
    {
        isDead = true;
        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
            hitStopRoutine = null;
            Time.timeScale = 1f;
        }
        AudioManager.Instance?.StopAllAudio();
        AudioManager.Instance?.PlayPlayerDeath();

        if (sr != null && deadSprite != null)
            sr.sprite = deadSprite;

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        var camScroll = Camera.main.GetComponent<CameraAutoScroll>();
        if (camScroll != null) camScroll.enabled = false;

        if (gameManager == null)
        {
            StartCoroutine(GameOverDelay());
        }
    }

    IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
        hitStopRoutine = null;
    }

    IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        isDead = false;
        drainTimer = 0f;
        fishEatenCounter = 0;

        OnHealthChanged?.Invoke(1f);

        if (sr != null && aliveSprite != null)
            sr.sprite = aliveSprite;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;

        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = true;

        var camScroll = Camera.main.GetComponent<CameraAutoScroll>();
        if (camScroll != null) camScroll.enabled = true;
    }

    public bool IsDead() => isDead;

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    public void OnFishEaten()
    {
        if (isDead) return;
        if (currentHealth >= maxHealth) return;

        fishEatenCounter++;

        AudioManager.Instance?.PlayFishEat(); // 🔊 ADDED SOUND HERE

        if (fishEatenCounter >= fishPerHeal)
        {
            fishEatenCounter = 0;
            Heal(1);
        }
    }

    void Update()
    {
        if (isDead) return;
        if (!useHealthDrain) return;

        drainTimer += Time.deltaTime;

        if (drainTimer >= secondsPerDrain)
        {
            drainTimer = 0f;
            TakeDrainDamage(1);
        }
    }

    void TakeDrainDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        AudioManager.Instance?.PlayPlayerDamage();

        float healthPercent = (float)currentHealth / maxHealth;
        OnHealthChanged?.Invoke(healthPercent);

        if (currentHealth <= 0)
            Die();
    }

}