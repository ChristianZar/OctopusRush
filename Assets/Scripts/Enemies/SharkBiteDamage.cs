using UnityEngine;

public class SharkBiteDamage : MonoBehaviour
{
    public int damagePerTick = 1;
    public float tickRate = 0.35f;

    float nextTickTime = 0f;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null || hp.IsDead()) return;

        if (Time.time >= nextTickTime)
        {
            nextTickTime = Time.time + tickRate;
            hp.TakeDamage(damagePerTick);
            Debug.Log("Biting... -" + damagePerTick);
        }
    }
}
