using UnityEngine;

public class SharkBiteDamage : MonoBehaviour
{
    public int damagePerTick = 1;
    public float tickRate = 0.35f;   // how often damage happens while touching

    float nextTickTime = 0f;

    void OnTriggerStay2D(Collider2D other)
    {
        // only bite the player
        if (!other.CompareTag("Player")) return;

        if (Time.time >= nextTickTime)
        {
            nextTickTime = Time.time + tickRate;

            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damagePerTick);
                Debug.Log("Biting... -" + damagePerTick);
            }
        }
    }
}
