using UnityEngine;

public class CrabPatrol : MonoBehaviour
{
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Random Movement")]
    public float minSpeed = 1.5f;
    public float maxSpeed = 3.0f;
    public float randomStartDelay = 0.4f;

    private float speed;
    private bool goingRight;
    private SpriteRenderer sr;
    private float delayTimer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // randomize speed + direction + delay
        speed = Random.Range(minSpeed, maxSpeed);
        goingRight = Random.value > 0.5f;
        delayTimer = Random.Range(0f, randomStartDelay);
    }

    void Update()
    {
        if (leftPoint == null || rightPoint == null) return;

        // random “desync” delay so they don't move together
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        Transform target = goingRight ? rightPoint : leftPoint;
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (sr != null) sr.flipX = !goingRight;

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
            goingRight = !goingRight;
    }
}
