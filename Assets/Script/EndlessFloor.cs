using UnityEngine;

public class EndlessFloor : MonoBehaviour
{
    public Transform cam;
    public float recycleOffset = 2f;

    private float tileWidth;

    void Start()
    {
        if (cam == null) cam = Camera.main.transform;

        // Automatically get the width of this floor tile
        tileWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        if (cam == null) return;

        // If this tile is fully behind the camera, move it forward
        if (transform.position.x + tileWidth < cam.position.x - recycleOffset)
        {
            transform.position += Vector3.right * (tileWidth * 2f);
        }
    }
}
