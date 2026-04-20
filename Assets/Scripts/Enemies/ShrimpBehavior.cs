using UnityEngine;

public class ShrimpBehavior : MonoBehaviour
{
    [Header("Normal Swim")]
    public float swimSpeed = 2f;
    public float wiggleAmount = 0.4f;
    public float wiggleSpeed = 5f;

    [Header("Run Away")]
    public float scareRadius = 2.5f;
    public float fleeSpeed = 5f;
    public float smoothSpeed = 6f;

    private Transform player;
    private Vector2 currentMove;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        Vector2 targetMove;

        if (dist <= scareRadius)
        {
            // shrimp flees away from octopus
            targetMove = ((Vector2)transform.position - (Vector2)player.position).normalized * fleeSpeed;
        }
        else
        {
            // normal swim to the left
            targetMove = Vector2.left * swimSpeed;
        }

        // slight wiggle
        targetMove.y += Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;

        currentMove = Vector2.Lerp(currentMove, targetMove, smoothSpeed * Time.deltaTime);
        transform.position += (Vector3)(currentMove * Time.deltaTime);

        if (sr != null)
            sr.flipX = currentMove.x > 0f;
    }
}