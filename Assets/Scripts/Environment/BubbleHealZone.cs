using UnityEngine;

public class BubbleHealZone : MonoBehaviour
{
    public int healAmount = 1;
    public float healCooldown = 0.3f;

    private float nextHealTime;
    private BoxCollider2D box;
    private PlayerHealth playerHealth;
    private Collider2D playerCol;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerCol = player.GetComponent<Collider2D>();
        }
    }

    void FixedUpdate()
    {
        if (box == null || playerHealth == null || playerCol == null) return;

        float now = Time.time;
        if (now < nextHealTime) return;

        if (box.bounds.Intersects(playerCol.bounds))
        {
            playerHealth.Heal(healAmount);
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