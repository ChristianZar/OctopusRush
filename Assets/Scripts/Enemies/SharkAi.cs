using UnityEngine;

public class SharkAI : MonoBehaviour
{
    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;

    [Header("Chase")]
    public float detectRadius = 5f;
    public float chaseSpeed = 3.5f;

    [Header("Return Home")]
    public float returnSpeed = 3f;
    public float homeSnapDistance = 0.1f;

    Transform player;
    Vector3 startPos;
    int dir = 1;

    enum State { Patrol, Chase, ReturnHome }
    State state = State.Patrol;

    [Header("Facing / BitePoint")]
public Transform bitePoint;     // drag BitePoint here
public float biteOffset = 0.5f; // how far in front
private float lastX;

[Header("Ink Effect")]
public float slowMultiplier = 0.4f;
public float slowDuration = 1.5f;

float slowTimer = 0f;
float originalPatrolSpeed;
float originalChaseSpeed;



    void Start()
    {
        startPos = transform.position;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        lastX = transform.position.x;

        originalPatrolSpeed = patrolSpeed;
        originalChaseSpeed = chaseSpeed;


    }

    void Update()
    {
        if (slowTimer > 0f)
{
    slowTimer -= Time.deltaTime;

    if (slowTimer <= 0f)
    {
        patrolSpeed = originalPatrolSpeed;
        chaseSpeed = originalChaseSpeed;
    }
}


        if (player == null)
        {
            state = State.Patrol;
            Patrol();
            return;
        }

        float d = Vector2.Distance(transform.position, player.position);

        // If player is close -> chase
        if (d <= detectRadius)
        {
            state = State.Chase;
        }
        else
        {
            // If player got away -> go back home first
            if (state == State.Chase)
                state = State.ReturnHome;
        }

        if (state == State.Chase) Chase();
        else if (state == State.ReturnHome) ReturnHome();
        else Patrol();
        UpdateFacing();

    }

    void Patrol()
    {
        // Always patrol around the original startPos
        float leftX = startPos.x - patrolDistance;
        float rightX = startPos.x + patrolDistance;

        Vector3 pos = transform.position;
        pos.x += dir * patrolSpeed * Time.deltaTime;

        // keep same Y as start (optional but helps for clean patrol lanes)
        pos.y = startPos.y;

        transform.position = pos;

        if (pos.x >= rightX) dir = -1;
        if (pos.x <= leftX) dir = 1;
    }

    void ReturnHome()
    {
        // Move to exact startPos (home) first
        Vector3 target = startPos;
        target.z = transform.position.z; // keep z same

        transform.position = Vector3.MoveTowards(transform.position, target, returnSpeed * Time.deltaTime);

        // Once we reach home, resume patrol
        if (Vector2.Distance(transform.position, startPos) <= homeSnapDistance)
        {
            transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
            state = State.Patrol;
        }
    }

    void Chase()
    {
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        transform.position += (Vector3)dirToPlayer * chaseSpeed * Time.deltaTime;
    }

    void UpdateFacing()
{
    float dx = transform.position.x - lastX;

    if (Mathf.Abs(dx) > 0.0001f)
    {
        bool movingRight = dx > 0;

        // Flip the shark sprite by scaling X
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (movingRight ? 1 : -1);
        transform.localScale = s;

        // Move the bite point to the front
        if (bitePoint != null)
        {
            Vector3 local = bitePoint.localPosition;
            local.x = (movingRight ? 1 : -1) * Mathf.Abs(biteOffset);
            bitePoint.localPosition = local;
        }
    }

    lastX = transform.position.x;
}

public void ApplyInkSlow()
{
    slowTimer = slowDuration;
    patrolSpeed = originalPatrolSpeed * slowMultiplier;
    chaseSpeed  = originalChaseSpeed  * slowMultiplier;
}



}
