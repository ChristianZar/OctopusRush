using UnityEngine;

public class AnglerFishAmbush : MonoBehaviour
{
    public Transform player;
    public GameObject fishBody;
    public GameObject lure;

    public float attackRadius = 3f;
    public float loseRadius = 6f;

    public float lureMoveDistance = 0.2f;
    public float lureMoveSpeed = 2f;

    public float attackSpeed = 3f;

    [Header("Damage")]
    public int damageAmount = 1;
    public float damageCooldown = 0.75f;

    private Vector3 lureStartPos;
    private bool attacking = false;
    private float damageTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (lure != null)
            lureStartPos = lure.transform.localPosition;

        if (fishBody != null)
            fishBody.SetActive(false);

        if (lure != null)
            lure.SetActive(true);
    }

    void Update()
    {
        if (player == null) return;

       float dist = Vector2.Distance(lure.transform.position, player.position);

        if (!attacking)
        {
            MoveLure();

            if (dist < attackRadius)
            {
                attacking = true;

                if (lure != null)
                    lure.SetActive(false);

                if (fishBody != null)
                    fishBody.SetActive(true);
            }
        }
        else
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * attackSpeed * Time.deltaTime);

            damageTimer -= Time.deltaTime;

            // Damage when close enough while attacking
            if (dist < 0.8f && damageTimer <= 0f)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null && !ph.IsDead())
                {
                    ph.TakeDamage(damageAmount);
                    damageTimer = damageCooldown;
                }
            }

            if (dist > loseRadius)
            {
                attacking = false;

                if (fishBody != null)
                    fishBody.SetActive(false);

                if (lure != null)
                {
                    lure.SetActive(true);
                    lure.transform.localPosition = lureStartPos;
                }
            }
        }
    }

    void MoveLure()
    {
        if (lure == null) return;

        float offset = Mathf.Sin(Time.time * lureMoveSpeed) * lureMoveDistance;
        Vector3 pos = lureStartPos;
        pos.x += offset;
        lure.transform.localPosition = pos;
    }
}