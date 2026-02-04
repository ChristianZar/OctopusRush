using UnityEngine;
using UnityEngine.UI;

public class SharkHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Image healthFill;        // ← THIS MUST EXIST
    public Canvas healthCanvas;     // ← THIS MUST EXIST

    void Start()
    {
        currentHealth = maxHealth;

        UpdateHealthBar();
    }

    void Update()
    {
        // Make the health bar always face the camera (optional but nice)
        if (healthCanvas != null)
        {
            healthCanvas.transform.rotation = Quaternion.identity;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
