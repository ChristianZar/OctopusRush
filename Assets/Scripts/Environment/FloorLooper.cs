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

        Camera cam = Camera.main;
        if (cam == null) return;

        float camHalfW = cam.orthographicSize * cam.aspect;
        float camLeft  = cam.transform.position.x - camHalfW;
        float camRight = cam.transform.position.x + camHalfW;

        // Recycle once the tile's right edge is fully off the left side of the screen
        if (transform.position.x + width < camLeft)
        {
            // Place the tile's left edge just past the camera right edge
            transform.position = new Vector3(camRight, transform.position.y, transform.position.z);

            var spawner = GetComponent<FloorCrabSpawner>();
            if (spawner != null)
            {
                spawner.ResetSpawner();
                spawner.SpawnCrabs();
            }
        }
    }
}
