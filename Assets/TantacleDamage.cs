using UnityEngine;

public class TentacleDamage : MonoBehaviour
{
    public int damage = 5;
    public float attackDuration = 0.15f;   // how long hitbox is active
    public float attackCooldown = 0.5f;

    private Collider2D col;
    private float nextAttackTime = 0f;
    private PlayerWeapon playerWeapon;

    void Start()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerWeapon = player.GetComponent<PlayerWeapon>();
    }

    void Update()
    {
        // Skip tentacle attack if player has a ranged weapon equipped (F key used for shooting)
        if (playerWeapon != null && playerWeapon.currentWeapon != WeaponType.None)
            return;

        if (Time.time < nextAttackTime) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(DoAttack());
        }
    }

    System.Collections.IEnumerator DoAttack()
    {
        col.enabled = true;
        yield return new WaitForSeconds(attackDuration);
        col.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        SharkHealth shark = other.GetComponentInParent<SharkHealth>();
        if (shark != null)
            shark.TakeDamage(damage);
    }
}
