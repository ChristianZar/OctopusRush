using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SoftCeilingScreen : MonoBehaviour
{
    public Camera cam;                 // leave blank for Camera.main
    public float topMargin = 0.6f;     // how far below top edge the ceiling is (world units)
    public float pushStrength = 20f;   // stronger = harder push down
    public float damping = 0.6f;       // reduces upward velocity near ceiling

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (cam == null) cam = Camera.main;
    }

    void FixedUpdate()
    {
        if (cam == null) return;

        // Top of the camera view in world space (y)
        float topY = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y;
        float maxY = topY - topMargin;

        if (transform.position.y > maxY)
        {
            float overflow = transform.position.y - maxY;

            // push down (stronger the farther you go above)
            rb.AddForce(Vector2.down * pushStrength * (1f + overflow), ForceMode2D.Force);

            // damp upward velocity so you don't keep blasting upward
            if (rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * damping);
        }
    }
}
