using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f;
    public int damage = 20; // change this number

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ don't destroy when touching the player
        if (other.CompareTag("Player")) return;

        // ✅ if we hit a shark, damage it + destroy bullet
       SharkHealth sharkHealth = other.GetComponentInParent<SharkHealth>();
if (sharkHealth != null)
{
    sharkHealth.TakeDamage(damage, transform.position);
    Destroy(gameObject);
    return;
}


        // ✅ destroy on anything solid (same behavior as your working script)
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
