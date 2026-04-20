using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    public PlayerHealth playerHealth;

    [Header("Colors")]
    public Color highColor   = new Color(0.2f, 0.9f, 0.3f);   // green
    public Color midColor    = new Color(1f,   0.75f, 0f);     // yellow
    public Color lowColor    = new Color(0.9f, 0.15f, 0.1f);   // red
    public float midThreshold = 0.5f;
    public float lowThreshold = 0.25f;

    [Header("Damage Flash")]
    public Color flashColor    = Color.white;
    public float flashDuration = 0.12f;

    private float lastHealthPct = 1f;
    private Coroutine flashRoutine;

    void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
            float pct = playerHealth.GetHealthPercent();
            Apply(pct);
            lastHealthPct = pct;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(float pct)
    {
        if (pct < lastHealthPct)
            TriggerFlash();

        Apply(pct);
        lastHealthPct = pct;
    }

    void Apply(float pct)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = pct;

        Color target;
        if (pct > midThreshold)
            target = Color.Lerp(midColor, highColor, (pct - midThreshold) / (1f - midThreshold));
        else if (pct > lowThreshold)
            target = Color.Lerp(lowColor, midColor, (pct - lowThreshold) / (midThreshold - lowThreshold));
        else
            target = lowColor;

        fillImage.color = target;
    }

    void TriggerFlash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        Color original = fillImage.color;
        fillImage.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        fillImage.color = original;
        flashRoutine = null;
    }
}
