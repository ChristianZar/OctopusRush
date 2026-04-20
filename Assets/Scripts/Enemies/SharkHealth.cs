using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SharkHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Image healthFill;
    public Canvas healthCanvas;

    [Header("Hit FX")]
    public GameObject bloodPuffPrefab;
    public Transform hitFxPoint; // optional fallback

    [Header("Flash")]
    public SpriteRenderer sharkSprite;   // drag shark sprite renderer here
    public float flashTime = 0.08f;

    private Coroutine flashRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        if (healthCanvas != null)
            healthCanvas.transform.rotation = Quaternion.identity;
    }

    // Bullet can call this one (hit position)
    public void TakeDamage(int damage, Vector3 hitPos)
    {
        ApplyDamage(damage, hitPos);
    }

    // Other things can call this one (no hit position)
    public void TakeDamage(int damage)
    {
        Vector3 pos = (hitFxPoint != null) ? hitFxPoint.position : transform.position;
        ApplyDamage(damage, pos);
    }

    private void ApplyDamage(int damage, Vector3 bloodPos)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SpawnBlood(bloodPos);
        FlashRed();
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    private void SpawnBlood(Vector3 pos)
    {
        if (bloodPuffPrefab == null) return;
        Instantiate(bloodPuffPrefab, pos, Quaternion.identity);
    }

    private void FlashRed()
    {
        if (sharkSprite == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        Color original = sharkSprite.color;
        sharkSprite.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        sharkSprite.color = original;
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        AchievementManager.Instance?.ReportSharkKilled();
        Destroy(gameObject);
    }
}
