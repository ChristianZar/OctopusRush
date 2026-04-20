using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldOrbPowerUp : MonoBehaviour
{
    [Header("Enter From Top")]
    public float dropSpeed = 2.5f;
    public float targetY = 1.5f;

    [Header("Forward Zig-Zag")]
    public float forwardSpeed = 2.5f;
    public float zigZagAmplitude = 1.2f;
    public float zigZagFrequency = 2.5f;

    [Header("Pickup Sequence")]
    public float freezeTime = 0.20f;
    public float shakeTime = 0.35f;
    public float shakeAmount = 0.08f;

    [Header("FX (optional)")]
    public GameObject breakFxPrefab;

    [Header("Cleanup")]
    public Transform reference;
    public float destroyBehindDistance = 15f;

    [Header("Vertical Limits")]
    public float minZigZagY = -2.8f;
    public float maxZigZagY = 3.8f;

    private Vector3 pos;
    private bool collected;
    private bool startedZigZag;
    private float zigZagTimer;
    private float startY;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (reference == null && Camera.main != null)
            reference = Camera.main.transform;

        pos = transform.position;
        startY = pos.y;
    }

    void Update()
    {
        if (collected) return;

        if (!startedZigZag)
        {
            // Step 1: drop from top
            pos.y -= dropSpeed * Time.deltaTime;

            if (pos.y <= targetY)
            {
                pos.y = targetY;
                startedZigZag = true;
                startY = pos.y;
                zigZagTimer = 0f;
            }
        }
        else
        {
            // Step 2: move forward with bigger zig-zag
            zigZagTimer += Time.deltaTime;
            pos.x += forwardSpeed * Time.deltaTime;

            float waveY = startY + Mathf.Sin(zigZagTimer * zigZagFrequency) * zigZagAmplitude;

            // keep it within upper/lower bounds
            pos.y = Mathf.Clamp(waveY, minZigZagY, maxZigZagY);
        }

        transform.position = pos;

        if (reference != null && transform.position.x < reference.position.x - destroyBehindDistance)
        {
            Destroy(gameObject);
        }
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

        float old = Time.timeScale;
        Time.timeScale = 0f;

        AudioManager.Instance?.PlayShieldPickup();

        yield return new WaitForSecondsRealtime(freezeTime);
        yield return ShakeRealtime(shakeTime, shakeAmount);

        if (breakFxPrefab != null)
            Instantiate(breakFxPrefab, transform.position, Quaternion.identity);

        var shield = player.GetComponent<ShieldSystem>();
        if (shield != null)
            shield.ActivateShield();

        AchievementManager.Instance?.ReportShieldCollected();


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