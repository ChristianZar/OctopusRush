using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    [Header("Panel Animation")]
    public float fadeInDuration = 0.35f;

    private bool isGameOver = false;
    private float gameStartTime;
    private float deathTime;
    private CanvasGroup panelCanvasGroup;
    private bool isNewBest;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (playerController == null && player != null)
            playerController = player.GetComponent<PlayerController>();

        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        if (gameOverPanel != null)
        {
            panelCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
                panelCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            panelCanvasGroup.alpha = 0f;
            gameOverPanel.SetActive(false);
        }

        continuesLeft = 1;
        hasUsedContinue = false;
        isGameOver = false;
        gameStartTime = Time.time;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.IsDead() && !isGameOver)
            PlayerDied();
    }

    public void PlayerDied()
    {
        if (isGameOver) return;

        isGameOver = true;
        deathTime = Time.time;

        if (scoreManager != null)
            scoreManager.SaveLastRun();

        Invoke("DisplayGameOverPanel", 1.5f);
    }

    void DisplayGameOverPanel()
    {
        Time.timeScale = 0f;

        UpdateStatsDisplay();

        if (hasUsedContinue || continuesLeft <= 0)
            if (continueButton != null)
                continueButton.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            StartCoroutine(FadeInPanel());
            if (isNewBest)
                StartCoroutine(PunchNewBest());
        }
    }

    IEnumerator FadeInPanel()
    {
        if (panelCanvasGroup == null) yield break;

        float elapsed = 0f;
        panelCanvasGroup.alpha = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            panelCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
    }

    IEnumerator PunchNewBest()
    {
        if (gameOverStatsText == null) yield break;

        // Wait for panel to mostly fade in first
        yield return new WaitForSecondsRealtime(fadeInDuration * 0.8f);

        Transform t = gameOverStatsText.transform;
        float elapsed = 0f;
        float duration = 0.45f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float s = 1f + Mathf.Sin(elapsed / duration * Mathf.PI) * 0.14f;
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        t.localScale = Vector3.one;
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

        isNewBest = currentMeters > bestMeters && currentMeters > 0;

        string statsText =
            "<b>DISTANCE</b>          <b>TIME</b>\n" +
            $"  {Mathf.FloorToInt(currentMeters)} m               {minutes}:{seconds:00}\n\n";

        if (isNewBest)
            statsText += "<color=#FFD700><b>★  NEW BEST!  ★</b></color>\n\n";

        statsText +=
            $"<color=#AAAAAA>Last run    {Mathf.FloorToInt(lastMeters)} m\n" +
            $"Best        {Mathf.FloorToInt(bestMeters)} m</color>";

        gameOverStatsText.text = statsText;
    }

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

    void RespawnPlayer()
    {
        if (player == null) return;
        player.transform.position = respawnPosition;
        if (playerHealth != null && resetHealthOnRespawn)
            playerHealth.Respawn();
        player.SetActive(true);
    }

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

    public bool IsGameOver() => isGameOver;
    public int GetContinuesLeft() => hasUsedContinue ? 0 : continuesLeft;
}
