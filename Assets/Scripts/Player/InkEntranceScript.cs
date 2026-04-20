using System.Collections;
using UnityEngine;

public class InkEntranceScript : MonoBehaviour
{
    [Header("Ink Cloud Prefab")]
    public GameObject inkCloudPrefab;

    [Header("Timing")]
    public float delay = 1f;

    [Header("Pop Effect")]
    public float popScale = 1.25f;
    public float popDuration = 0.2f;

    [Header("Disable these scripts during intro (optional)")]
    public MonoBehaviour[] disableDuringIntro;

    private SpriteRenderer sr;
    private Vector3 originalScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // Hide player at start
        if (sr != null) sr.enabled = false;

        // Disable movement/shooting scripts during intro (optional)
        if (disableDuringIntro != null)
        {
            foreach (var s in disableDuringIntro)
                if (s != null) s.enabled = false;
        }

        // Spawn ink cloud (if assigned)
        if (inkCloudPrefab != null)
            Instantiate(inkCloudPrefab, transform.position, Quaternion.identity);

        // Start intro sequence
        StartCoroutine(DoEntrance());
    }

    private IEnumerator DoEntrance()
    {
        yield return new WaitForSeconds(delay);

        // Show player
        if (sr != null) sr.enabled = true;

        // Pop effect
        transform.localScale = originalScale * popScale;
        float t = 0f;

        while (t < popDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, t / popDuration);
            yield return null;
        }
        transform.localScale = originalScale;

        // Re-enable scripts
        if (disableDuringIntro != null)
        {
            foreach (var s in disableDuringIntro)
                if (s != null) s.enabled = true;
        }
    }
}
