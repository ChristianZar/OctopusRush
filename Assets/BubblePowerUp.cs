using UnityEngine;

public class BubblePowerup : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public Vector2 drift = new Vector2(1f, 0.2f); // forward + slight up/down
    public float lifetime = 8f;

    [Header("Powerup")]
    public WeaponType weaponToGrant = WeaponType.AK47;

    [Header("Pickup Delay")]
    public float pickupDelay = 2f;   // ✅ wait before collectible

    [Header("FX (optional)")]
    public GameObject popFxPrefab;

    private float t;
    private float spawnTime;
    private Collider2D myCollider;

    void Start()
    {
        Destroy(gameObject, lifetime);
        drift = drift.normalized;

        spawnTime = Time.time;

        // ✅ disable pickup collider at first
        myCollider = GetComponent<Collider2D>();
        if (myCollider != null)
            myCollider.enabled = false;
    }

    void Update()
    {
        // ✅ enable pickup after delay
        if (myCollider != null && !myCollider.enabled && Time.time >= spawnTime + pickupDelay)
            myCollider.enabled = true;

        // Smooth little float wobble
        t += Time.deltaTime;
        float wobble = Mathf.Sin(t * 3f) * 0.15f;

        Vector3 move = new Vector3(drift.x, drift.y + wobble, 0f) * speed * Time.deltaTime;
        transform.position += move;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // extra safety (in case collider gets enabled weirdly)
        if (Time.time < spawnTime + pickupDelay) return;

        if (!other.CompareTag("Player")) return;

        // Give the weapon to the player
        var weapon = other.GetComponent<PlayerWeapon>();
        if (weapon != null)
        {
            weapon.Equip(weaponToGrant);
        }

        // Pop FX (optional)
        if (popFxPrefab != null)
            Instantiate(popFxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
