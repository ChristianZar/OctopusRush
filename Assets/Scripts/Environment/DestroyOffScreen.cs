using UnityEngine;

public class DestroyOffscreen : MonoBehaviour
{
    [SerializeField] private float extraBelow = 1.5f; // delete this far below camera bottom
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Works for Orthographic 2D cameras
        float bottomY = cam.transform.position.y - cam.orthographicSize;

        if (transform.position.y < bottomY - extraBelow)
        {
            Destroy(gameObject);
        }
    }
}
