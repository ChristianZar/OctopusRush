using UnityEngine;

public class TentacleDamage : MonoBehaviour
{
    public int damage = 5;
    public float attackDuration = 0.15f;   // how long hitbox is active
    public float attackCooldown = 0.5f;

    private Collider2D col;
    private float nextAttackTime = 0f;

    void Start()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false; // off until you press attack
    }

    void Update()
    {
        if (Time.time < nextAttackTime) return;

        // Press F to attack (change key if you want)
       if (Input.GetKeyDown(KeyCode.F))

        {
             Debug.Log("F pressed -> Tentacle attack");
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
        // Only damage objects that have SharkHealth
        SharkHealth shark = other.GetComponentInParent<SharkHealth>();
        if (shark != null)
        {
            shark.TakeDamage(damage);
            Debug.Log("Tentacle HIT shark for " + damage);
        }
    }
}
