using UnityEngine;

/// <summary>
/// Moves an obstacle from right to left and destroys it when off screen
/// Attached automatically by ObstacleSpawner or manually to obstacle prefabs
/// </summary>
public class ObstacleMover : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed the obstacle moves to the left")]
    public float moveSpeed = 3f;
    
    [Header("Destruction")]
    [Tooltip("X position where obstacle is destroyed")]
    public float destroyX = -12f;
    
    [Header("Rotation (Optional)")]
    [Tooltip("Enable rotation while moving")]
    public bool rotate = false;
    
    [Tooltip("Rotation speed (degrees per second)")]
    public float rotationSpeed = 30f;
    
    [Header("Bob Effect (Optional)")]
    [Tooltip("Enable bobbing up and down")]
    public bool bob = false;
    
    [Tooltip("Bob speed")]
    public float bobSpeed = 2f;
    
    [Tooltip("Bob amount")]
    public float bobAmount = 0.3f;
    
    private Vector3 startPosition;
    private float bobTimer = 0f;
    
    void Start()
    {
        startPosition = transform.position;
        bobTimer = Random.Range(0f, Mathf.PI * 2f); // Random start phase
    }
    
    void Update()
    {
        // Move left
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        
        // Optional rotation
        if (rotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        
        // Optional bobbing
        if (bob)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float yOffset = Mathf.Sin(bobTimer) * bobAmount;
            Vector3 pos = transform.position;
            pos.y = startPosition.y + yOffset;
            transform.position = pos;
            
            // Update start position to maintain bob
            startPosition.x = pos.x;
        }
        
        // Destroy when off screen
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Called when obstacle is destroyed
    /// </summary>
    void OnDestroy()
    {
        // Optional: Play destruction particle effect
        // Optional: Play sound effect
    }
}
