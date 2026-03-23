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
    private float deathTime;
    
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
        if (isGameOver) return;

        isGameOver = true;
        deathTime = Time.time;

        if (scoreManager != null)
            scoreManager.SaveLastRun();

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
    
    void UpdateStatsDisplay()
    {
        if (gameOverStatsText == null) return;

        float timeSurvived = Mathf.Max(0f, deathTime - gameStartTime);
        int minutes = Mathf.FloorToInt(timeSurvived / 60f);
        int seconds = Mathf.FloorToInt(timeSurvived % 60f);

        float currentMeters = scoreManager != null ? scoreManager.GetMeters() : 0f;
        float lastMeters = PlayerPrefs.GetFloat("LAST_SCORE", 0f);
        float bestMeters = PlayerPrefs.GetFloat("BEST_SCORE", 0f);

        string statsText = $"<b>Distance</b>   {Mathf.FloorToInt(currentMeters)} m\n" +
                           $"<b>Time</b>   {minutes}:{seconds:00}\n\n";

        if (currentMeters > bestMeters && currentMeters > 0)
            statsText += "<color=#FFD700>★  NEW BEST!  ★</color>\n\n";

        statsText += $"<color=#AAAAAA>Last run   {Mathf.FloorToInt(lastMeters)} m\n" +
                     $"Best   {Mathf.FloorToInt(bestMeters)} m</color>";

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