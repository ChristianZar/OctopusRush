using UnityEngine;

/// <summary>
/// Obstacle that stays stationary in world space
/// Perfect for auto-scrolling games!
/// Player/camera moves past them
/// </summary>
public class ObstacleMover : MonoBehaviour
{
    [Header("Rotation (Optional)")]
    [Tooltip("Enable rotation while stationary")]
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
        // Obstacles stay in place - player moves past them!
        // NO MOVEMENT CODE HERE!
        
        // Optional rotation (looks cool for rocks/urchins)
        if (rotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        
        // Optional bobbing (looks cool for jellyfish/seaweed)
        if (bob)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float yOffset = Mathf.Sin(bobTimer) * bobAmount;
            Vector3 pos = transform.position;
            pos.y = startPosition.y + yOffset;
            transform.position = pos;
        }
    }
    
    // ObstacleSpawner handles destruction based on camera position
    // No need to check destroyX here!
}