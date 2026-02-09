using UnityEngine;
using System.Collections.Generic;

/// <summary>
<<<<<<< Updated upstream
/// UPDATED ObstacleSpawner for scrolling/moving camera gameplay
/// Spawns obstacles ahead of the player as they move forward
/// Perfect for games where player keeps moving!
=======
/// Spawns both floor decorations (coral, rock, seaweed) and floating obstacles (sea urchin, jellyfish)
/// Decorations are placed on the ground for visual appeal
/// Obstacles float in the water as hazards
>>>>>>> Stashed changes
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Floor Decorations (Non-Obstacles)")]
    [Tooltip("Decorative sprites that spawn on the floor")]
    public GameObject[] floorDecorations; // coral, rock, seaweed
    
    [Tooltip("Floor Y position where decorations spawn")]
    public float floorY = -4.5f;
    
    [Tooltip("Random Y variation for floor decorations")]
    public float floorYVariation = 0.2f;
    
    [Header("Floating Obstacles (Hazards)")]
    [Tooltip("Obstacle prefabs that float in water (sea urchin, jellyfish)")]
    public GameObject[] floatingObstacles; // sea urchin, jellyfish
    
    [Header("Spawn Settings")]
    [Tooltip("Distance ahead of player/camera to spawn")]
    public float spawnDistanceAhead = 15f;
    
<<<<<<< Updated upstream
    [Tooltip("Distance between obstacle spawns")]
    public float spawnSpacing = 5f;
    
    [Tooltip("Random variation in spawn spacing (+/- units)")]
    public float spacingVariation = 2f;
    
    [Header("Spawn Area")]
    [Tooltip("Minimum Y position for spawning")]
    public float minY = -4f;
    
    [Tooltip("Maximum Y position for spawning")]
    public float maxY = 4f;
=======
    [Tooltip("Distance between spawns (INCREASED for easier gameplay)")]
    public float spawnSpacing = 8f;
    
    [Tooltip("Random variation in spawn spacing (+/- units)")]
    public float spacingVariation = 3f;
    
    [Tooltip("Chance to spawn decoration instead of obstacle")]
    [Range(0f, 1f)]
    public float decorationChance = 0.5f; // 50% decoration, 50% obstacle
    
    [Header("Floating Obstacle Area")]
    [Tooltip("Minimum Y position for floating obstacles")]
    public float minY = -3.5f;
>>>>>>> Stashed changes
    
    [Tooltip("Maximum Y position for floating obstacles")]
    public float maxY = 3.5f;
    
    [Tooltip("Distance behind player/camera to destroy")]
    public float destroyDistanceBehind = 15f;
    
<<<<<<< Updated upstream
=======
    [Header("Safe Zone (For Floating Obstacles)")]
    [Tooltip("Create safe corridor in center (no floating obstacles here)")]
    public bool useSafeZone = true;
    
    [Tooltip("Center safe zone radius (obstacles won't spawn too close to Y=0)")]
    public float safeZoneRadius = 1.5f;
    
>>>>>>> Stashed changes
    [Header("Camera/Player Reference")]
    [Tooltip("The camera to track (will use Main Camera if not set)")]
    public Camera trackedCamera;
    
    [Tooltip("OR track the player directly")]
    public Transform playerTransform;
    
    [Tooltip("Track player instead of camera")]
    public bool trackPlayer = true;
    
    [Header("Spawn Patterns (Floating Obstacles Only)")]
    [Tooltip("Enable spawn patterns (groups of obstacles)")]
    public bool useSpawnPatterns = true;
    
    [Tooltip("Chance of spawning a pattern instead of single obstacle (0-1)")]
    [Range(0f, 1f)]
<<<<<<< Updated upstream
    public float patternChance = 0.3f;
=======
    public float patternChance = 0.15f;
    
    [Header("Gap Sizes (For Patterns)")]
    [Tooltip("Gap size for vertical line patterns")]
    public float verticalGapSize = 3.5f;
    
    [Tooltip("Spacing between obstacles in horizontal patterns")]
    public float horizontalSpacing = 3f;
>>>>>>> Stashed changes
    
    [Header("Difficulty Settings")]
    [Tooltip("Increase difficulty over distance")]
    public bool increaseDifficulty = true;
    
    [Tooltip("Distance traveled before difficulty increases")]
<<<<<<< Updated upstream
    public float difficultyIncreaseDistance = 50f;
    
    [Tooltip("How much to decrease spawn spacing each difficulty increase")]
    public float spacingDecreaseAmount = 0.5f;
    
    [Tooltip("Minimum spawn spacing (won't go below this)")]
    public float minSpawnSpacing = 2f;
