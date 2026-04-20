using UnityEngine;

public class EndlessFloor : MonoBehaviour
{
    [Header("Camera")]
    public Camera cam;
    public float recycleOffset = 2f;   // world units behind left edge before recycling

    [Header("Gap (Deep Hole)")]
    [Tooltip("Minimum world units of empty space before the tile reappears on the right.")]
    public float minGapX = 1f;
    [Tooltip("Maximum world units of empty space — wider = deeper-looking hole.")]
    public float maxGapX = 4f;

    private float tileWidth;
    private FloorDecoSpawner decoSpawner;

    void Start()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main;

        InitTileWidth();
        decoSpawner = GetComponent<FloorDecoSpawner>();
        decoSpawner?.Init(tileWidth);
    }

    void InitTileWidth()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.bounds.size.x > 0) { tileWidth = sr.bounds.size.x; return; }

        var col = GetComponent<BoxCollider2D>();
        if (col == null) col = GetComponentInChildren<BoxCollider2D>();
        if (col != null) tileWidth = col.bounds.size.x;
    }

    void Update()
    {
        if (cam == null) { cam = Camera.main; return; }

        if (tileWidth <= 0) { InitTileWidth(); return; }

        float leftEdge = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;

        // Tile right edge (sprite is center-pivoted, so right edge = pos + halfWidth)
        // Recycle once the tile's right edge clears the left side of the screen
        if (transform.position.x + tileWidth * 0.5f < leftEdge - recycleOffset)
        {
            // #1 & #3: jump to camera right edge + random gap
            float rightEdge = cam.ViewportToWorldPoint(
                new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;

            float gap = Random.Range(minGapX, maxGapX);

            Vector3 p = transform.position;
            p.x = rightEdge + gap;
            transform.position = p;

            // Refresh crabs and decorations for the "new" tile
            var crabSpawner = GetComponent<FloorCrabSpawner>();
            if (crabSpawner != null) { crabSpawner.ResetSpawner(); crabSpawner.SpawnCrabs(); }

            decoSpawner?.Refresh(tileWidth);
        }
    }
}
