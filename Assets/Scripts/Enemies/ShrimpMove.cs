using UnityEngine;

public class ShrimpMove : MonoBehaviour
{
    public float speed = 2f;
    public float range = 2f;

    private Vector3 startPos;
    private SpriteRenderer sr;

    void Start()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float move = Mathf.Sin(Time.time * speed) * range;
        transform.position = new Vector3(startPos.x + move, startPos.y, transform.position.z);

        if (sr != null)
            sr.flipX = move > 0;
    }
}