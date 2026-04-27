using UnityEngine;

public class PufferFishBehavior : MonoBehaviour
{
    [Header("Cycle Timing")]
    public float normalTime = 2.5f;
    public float inflatedTime = 2f;

    [Header("Damage / Reward")]
    public int contactDamage = 1;
    public int eatHeal = 2;       // HP restored when eating a deflated puffer

    [Header("Eat FX")]
    public GameObject bloodPuffPrefab;
    public Vector3 bloodOffset = Vector3.zero;

    private Animator anim;
    private bool isInflated = false;
    private float stateTimer;
    private Camera cam;

    void Start()
    {
        anim = GetComponent<Animator>();
        stateTimer = normalTime;
        cam = Camera.main;

        if (anim != null)
            anim.SetBool("IsInflated", false);
    }

    void Update()
    {
        if (cam != null && transform.position.x > cam.transform.position.x + cam.orthographicSize * cam.aspect + 5f) return;
        if (anim == null) return;

        stateTimer -= Time.deltaTime;

        if (!isInflated)
        {
            if (stateTimer <= 0f)
            {
                isInflated = true;
                stateTimer = inflatedTime;

                anim.SetTrigger("InflateNow");
                anim.SetBool("IsInflated", true);
            }
        }
        else
        {
            if (stateTimer <= 0f)
            {
                isInflated = false;
                stateTimer = normalTime;

                anim.SetBool("IsInflated", false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null) return;
        if (ph.IsDead()) return;

        if (isInflated)
        {
            ph.TakeDamage(contactDamage);
        }
        else
        {
            if (bloodPuffPrefab != null)
                Instantiate(bloodPuffPrefab, transform.position + bloodOffset, Quaternion.identity);

            ph.Heal(eatHeal);
            AchievementManager.Instance?.ReportPufferfishEaten();
            Destroy(gameObject);
        }
    }
}