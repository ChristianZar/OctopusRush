using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Attach to the same GameObject as (or alongside) the ink bar UI.
/// Wire up inkFill, cooldownOverlay, and player in the Inspector.
public class InkBarUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public Image inkFill;             // the ink fill bar image
    public Image cooldownOverlay;     // a semi-transparent red/grey overlay (set alpha via color)

    [Header("Colors")]
    public Color normalColor   = new Color(0.1f, 0.6f, 1f, 1f);   // blue ink
    public Color depletedColor = new Color(0.4f, 0.4f, 0.4f, 1f); // grey when empty
    public Color cooldownColor = new Color(1f, 0.2f, 0.2f, 0.55f); // red tint during cooldown

    void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.GetComponent<PlayerController>();
        }

        if (cooldownOverlay != null)
            cooldownOverlay.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float pct = player.GetCurrentInk() / player.GetMaxInk();

        if (inkFill != null)
        {
            inkFill.fillAmount = pct;
            inkFill.color = pct > 0.01f ? normalColor : depletedColor;
        }

        if (cooldownOverlay != null)
        {
            bool onCooldown = player.IsInkOnCooldown();
            cooldownOverlay.gameObject.SetActive(onCooldown);
            if (onCooldown)
            {
                float cdPct = player.GetInkCooldownNormalized();
                Color c = cooldownColor;
                c.a = cooldownColor.a * cdPct;
                cooldownOverlay.color = c;
                cooldownOverlay.fillAmount = cdPct;
            }
        }
    }
}
