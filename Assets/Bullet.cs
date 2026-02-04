using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Optional: destroy bullet when it hits enemies
        // if (other.CompareTag("Enemy")) { ... }
        // Destroy(gameObject);

        // For now, just destroy on anything solid:
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