=======
    public float difficultyIncreaseDistance = 60f;
    
    [Tooltip("How much to decrease spawn spacing each difficulty increase")]
    public float spacingDecreaseAmount = 0.3f;
    
    [Tooltip("Minimum spawn spacing (won't go below this)")]
    public float minSpawnSpacing = 4f;
>>>>>>> Stashed changes
    
    // Private variables
    private float lastSpawnX = 0f;
    private float nextSpawnX = 0f;
    private float currentSpawnSpacing;
    private float distanceTraveled = 0f;
    private float lastDifficultyIncreaseDistance = 0f;
    private Vector3 lastTrackedPosition;
    private List<GameObject> activeObjects = new List<GameObject>();
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
        bool hasDecorations = floorDecorations != null && floorDecorations.Length > 0;
        bool hasObstacles = floatingObstacles != null && floatingObstacles.Length > 0;
        
        if (!hasDecorations && !hasObstacles)
        {
            Debug.LogError("ObstacleSpawner: No decorations or obstacles assigned!");
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
<<<<<<< Updated upstream
=======
        Debug.Log($"Floor Decorations: {floorDecorations?.Length ?? 0}, Floating Obstacles: {floatingObstacles?.Length ?? 0}");
>>>>>>> Stashed changes
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
            SpawnObject();
            CalculateNextSpawnX();
        }
        
        // Update difficulty
        if (increaseDifficulty)
        {
            UpdateDifficulty();
        }
        
        // Clean up objects that are far behind
        CleanupObjects(currentPos.x);
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
<<<<<<< Updated upstream
    /// Spawns an obstacle at nextSpawnX
    /// </summary>
    void SpawnObstacle()
=======
    /// Spawns either a floor decoration or floating obstacle
    /// </summary>
    void SpawnObject()
    {
        bool hasDecorations = floorDecorations != null && floorDecorations.Length > 0;
        bool hasObstacles = floatingObstacles != null && floatingObstacles.Length > 0;
        
        // Decide: decoration or obstacle?
        bool spawnDecoration = hasDecorations && (!hasObstacles || Random.value < decorationChance);
        
        if (spawnDecoration)
        {
            SpawnFloorDecoration(nextSpawnX);
        }
        else if (hasObstacles)
        {
            SpawnFloatingObstacle(nextSpawnX);
        }
    }
    
    /// <summary>
    /// Spawns a floor decoration (coral, rock, seaweed)
    /// </summary>
    void SpawnFloorDecoration(float xPos)
    {
        // Choose random decoration
        GameObject decorationPrefab = floorDecorations[Random.Range(0, floorDecorations.Length)];
        
        // Position on floor with slight variation
        float yPos = floorY + Random.Range(-floorYVariation, floorYVariation);
        Vector3 spawnPos = new Vector3(xPos, yPos, 0f);
        
        // Create decoration
        GameObject decoration = Instantiate(decorationPrefab, spawnPos, Quaternion.identity);
        decoration.transform.parent = transform;
        
        // Add to active list
        activeObjects.Add(decoration);
        
        Debug.Log($"Spawned floor decoration '{decorationPrefab.name}' at X={xPos:F1}, Y={yPos:F1}");
    }
    
    /// <summary>
    /// Spawns a floating obstacle (sea urchin, jellyfish)
    /// </summary>
    void SpawnFloatingObstacle(float xPos)
>>>>>>> Stashed changes
    {
        // Decide if spawning pattern or single obstacle
        bool spawnPattern = useSpawnPatterns && Random.value < patternChance;
        
        if (spawnPattern)
        {
<<<<<<< Updated upstream
            SpawnPattern(nextSpawnX);
        }
        else
        {
            SpawnSingleObstacle(nextSpawnX);
=======
            SpawnPattern(xPos);
        }
        else
        {
            SpawnSingleObstacle(xPos);
>>>>>>> Stashed changes
        }
    }
    
    /// <summary>
<<<<<<< Updated upstream
    /// Spawns a single random obstacle at specific X position
=======
    /// Spawns a single floating obstacle at specific X position
