using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UPDATED ObstacleSpawner for scrolling/moving camera gameplay
/// Spawns obstacles ahead of the player as they move forward
/// Perfect for games where player keeps moving!
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [Tooltip("Array of different obstacle prefabs to spawn")]
    public GameObject[] obstaclePrefabs;
    
    [Header("Spawn Settings")]
    [Tooltip("Distance ahead of player/camera to spawn")]
    public float spawnDistanceAhead = 15f;
    
    [Tooltip("Distance between obstacle spawns")]
    public float spawnSpacing = 5f;
    
    [Tooltip("Random variation in spawn spacing (+/- units)")]
    public float spacingVariation = 2f;
    
    [Header("Spawn Area")]
    [Tooltip("Minimum Y position for spawning")]
    public float minY = -4f;
    
    [Tooltip("Maximum Y position for spawning")]
    public float maxY = 4f;
    
    [Tooltip("Distance behind player/camera to destroy obstacles")]
    public float destroyDistanceBehind = 15f;
    
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
    
    [Tooltip("Chance of spawning a pattern instead of single obstacle (0-1)")]
    [Range(0f, 1f)]
    public float patternChance = 0.3f;
    
    [Header("Difficulty Settings")]
    [Tooltip("Increase difficulty over distance")]
    public bool increaseDifficulty = true;
    
    [Tooltip("Distance traveled before difficulty increases")]
    public float difficultyIncreaseDistance = 50f;
    
    [Tooltip("How much to decrease spawn spacing each difficulty increase")]
    public float spacingDecreaseAmount = 0.5f;
    
    [Tooltip("Minimum spawn spacing (won't go below this)")]
    public float minSpawnSpacing = 2f;
    
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
        
        // Random Y position
        float yPos = Random.Range(minY, maxY);
        
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
        // Choose random pattern type
        int patternType = Random.Range(0, 4);
        
        switch (patternType)
        {
            case 0:
                SpawnVerticalLine(xPos);
                break;
            case 1:
                SpawnHorizontalSpread(xPos);
                break;
            case 2:
                SpawnDiagonalLine(xPos);
                break;
            case 3:
                SpawnCluster(xPos);
                break;
        }
    }
    
    /// <summary>
    /// Spawns obstacles in a vertical line with a gap
    /// </summary>
    void SpawnVerticalLine(float xPos)
    {
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        
        // Create gap in middle
        float gapSize = 2.5f;
        float gapCenter = Random.Range(minY + 1.5f, maxY - 1.5f);
        
        // Spawn obstacles above and below gap
        for (float y = minY; y <= maxY; y += 1.5f)
        {
            // Skip gap area
            if (Mathf.Abs(y - gapCenter) < gapSize / 2f)
                continue;
            
            Vector3 spawnPos = new Vector3(xPos, y, 0f);
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.parent = transform;
            
            activeObstacles.Add(obstacle);
        }
        
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
            Vector3 spawnPos = new Vector3(xPos + xOffset, yPos, 0f);
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.transform.parent = transform;
            
            activeObstacles.Add(obstacle);
        }
        
        Debug.Log($"Spawned horizontal spread at X={xPos:F1}");
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
        
        // Draw next spawn position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(nextSpawnX, (minY + maxY) / 2f, 0), 0.5f);
    }
}