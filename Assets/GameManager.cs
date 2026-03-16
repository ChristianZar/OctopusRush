using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Game Manager that works with your existing PlayerHealth, PlayerController, and ScoreManager
/// Shows game over screen instead of auto-reloading scene
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverStatsText;
    
    [Header("Continue System")]
    public GameObject continueButton;
    private int continuesLeft = 1;
    private bool hasUsedContinue = false;
    
    [Header("Player Reference")]
    public GameObject player;
    public PlayerHealth playerHealth;
    public PlayerController playerController;
    
    [Header("Score Manager Reference")]
    public ScoreManager scoreManager;
    
    [Header("Respawn Settings")]
    public Vector3 respawnPosition = Vector3.zero;
    public bool resetHealthOnRespawn = true;
    
    private bool isGameOver = false;
    private float gameStartTime;
    
    void Start()
    {
        // Auto-find references if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        
        if (playerController == null && player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
        
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }
        
        // Make sure game over panel is hidden at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Reset continue system
        continuesLeft = 1;
        hasUsedContinue = false;
        isGameOver = false;
        
        // Record game start time
        gameStartTime = Time.time;
        
        Time.timeScale = 1f;
    }
    
    void Update()
    {
        // Check if player died (from PlayerHealth)
        if (playerHealth != null && playerHealth.IsDead() && !isGameOver)
        {
            // Player just died, show game over instead of auto-reloading
            PlayerDied();
        }
    }
    
    /// <summary>
    /// Called when the player dies
    /// This replaces the auto-reload in PlayerHealth
    /// </summary>
    public void PlayerDied()
    {
        if (isGameOver) return; // Already handling death
        
        
        isGameOver = true;
        
        // Save the last run score before showing game over
        if (scoreManager != null)
        {
            scoreManager.SaveLastRun();
        }
        
        ShowGameOver();
    }
    
    /// <summary>
    /// Shows the game over screen with stats
    /// </summary>
    void ShowGameOver()
    {
        // Small delay to let death animation play
        Invoke("DisplayGameOverPanel", 1.5f);
    }
    
    /// <summary>
    /// Actually displays the game over panel after delay
    /// </summary>
    void DisplayGameOverPanel()
    {
        // Pause the game
        Time.timeScale = 0f;
        
        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Update stats display
        UpdateStatsDisplay();
        
        // Check if continue is available
        if (hasUsedContinue || continuesLeft <= 0)
        {
            // Hide continue button if already used
            if (continueButton != null)
            {
                continueButton.SetActive(false);
            }
        }
        
    }
    
    /// <summary>
    /// Updates the game over stats text
    /// </summary>
    void UpdateStatsDisplay()
    {
        if (gameOverStatsText == null) return;
        
        // Calculate time survived
        float timeSurvived = Time.time - gameStartTime;
        int minutes = Mathf.FloorToInt(timeSurvived / 60f);
        int seconds = Mathf.FloorToInt(timeSurvived % 60f);
        
        // Get scores from ScoreManager if available
        float currentMeters = 0f;
        float lastMeters = 0f;
        float bestMeters = 0f;
        
        if (scoreManager != null && scoreManager.scoreTarget != null)
        {
            // Calculate current run meters
            currentMeters = Mathf.Max(0f, scoreManager.scoreTarget.position.x);
            
            // Get saved scores
            lastMeters = PlayerPrefs.GetFloat("LAST_SCORE", 0f);
            bestMeters = PlayerPrefs.GetFloat("BEST_SCORE", 0f);
        }
        
        // Build stats text
        string statsText = "";
        
        // Add big "YOU DIED" message
    
        statsText += "<size=80>YOUR RUN</size>\n";
        statsText += $"<size=56><b>Distance:</b> {Mathf.FloorToInt(currentMeters)}m </size>\n";
        statsText += $"<size=56><b>Time:</b> {minutes}:{seconds:00} </size>\n\n";
        
        // Check if new best score
        if (currentMeters > bestMeters && bestMeters > 0)
        {
            statsText += "<size=40><color=#FFD700>★ NEW BEST! ★</color></size>\n\n";
        }
        
        statsText += $"<size=28><color=#AAAAAA>Last: {Mathf.FloorToInt(lastMeters)}m</color>\n";
        statsText += $"<color=#FFFF88>Best: {Mathf.FloorToInt(bestMeters)}m</color></size>";
        
        gameOverStatsText.text = statsText;
    }
    
    /// <summary>
    /// Continue button - gives player one more chance
    /// </summary>
    public void OnContinueClicked()
    {
        if (continuesLeft > 0 && !hasUsedContinue)
        {
            hasUsedContinue = true;
            continuesLeft--;
            RespawnPlayer();
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
            isGameOver = false;
        }
    }
    
    /// <summary>
    /// Respawn the player with full health
    /// </summary>
    void RespawnPlayer()
    {
        if (player == null) return;
        player.transform.position = respawnPosition;
        if (playerHealth != null && resetHealthOnRespawn)
            playerHealth.Respawn();
        player.SetActive(true);
    }
    
    /// <summary>
    /// Restart button - reload current scene from beginning
    /// </summary>
    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Check if game is currently over
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    /// <summary>
    /// Get continues remaining
    /// </summary>
    public int GetContinuesLeft()
    {
        return hasUsedContinue ? 0 : continuesLeft;
    }
}