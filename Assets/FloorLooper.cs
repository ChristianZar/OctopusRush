using UnityEngine;

public class FloorLooper : MonoBehaviour
{
    private float width;

    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) width = sr.bounds.size.x;
        else width = GetComponent<BoxCollider2D>().bounds.size.x;
    }

    void Update()
    {
        if (width <= 0)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.bounds.size.x > 0)
                width = sr.bounds.size.x;
            else
            {
                var col = GetComponent<BoxCollider2D>();
                if (col == null) col = GetComponentInChildren<BoxCollider2D>();
                if (col != null) width = col.bounds.size.x;
            }
            if (width <= 0) return;
        }

        if (Camera.main != null && Camera.main.transform.position.x > transform.position.x + width)
        {
            transform.position += Vector3.right * width * 2f;

            var spawner = GetComponent<FloorCrabSpawner>();
            if (spawner != null)
            {
                spawner.ResetSpawner();
                spawner.SpawnCrabs();
            }
        }
    }
}
