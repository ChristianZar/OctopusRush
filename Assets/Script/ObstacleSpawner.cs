using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// IMPROVED ObstacleSpawner with LARGER GAPS and better spacing
/// Makes the game more playable and fair!
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [Tooltip("Array of different obstacle prefabs to spawn")]
    public GameObject[] obstaclePrefabs;
    
    [Header("Spawn Settings")]
    [Tooltip("Distance ahead of player/camera to spawn")]
    public float spawnDistanceAhead = 15f;
    
    [Tooltip("Distance between obstacle spawns (INCREASED for easier gameplay)")]
    public float spawnSpacing = 8f; // Increased from 5 to 8!
    
    [Tooltip("Random variation in spawn spacing (+/- units)")]
    public float spacingVariation = 3f; // Increased from 2 to 3!
    
    [Header("Spawn Area")]
    [Tooltip("Minimum Y position for spawning")]
    public float minY = -3.5f; // Reduced range (was -4)
    
    [Tooltip("Maximum Y position for spawning")]
    public float maxY = 3.5f; // Reduced range (was 4)
    
    [Tooltip("Distance behind player/camera to destroy obstacles")]
    public float destroyDistanceBehind = 15f;
    
    [Header("Safe Zone")]
    [Tooltip("Create safe corridor in center (no spawns here)")]
    public bool useSafeZone = true;
    
    [Tooltip("Center safe zone range (obstacles won't spawn too close to Y=0)")]
    public float safeZoneRadius = 1.5f;
    
    [Header("Camera/Player Reference")]
    [Tooltip("The camera to track (will use Main Camera if not set)")]
    public Camera trackedCamera;
    
    [Tooltip("OR track the player directly")]
    public Transform playerTransform;
    
    [Tooltip("Track player instead of camera")]
    public bool trackPlayer = true;
    
    [Header("Spawn Patterns")]
    [Tooltip("Enable spawn patterns (groups of obstacles)")]
    public bool useSpawnPatterns = true;
    
    [Tooltip("Chance of spawning a pattern instead of single obstacle (REDUCED)")]
    [Range(0f, 1f)]
    public float patternChance = 0.15f; // Reduced from 0.3 to 0.15 (less patterns = easier)
    
    [Header("Gap Sizes (INCREASED for easier gameplay)")]
    [Tooltip("Gap size for vertical line patterns")]
    public float verticalGapSize = 3.5f; // Increased from 2.5 to 3.5!
    
    [Tooltip("Spacing between obstacles in horizontal patterns")]
    public float horizontalSpacing = 3f; // Increased from 2 to 3!
    
    [Tooltip("Cluster spread radius")]
    public float clusterRadius = 2f; // Increased from 1.5 to 2!
    
    [Header("Difficulty Settings")]
    [Tooltip("Increase difficulty over distance")]
    public bool increaseDifficulty = true;
    
    [Tooltip("Distance traveled before difficulty increases")]
    public float difficultyIncreaseDistance = 60f; // Increased from 50 to 60
    
    [Tooltip("How much to decrease spawn spacing each difficulty increase")]
    public float spacingDecreaseAmount = 0.3f; // Reduced from 0.5 to 0.3
    
    [Tooltip("Minimum spawn spacing (won't go below this)")]
    public float minSpawnSpacing = 4f; // Increased from 2 to 4!
    
    // Private variables
    private float lastSpawnX = 0f;
    private float nextSpawnX = 0f;
    private float currentSpawnSpacing;
    private float distanceTraveled = 0f;
    private float lastDifficultyIncreaseDistance = 0f;
    private Vector3 lastTrackedPosition;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private bool isSpawning = false;
    
    void Start()
    {
        // Get camera if not assigned
        if (trackedCamera == null)
        {
            trackedCamera = Camera.main;
        }
        
        // Get player if not assigned
        if (playerTransform == null && trackPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // Validate
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogError("ObstacleSpawner: No obstacle prefabs assigned!");
            enabled = false;
            return;
        }
        
        if (trackedCamera == null && playerTransform == null)
        {
            Debug.LogError("ObstacleSpawner: Need either a camera or player to track!");
            enabled = false;
            return;
        }
        
        // Initialize
        currentSpawnSpacing = spawnSpacing;
        
        // Get starting position
        Vector3 startPos = GetTrackedPosition();
        lastTrackedPosition = startPos;
        lastSpawnX = startPos.x;
        CalculateNextSpawnX();
        
        // Start spawning
        isSpawning = true;
        
        Debug.Log($"ObstacleSpawner initialized. Tracking: {(trackPlayer ? "Player" : "Camera")}");
        Debug.Log($"Easier settings: Gap={verticalGapSize}, Spacing={spawnSpacing}, SafeZone={safeZoneRadius}");
    }
    
    void Update()
    {
        if (!isSpawning) return;
        
        // Get current position
        Vector3 currentPos = GetTrackedPosition();
        
        // Calculate distance traveled
        float distanceThisFrame = currentPos.x - lastTrackedPosition.x;
        if (distanceThisFrame > 0)
        {
            distanceTraveled += distanceThisFrame;
        }
        lastTrackedPosition = currentPos;
        
        // Check if we need to spawn ahead
        float spawnThreshold = currentPos.x + spawnDistanceAhead;
        
        while (nextSpawnX < spawnThreshold)
        {
            SpawnObstacle();
            CalculateNextSpawnX();
        }
        
        // Update difficulty
        if (increaseDifficulty)
        {
            UpdateDifficulty();
        }
        
        // Clean up obstacles that are far behind
        CleanupObstacles(currentPos.x);
    }
    
    /// <summary>
    /// Get the position we're tracking (player or camera)
    /// </summary>
    Vector3 GetTrackedPosition()
    {
        if (trackPlayer && playerTransform != null)
        {
            return playerTransform.position;
        }
        else if (trackedCamera != null)
        {
            return trackedCamera.transform.position;
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Get safe Y position (avoids center safe zone)
    /// </summary>
    float GetSafeYPosition()
    {
        float yPos = Random.Range(minY, maxY);
        
        // If safe zone enabled, avoid spawning near center
        if (useSafeZone)
        {
            // If spawn would be in safe zone, push it out
            if (Mathf.Abs(yPos) < safeZoneRadius)
            {
                // Push to top or bottom of safe zone
                if (yPos >= 0)
                {
                    yPos = safeZoneRadius + Random.Range(0.5f, 1.5f);
                }
                else
                {
                    yPos = -safeZoneRadius - Random.Range(0.5f, 1.5f);
                }
                
                // Clamp to bounds
                yPos = Mathf.Clamp(yPos, minY, maxY);
            }
        }
        
        return yPos;
    }
    
    /// <summary>
    /// Spawns an obstacle at nextSpawnX
    /// </summary>
    void SpawnObstacle()
    {
        // Decide if spawning pattern or single obstacle
        bool spawnPattern = useSpawnPatterns && Random.value < patternChance;
        
        if (spawnPattern)
        {
            SpawnPattern(nextSpawnX);
        }
        else
        {
            SpawnSingleObstacle(nextSpawnX);
        }
    }
    
    /// <summary>
    /// Spawns a single random obstacle at specific X position
    /// </summary>
    void SpawnSingleObstacle(float xPos)
    {
        // Choose random obstacle prefab
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        
        // Safe Y position (avoids center corridor)
        float yPos = GetSafeYPosition();
        
        // Spawn position
        Vector3 spawnPos = new Vector3(xPos, yPos, 0f);
        
        // Create obstacle
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        
        // Parent to this spawner (optional, for organization)
        obstacle.transform.parent = transform;
        
        // Add to active list
        activeObstacles.Add(obstacle);
        
        Debug.Log($"Spawned {obstaclePrefab.name} at X={xPos:F1}, Y={yPos:F1}");
    }
    
    /// <summary>
    /// Spawns a pattern of obstacles at specific X position
    /// </summary>
    void SpawnPattern(float xPos)
    {
        // Choose random pattern type (reduced to 2 simpler patterns)
        int patternType = Random.Range(0, 2);
        
        switch (patternType)
        {
            case 0:
                SpawnVerticalLine(xPos);
                break;
            case 1:
                SpawnHorizontalSpread(xPos);
                break;
            // Removed diagonal and cluster - they're too hard!
        }
    }
    
    /// <summary>
    /// Spawns obstacles in a vertical line with a LARGE gap
    /// </summary>
    void SpawnVerticalLine(float xPos)
    {
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        
        // Create LARGE gap in middle
        float gapCenter = Random.Range(-1f, 1f); // Gap centered near middle
        
        // Spawn obstacles above and below gap
        // Increased spacing from 1.5 to 2 for easier dodging!
        for (float y = minY; y <= maxY; y += 2f)
        {
            // Skip LARGE gap area
            if (Mathf.Abs(y - gapCenter) < verticalGapSize / 2f)
                continue;
            
            Vector3 spawnPos = new Vector3(xPos, y, 0f);
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.parent = transform;
            
            activeObstacles.Add(obstacle);
        }
        
        Debug.Log($"Spawned vertical line at X={xPos:F1} with {verticalGapSize} gap");
    }
    
    /// <summary>
    /// Spawns obstacles spread horizontally with MORE space
    /// </summary>
    void SpawnHorizontalSpread(float xPos)
    {
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        float yPos = GetSafeYPosition();
        
        // Spawn only 2 obstacles (reduced from 3) with MORE spacing
        for (int i = 0; i < 2; i++)
        {
            float xOffset = i * horizontalSpacing; // Increased spacing!
            Vector3 spawnPos = new Vector3(xPos + xOffset, yPos, 0f);
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.parent = transform;
            
            activeObstacles.Add(obstacle);
        }
        
        Debug.Log($"Spawned horizontal spread (2 obstacles) at X={xPos:F1}");
    }
    
    /// <summary>
    /// Calculates next spawn X position with MORE spacing
    /// </summary>
    void CalculateNextSpawnX()
    {
        float variation = Random.Range(-spacingVariation, spacingVariation);
        nextSpawnX = lastSpawnX + currentSpawnSpacing + variation;
        lastSpawnX = nextSpawnX;
    }
    
    /// <summary>
    /// Increases difficulty based on distance traveled (SLOWER now)
    /// </summary>
    void UpdateDifficulty()
    {
        if (distanceTraveled - lastDifficultyIncreaseDistance >= difficultyIncreaseDistance)
        {
            lastDifficultyIncreaseDistance = distanceTraveled;
            
            // Decrease spacing (spawn more frequently) but SLOWER increase
            currentSpawnSpacing = Mathf.Max(currentSpawnSpacing - spacingDecreaseAmount, minSpawnSpacing);
            
            Debug.Log($"Difficulty increased at {distanceTraveled:F0} distance! New spacing: {currentSpawnSpacing:F1}");
        }
    }
    
    /// <summary>
    /// Removes obstacles that are far behind the player
    /// </summary>
    void CleanupObstacles(float currentX)
    {
        float destroyThreshold = currentX - destroyDistanceBehind;
        
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obstacle = activeObstacles[i];
            
            if (obstacle == null)
            {
                activeObstacles.RemoveAt(i);
            }
            else if (obstacle.transform.position.x < destroyThreshold)
            {
                Destroy(obstacle);
                activeObstacles.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Stop spawning obstacles
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
    }
    
    /// <summary>
    /// Start spawning obstacles
    /// </summary>
    public void StartSpawning()
    {
        isSpawning = true;
    }
    
    /// <summary>
    /// Clear all active obstacles
    /// </summary>
    public void ClearAllObstacles()
    {
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }
        activeObstacles.Clear();
    }
    
    /// <summary>
    /// Get total distance traveled
    /// </summary>
    public float GetDistanceTraveled()
    {
        return distanceTraveled;
    }
    
    /// <summary>
    /// Visualize spawn area in editor
    /// </summary>
    void OnDrawGizmos()
    {
        // Only draw when playing
        if (!Application.isPlaying) return;
        
        Vector3 trackedPos = GetTrackedPosition();
        
        // Draw spawn ahead line
        Gizmos.color = Color.green;
        float spawnX = trackedPos.x + spawnDistanceAhead;
        Gizmos.DrawLine(new Vector3(spawnX, minY, 0), new Vector3(spawnX, maxY, 0));
        
        // Draw destroy behind line
        Gizmos.color = Color.red;
        float destroyX = trackedPos.x - destroyDistanceBehind;
        Gizmos.DrawLine(new Vector3(destroyX, minY, 0), new Vector3(destroyX, maxY, 0));
        
        // Draw safe zone (center corridor)
        if (useSafeZone)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Vector3 safeCenter = new Vector3(trackedPos.x, 0, 0);
            Vector3 safeSize = new Vector3(50, safeZoneRadius * 2, 0.1f);
            Gizmos.DrawCube(safeCenter, safeSize);
        }
        
        // Draw next spawn position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(nextSpawnX, (minY + maxY) / 2f, 0), 0.5f);
    }
}