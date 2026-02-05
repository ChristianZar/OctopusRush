using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AutoForward : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }
}
