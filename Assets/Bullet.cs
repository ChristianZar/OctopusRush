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
        // ✅ don't destroy when touching the player
        if (other.CompareTag("Player")) return;

        // destroy on anything solid
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
