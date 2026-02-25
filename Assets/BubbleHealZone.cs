using UnityEngine;

public class BubbleHealZone : MonoBehaviour
{
    public int healAmount = 1;
    public float healCooldown = 0.3f;

    private float nextHealTime;
    private BoxCollider2D box;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        Debug.Log("[BubbleHealZone] Awake on " + gameObject.name + "  box=" + (box != null));
    }

    void FixedUpdate()
    {
        // always tell us if this script is alive
        if (Time.frameCount % 60 == 0)
            Debug.Log("[BubbleHealZone] running... timeScale=" + Time.timeScale);

        if (box == null) return;

        // find player by tag
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            if (Time.frameCount % 60 == 0) Debug.LogWarning("[BubbleHealZone] No object with tag Player found.");
            return;
        }

        var ph = player.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            if (Time.frameCount % 60 == 0) Debug.LogWarning("[BubbleHealZone] Player has no PlayerHealth.");
            return;
        }

        // cooldown uses unscaled time so it still works if timeScale=0
        float now = Time.unscaledTime;
        if (now < nextHealTime) return;

        var playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) return;

        bool inside = box.bounds.Intersects(playerCol.bounds);

        if (inside)
        {
            Debug.Log("[BubbleHealZone] OVERLAP -> trying heal. hp=" + ph.currentHealth + "/" + ph.maxHealth);
            ph.Heal(healAmount);
            nextHealTime = now + healCooldown;
        }
    }

    void OnDrawGizmos()
    {
        var b = GetComponent<BoxCollider2D>();
        if (b == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(b.offset, b.size);
    }
}