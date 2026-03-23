using UnityEngine;

public class EndlessFloor : MonoBehaviour
{
    public Transform cam;
    public float recycleOffset = 2f;

    private float tileWidth;

    void Start()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        InitTileWidth();
    }

    void InitTileWidth()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.bounds.size.x > 0)
        {
            tileWidth = sr.bounds.size.x;
            return;
        }

        var col = GetComponent<BoxCollider2D>();
        if (col == null) col = GetComponentInChildren<BoxCollider2D>();
        if (col != null)
            tileWidth = col.bounds.size.x;
    }

    void Update()
    {
        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else return;
        }

        if (tileWidth <= 0)
        {
            InitTileWidth();
            return;
        }

        // If this tile is fully behind the camera, move it forward
        if (transform.position.x + tileWidth < cam.position.x - recycleOffset)
        {
            transform.position += Vector3.right * (tileWidth * 2f);

            // spawn fresh crabs for the recycled tile
            var spawner = GetComponent<FloorCrabSpawner>();
            if (spawner != null)
            {
                spawner.ResetSpawner();
                spawner.SpawnCrabs();
            }
        }
    }
}
