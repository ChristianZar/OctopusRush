using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [Header("Pause Button (optional – auto-created if left empty)")]
    public GameObject pauseButton;

    [Header("Achievements")]
    public AchievementPanel achievementPanel;

    private bool isPaused = false;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButton == null)
            pauseButton = CreatePauseButton();
    }

    // ── Canvas scaler enforcement ──────────────────────────────────────────────
    static void EnsureCanvasScaler(Canvas canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
    }

    // ── Auto-build a pause button in the top-right corner ─────────────────────
    GameObject CreatePauseButton()
    {
        // Find (or create) a screen-space Canvas to host the button
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("PauseButtonCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
        }
        EnsureCanvasScaler(canvas);

        // Button GameObject
        var btnGO = new GameObject("PauseBtn_HUD",
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasRenderer));
        btnGO.transform.SetParent(canvas.transform, false);

        // Top-right corner, 160×160 px, 20 px inset
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(160f, 160f);
        rt.anchoredPosition = new Vector2(-20f, -20f);

        // Semi-transparent dark circle background
        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.55f);
        img.raycastTarget = true;

        // ⏸ label
        var labelGO = new GameObject("PauseIcon", typeof(RectTransform), typeof(CanvasRenderer));
        labelGO.transform.SetParent(btnGO.transform, false);
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = labelRT.offsetMax = Vector2.zero;
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "⏸";
        tmp.fontSize  = 76f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.raycastTarget = false;

        // Wire up click
        btnGO.GetComponent<Button>().onClick.AddListener(TogglePause);

        return btnGO;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            // If achievement panel is visible, close it instead of toggling pause
            if (achievementPanel != null && achievementPanel.gameObject.activeSelf)
                achievementPanel.Hide();
            else
                TogglePause();
        }
    }

    public void TogglePause()
    {
        if (gameManager != null && gameManager.IsGameOver()) return;
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
    }

    public void OnResumeClicked()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnAchievementsClicked()
    {
        if (achievementPanel != null)
            achievementPanel.Toggle();
    }

    public bool IsPaused => isPaused;
}