>>>>>>> Stashed changes
    /// </summary>
    void SpawnSingleObstacle(float xPos)
    {
        // Choose random obstacle prefab
<<<<<<< Updated upstream
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        
        // Random Y position
        float yPos = Random.Range(minY, maxY);
=======
        GameObject obstaclePrefab = floatingObstacles[Random.Range(0, floatingObstacles.Length)];
        
        // Safe Y position (avoids center corridor)
        float yPos = GetSafeYPosition();
>>>>>>> Stashed changes
        
        // Spawn position
        Vector3 spawnPos = new Vector3(xPos, yPos, 0f);
        
        // Create obstacle
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
<<<<<<< Updated upstream
        
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
        // Choose random pattern type
        int patternType = Random.Range(0, 4);
=======
        obstacle.transform.parent = transform;
        
        // Make sure it has the Obstacle tag for collisions
        if (!obstacle.CompareTag("Obstacle"))
        {
            obstacle.tag = "Obstacle";
        }
        
        // Add to active list
        activeObjects.Add(obstacle);
        
        Debug.Log($"Spawned floating obstacle '{obstaclePrefab.name}' at X={xPos:F1}, Y={yPos:F1}");
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
    /// Spawns a pattern of floating obstacles at specific X position
    /// </summary>
    void SpawnPattern(float xPos)
    {
        // Choose random pattern type (only 2 simple patterns)
        int patternType = Random.Range(0, 2);
>>>>>>> Stashed changes
        
        switch (patternType)
        {
            case 0:
                SpawnVerticalLine(xPos);
                break;
            case 1:
                SpawnHorizontalSpread(xPos);
                break;
<<<<<<< Updated upstream
            case 2:
                SpawnDiagonalLine(xPos);
                break;
            case 3:
                SpawnCluster(xPos);
                break;
=======
>>>>>>> Stashed changes
        }
    }
    
    /// <summary>
<<<<<<< Updated upstream
    /// Spawns obstacles in a vertical line with a gap
=======
    /// Spawns floating obstacles in a vertical line with a LARGE gap
>>>>>>> Stashed changes
    /// </summary>
    void SpawnVerticalLine(float xPos)
    {
        GameObject obstaclePrefab = floatingObstacles[Random.Range(0, floatingObstacles.Length)];
        
<<<<<<< Updated upstream
        // Create gap in middle
        float gapSize = 2.5f;
        float gapCenter = Random.Range(minY + 1.5f, maxY - 1.5f);
        
        // Spawn obstacles above and below gap
        for (float y = minY; y <= maxY; y += 1.5f)
=======
        // Create LARGE gap in middle
        float gapCenter = Random.Range(-1f, 1f);
        
        // Spawn obstacles above and below gap
        for (float y = minY; y <= maxY; y += 2f)
>>>>>>> Stashed changes
        {
            // Skip gap area
            if (Mathf.Abs(y - gapCenter) < gapSize / 2f)
                continue;
            
            Vector3 spawnPos = new Vector3(xPos, y, 0f);
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.parent = transform;
            
            if (!obstacle.CompareTag("Obstacle"))
            {
                obstacle.tag = "Obstacle";
            }
            
            activeObjects.Add(obstacle);
        }
        
<<<<<<< Updated upstream
        Debug.Log($"Spawned vertical line at X={xPos:F1}");
    }
    
    /// <summary>
    /// Spawns obstacles spread horizontally
    /// </summary>
    void SpawnHorizontalSpread(float xPos)
    {
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        float yPos = Random.Range(minY + 1f, maxY - 1f);
        
        // Spawn 3 obstacles at different X positions
        for (int i = 0; i < 3; i++)
        {
            float xOffset = i * 2f;
=======
        Debug.Log($"Spawned vertical line pattern at X={xPos:F1} with {verticalGapSize} gap");
    }
    
    /// <summary>
    /// Spawns floating obstacles spread horizontally with MORE space
    /// </summary>
    void SpawnHorizontalSpread(float xPos)
    {
        GameObject obstaclePrefab = floatingObstacles[Random.Range(0, floatingObstacles.Length)];
        float yPos = GetSafeYPosition();
        
        // Spawn only 2 obstacles with MORE spacing
        for (int i = 0; i < 2; i++)
        {
            float xOffset = i * horizontalSpacing;
>>>>>>> Stashed changes
            Vector3 spawnPos = new Vector3(xPos + xOffset, yPos, 0f);
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.parent = transform;
            
            if (!obstacle.CompareTag("Obstacle"))
            {
                obstacle.tag = "Obstacle";
            }
            
            activeObjects.Add(obstacle);
        }
        
<<<<<<< Updated upstream
        Debug.Log($"Spawned horizontal spread at X={xPos:F1}");
=======
        Debug.Log($"Spawned horizontal spread pattern at X={xPos:F1}");
>>>>>>> Stashed changes
    }
    
    /// <summary>
    /// Spawns obstacles in a diagonal line
    /// </summary>
    void SpawnDiagonalLine(float xPos)
    {
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        
        for (int i = 0; i < 4; i++)
        {
            float xOffset = i * 1.5f;
            float yPos = Mathf.Lerp(minY + 1f, maxY - 1f, i / 3f);
            
            Vector3 spawnPos = new Vector3(xPos + xOffset, yPos, 0f);
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.parent = transform;
            
            activeObstacles.Add(obstacle);
        }
        
        Debug.Log($"Spawned diagonal line at X={xPos:F1}");
    }
    
    /// <summary>
    /// Spawns a cluster of obstacles
    /// </summary>
    void SpawnCluster(float xPos)
    {
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        float centerY = Random.Range(minY + 1.5f, maxY - 1.5f);
        
        // Spawn 4-6 obstacles in a cluster
        int count = Random.Range(4, 7);
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 1.5f;
            Vector3 spawnPos = new Vector3(xPos + offset.x, centerY + offset.y, 0f);
            
            // Clamp Y to bounds
            spawnPos.y = Mathf.Clamp(spawnPos.y, minY + 0.5f, maxY - 0.5f);
            
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.parent = transform;
            
            activeObstacles.Add(obstacle);
        }
        
        Debug.Log($"Spawned cluster at X={xPos:F1}");
    }
    
    /// <summary>
    /// Calculates next spawn X position
    /// </summary>
    void CalculateNextSpawnX()
    {
        float variation = Random.Range(-spacingVariation, spacingVariation);
        nextSpawnX = lastSpawnX + currentSpawnSpacing + variation;
        lastSpawnX = nextSpawnX;
    }
    
    /// <summary>
    /// Increases difficulty based on distance traveled
    /// </summary>
    void UpdateDifficulty()
    {
        if (distanceTraveled - lastDifficultyIncreaseDistance >= difficultyIncreaseDistance)
        {
            lastDifficultyIncreaseDistance = distanceTraveled;
            
            // Decrease spacing (spawn more frequently)
            currentSpawnSpacing = Mathf.Max(currentSpawnSpacing - spacingDecreaseAmount, minSpawnSpacing);
            
            Debug.Log($"Difficulty increased at {distanceTraveled:F0} distance! New spacing: {currentSpawnSpacing:F1}");
        }
    }
    
    /// <summary>
    /// Removes objects that are far behind the player
    /// </summary>
    void CleanupObjects(float currentX)
    {
        float destroyThreshold = currentX - destroyDistanceBehind;
        
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeObjects[i];
            
            if (obj == null)
            {
                activeObjects.RemoveAt(i);
            }
            else if (obj.transform.position.x < destroyThreshold)
            {
                Destroy(obj);
                activeObjects.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Stop spawning
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
    }
    
    /// <summary>
    /// Start spawning
    /// </summary>
    public void StartSpawning()
    {
        isSpawning = true;
    }
    
    /// <summary>
    /// Clear all active objects
    /// </summary>
    public void ClearAllObjects()
    {
        foreach (GameObject obj in activeObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        activeObjects.Clear();
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
        
        // Draw spawn ahead line (green)
        Gizmos.color = Color.green;
        float spawnX = trackedPos.x + spawnDistanceAhead;
        Gizmos.DrawLine(new Vector3(spawnX, minY, 0), new Vector3(spawnX, maxY, 0));
        
        // Draw destroy behind line (red)
        Gizmos.color = Color.red;
        float destroyX = trackedPos.x - destroyDistanceBehind;
        Gizmos.DrawLine(new Vector3(destroyX, minY, 0), new Vector3(destroyX, maxY, 0));
        
<<<<<<< Updated upstream
        // Draw next spawn position
=======
        // Draw safe zone (center corridor) - light green
        if (useSafeZone)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Vector3 safeCenter = new Vector3(trackedPos.x, 0, 0);
            Vector3 safeSize = new Vector3(50, safeZoneRadius * 2, 0.1f);
            Gizmos.DrawCube(safeCenter, safeSize);
        }
        
        // Draw floor line (yellow)
>>>>>>> Stashed changes
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(trackedPos.x - 20, floorY, 0),
            new Vector3(trackedPos.x + 20, floorY, 0)
        );
        
        // Draw next spawn position (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(nextSpawnX, (minY + maxY) / 2f, 0), 0.5f);
    }
}