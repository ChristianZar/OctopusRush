using UnityEngine;

/// <summary>
/// Creates a scrolling/parallax background effect for the menu
/// Perfect for underwater atmosphere!
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scrolling Settings")]
    [Tooltip("Speed of background scrolling")]
    public Vector2 scrollSpeed = new Vector2(0.02f, 0.01f);
    
    [Tooltip("Enable parallax effect with multiple layers")]
    public bool enableParallax = true;
    
    [Tooltip("Parallax multiplier (closer layers move faster)")]
    [Range(0f, 2f)]
    public float parallaxMultiplier = 1f;

    [Header("Floating Animation")]
    [Tooltip("Enable gentle floating motion")]
    public bool enableFloating = true;
    
    [Tooltip("Floating speed")]
    public float floatSpeed = 0.5f;
    
    [Tooltip("Floating amplitude (how far it moves)")]
    public float floatAmplitude = 0.3f;

    private Material material;
    private Vector2 offset;
    private Vector3 startPosition;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        
        if (rend != null && rend.material != null)
        {
            // Create instance of material to avoid changing shared material
            material = rend.material;
        }
        
        startPosition = transform.position;
    }

    void Update()
    {
        // Scrolling effect using material offset
        if (material != null)
        {
            offset += scrollSpeed * Time.deltaTime * parallaxMultiplier;
            material.mainTextureOffset = offset;
        }

        // Gentle floating motion
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnDestroy()
    {
        // Clean up material instance
        if (material != null)
        {
            Destroy(material);
        }
    }
}
