using UnityEngine;

public class FloorLooper : MonoBehaviour
{
    private float width;

    void Start()
    {
        // Prefer SpriteRenderer width if present
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            width = sr.bounds.size.x;
        }
        else
        {
            width = GetComponent<BoxCollider2D>().bounds.size.x;
        }

        Debug.Log($"{gameObject.name} width = {width}");
    }

    void Update()
    {
        if (Camera.main.transform.position.x > transform.position.x + width)
        {
            transform.position += Vector3.right * width * 2f;
        }
    }
}
g