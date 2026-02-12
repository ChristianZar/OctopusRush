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

        // Get width from the floor sprite
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            tileWidth = sr.bounds.size.x;
        else
            tileWidth = GetComponent<BoxCollider2D>().bounds.size.x;
    }

    void Update()
    {
        if (cam == null) return;

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
