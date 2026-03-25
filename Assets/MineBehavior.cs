using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBehavior : MonoBehaviour
{
    [Header("Sprites (assign in Inspector)")]
    public Sprite idleSprite;       // Frame 1 – mine sitting still
    public Sprite warningSprite;    // Frame 2 – shaking, bubbles forming
    public Sprite explode1Sprite;   // Frame 3 – explosion start
    public Sprite explode2Sprite;   // Frame 4 – explosion end

    [Header("Timing")]
    public float warningDuration  = 1.5f;   // seconds of shaking before boom
    public float explodeFrameTime = 0.12f;  // each explosion frame duration
    public float chainDelay       = 0.15f;  // stagger between each linked mine exploding

    [Header("Damage")]
    public int   damage       = 2;
    public float damageRadius = 1.5f;

    [Header("Screen Shake")]
    public float shakeDuration  = 0.35f;
    public float shakeMagnitude = 0.18f;

    [Header("Warning Shake")]
    public float shakeAmount = 0.06f;
    public float shakeSpeed  = 25f;

    [Header("Red Pulse")]
    public float pulseStartSpeed = 2f;   // flashes per second at start of warning
    public float pulseEndSpeed   = 10f;  // flashes per second just before explosion

    // Set by MineSpawner when the cluster is created
    [HideInInspector] public List<MineBehavior> linkedMines = new List<MineBehavior>();

    private SpriteRenderer sr;
    private Vector3        startPos;
    internal bool          triggered = false;

    void Awake()
    {
        sr       = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    void Start()
    {
        if (sr != null && idleSprite != null)
            sr.sprite = idleSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(WarningThenExplode());
    }

    // Called by linked cluster-mates to chain-detonate this mine
    public void TriggerExplosion(float delay = 0f)
    {
        if (triggered) return;
        triggered = true;
        if (delay > 0f)
            StartCoroutine(DelayedExplode(delay));
        else
            StartCoroutine(ExplodeSequence());
    }

    IEnumerator DelayedExplode(float delay)
    {
        if (sr != null && warningSprite != null)
            sr.sprite = warningSprite;

        // Pulse red while waiting for the chain delay
        float elapsed = 0f;
        while (elapsed < delay)
        {
            float pulse = Mathf.Abs(Mathf.Sin(elapsed * pulseEndSpeed * Mathf.PI));
            if (sr != null)
                sr.color = Color.Lerp(Color.white, Color.red, pulse);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sr != null) sr.color = Color.red;
        yield return StartCoroutine(ExplodeSequence());
    }

    IEnumerator WarningThenExplode()
    {
        if (sr != null && warningSprite != null)
            sr.sprite = warningSprite;

        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            float t = elapsed / warningDuration;  // 0 → 1 over warning duration

            // Shake
            float offsetX = Mathf.Sin(elapsed * shakeSpeed) * shakeAmount;
            transform.position = startPos + new Vector3(offsetX, 0f, 0f);

            // Red pulse — speed and intensity increase as explosion approaches
            float pulseSpeed = Mathf.Lerp(pulseStartSpeed, pulseEndSpeed, t);
            float pulse      = Mathf.Abs(Mathf.Sin(elapsed * pulseSpeed * Mathf.PI));
            if (sr != null)
                sr.color = Color.Lerp(Color.white, Color.red, pulse * t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sr != null) sr.color = Color.red;
        transform.position = startPos;
        yield return StartCoroutine(ExplodeSequence());
    }

    IEnumerator ExplodeSequence()
    {
        transform.position = startPos;

        if (sr != null)
        {
            sr.color  = Color.white;
            if (explode1Sprite != null) sr.sprite = explode1Sprite;
        }

        DealDamage();
        ShakeCamera();
        TriggerLinkedMines();
        AudioManager.Instance?.PlayMineExplode();

        yield return new WaitForSeconds(explodeFrameTime);

        if (sr != null && explode2Sprite != null)
            sr.sprite = explode2Sprite;

        yield return new WaitForSeconds(explodeFrameTime);

        Destroy(gameObject);
    }

    void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth ph = hit.GetComponentInParent<PlayerHealth>();
            if (ph != null && !ph.IsDead())
            {
                ph.TakeDamage(damage);
                break;
            }
        }
    }

    void ShakeCamera()
    {
        CameraShake shake = Camera.main != null
            ? Camera.main.GetComponent<CameraShake>()
            : null;
        if (shake != null)
            shake.Shake(shakeDuration, shakeMagnitude);
    }

    void TriggerLinkedMines()
    {
        for (int i = 0; i < linkedMines.Count; i++)
        {
            if (linkedMines[i] != null)
                linkedMines[i].TriggerExplosion((i + 1) * chainDelay);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
