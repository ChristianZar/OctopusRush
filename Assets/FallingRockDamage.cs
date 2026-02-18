using UnityEngine;

public class FallingRockDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float minFallSpeedToHurt = 0f;
    [SerializeField] private bool destroyOnHitPlayer = true;

    Rigidbody2D rb;
    bool hitOnce;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void TryHit(Collider2D col)
    {
        if (hitOnce) return;

        // if the collider is a child, this still finds PlayerHealth on parent
        var hp = col.GetComponentInParent<PlayerHealth>();
        if (hp == null) return;

        float speed = rb ? rb.linearVelocity.magnitude : 999f;
        if (speed < minFallSpeedToHurt) return;

        hitOnce = true;
        hp.TakeDamage(damage);

        if (destroyOnHitPlayer) Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D c) => TryHit(c.collider);
    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
}
