using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldOrbPowerUp : MonoBehaviour
{
    [Header("Enter From Ceiling")]
    public bool enterFromTop = true;
    public float enterDistance = 3.5f;      // how far above it starts
    public float enterDuration = 0.6f;      // how fast it glides down

    [Header("Forward Drift (Player chases it)")]
    public float forwardSpeed = 2.2f;       // set slightly faster than camera scroll

    [Header("Bob (after it enters)")]
    public float bobSpeed = 2f;
    public float bobAmount = 0.25f;

    [Header("Pickup Sequence")]
    public float freezeTime = 0.20f;
    public float shakeTime = 0.35f;
    public float shakeAmount = 0.08f;

    [Header("FX (optional)")]
    public GameObject breakFxPrefab;

    [Header("Cleanup")]
public Transform reference;
public float destroyBehindDistance = 15f;

    Vector3 pos;            // we’ll manage position manually
    float baseY;            // target Y center for bob
    bool collected;
    bool entered;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (reference == null && Camera.main != null)
    reference = Camera.main.transform;

        pos = transform.position;
        baseY = pos.y;

        if (enterFromTop)
        {
            // start above ceiling and glide down to baseY
            pos.y = baseY + enterDistance;
            transform.position = pos;
            StartCoroutine(EnterRoutine(pos.y, baseY));
        }
        else
        {
            entered = true;
        }
    }

    void Update()
    {
        if (collected) return;

        // move forward always
        pos.x += forwardSpeed * Time.deltaTime;

        // bob only after entering
        if (entered)
        {
            float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            pos.y = baseY + yOffset;
        }

        transform.position = pos;

        // Destroy if far behind the camera/player so spawner can make a new one
if (reference != null && transform.position.x < reference.position.x - destroyBehindDistance)
{
    Destroy(gameObject);
}
    }

    IEnumerator EnterRoutine(float startY, float targetY)
    {
        float t = 0f;
        while (t < enterDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / enterDuration);
            pos.y = Mathf.Lerp(startY, targetY, k);
            transform.position = pos;
            yield return null;
        }

        baseY = targetY;
        entered = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(CollectRoutine(other.gameObject));
    }

    IEnumerator CollectRoutine(GameObject player)
    {
        collected = true;

        // Freeze world briefly
        float old = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(freezeTime);
        yield return ShakeRealtime(shakeTime, shakeAmount);

        if (breakFxPrefab != null)
            Instantiate(breakFxPrefab, transform.position, Quaternion.identity);

        // Activate shield
        var shield = player.GetComponent<ShieldSystem>();
        if (shield != null)
            shield.ActivateShield();

        Time.timeScale = old;
        Destroy(gameObject);
    }

    IEnumerator ShakeRealtime(float duration, float amount)
    {
        Vector3 original = transform.position;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            transform.position = original + new Vector3(
                Random.Range(-amount, amount),
                Random.Range(-amount, amount),
                0f
            );
            yield return null;
        }

        transform.position = original;
    }
}