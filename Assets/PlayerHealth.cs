using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public System.Action<float> OnHealthChanged;

    [Header("Death Visuals")]
    public Sprite aliveSprite;   // optional
    public Sprite deadSprite;    // your “X eyes + blood” sprite later
    public float deathDelay = 1.5f;

    private bool isDead = false;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(1f);

        if (sr != null && aliveSprite != null)
            sr.sprite = aliveSprite;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        float healthPercent = (float)currentHealth / maxHealth;
        OnHealthChanged?.Invoke(healthPercent);

        Debug.Log("Octopus HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
{
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

    StartCoroutine(GameOverDelay());

    var camScroll = Camera.main.GetComponent<CameraAutoScroll>();
if (camScroll != null) camScroll.enabled = false;




}


   IEnumerator GameOverDelay()
{
    yield return new WaitForSeconds(deathDelay);
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}

    public bool IsDead() => isDead;
}
