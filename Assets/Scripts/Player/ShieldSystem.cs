using System.Collections;
using UnityEngine;

public class ShieldSystem : MonoBehaviour
{
    [Header("Shield Settings")]
    public GameObject shieldBubblePrefab;
    public float shieldDuration = 10f;

    private GameObject currentBubble;              // ✅ your variable
    public bool IsShieldActive { get; private set; }

    private Coroutine shieldRoutine;               // ✅ your variable

    public void ActivateShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        IsShieldActive = true;

        // Destroy old bubble if any
        if (currentBubble != null)
            Destroy(currentBubble);

        // Spawn new bubble
        currentBubble = Instantiate(shieldBubblePrefab, transform);
        currentBubble.transform.localPosition = Vector3.zero;
        currentBubble.transform.localScale = Vector3.one;

        float elapsed = 0f;
        float warningTime = shieldDuration - 2f; // last 2 seconds warning

        SpriteRenderer sr = currentBubble.GetComponent<SpriteRenderer>();
        Color baseColor = (sr != null) ? sr.color : Color.white;

        while (elapsed < shieldDuration)
        {
            elapsed += Time.deltaTime;

            // 🔥 WARNING PHASE (last 2 seconds)
            if (elapsed >= warningTime && currentBubble != null)
            {
                // Pulse scale
                float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.15f;
                currentBubble.transform.localScale = Vector3.one * pulse;

                // Flicker alpha
                if (sr != null)
                {
                    Color c = baseColor;
                    c.a = 0.6f + Mathf.PingPong(Time.time * 3f, 0.4f); // 0.6 -> 1.0
                    sr.color = c;
                }
            }

            yield return null;
        }

        // Cleanup
        if (currentBubble != null)
            Destroy(currentBubble);

        IsShieldActive = false;
        shieldRoutine = null;
    }
}