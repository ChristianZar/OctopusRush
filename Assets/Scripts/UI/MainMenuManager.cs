using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject startMenuPanel;
    public GameObject helpPanel;
    public GameObject creditsPanel;
    public string gameSceneName = "MainScene";

    [Header("Logo — hides when sub-panels open")]
    public GameObject logo;

    [Header("Achievements")]
    public AchievementPanel achievementPanel;

    [Header("Skin Shop")]
    public SkinShopPanel skinShopPanel;

    [Header("Transition")]
    public float transitionDuration = 0.25f;

    private bool _isTransitioning;
    private Coroutine _activeTransition;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    void Start()
    {
        // Ensure all panels start in a clean state (no leftover CanvasGroup alpha)
        SnapVisible(startMenuPanel);
        SnapHidden(helpPanel);
        SnapHidden(creditsPanel);
        SetLogo(true);
    }

    // ── Public navigation ──────────────────────────────────────────────────────

    public void ShowStartMenu()
    {
        SwitchTo(startMenuPanel, new[] { helpPanel, creditsPanel }, logoVisible: true);
    }

    public void ShowHelp()
    {
        SwitchTo(helpPanel, new[] { startMenuPanel, creditsPanel }, logoVisible: false);
    }

    public void ShowCredits()
    {
        SwitchTo(creditsPanel, new[] { startMenuPanel, helpPanel }, logoVisible: false);
    }

    public void ShowAchievements()
    {
        if (achievementPanel == null) return;
        achievementPanel.onClose = () => SwitchTo(startMenuPanel, new GameObject[] { }, logoVisible: true);
        achievementPanel.Show();
        SwitchTo(null, new[] { startMenuPanel }, logoVisible: false);
    }

    public void ShowSkinShop()
    {
        if (skinShopPanel == null) return;
        skinShopPanel.onClose = () => SwitchTo(startMenuPanel, new GameObject[] { }, logoVisible: true);
        skinShopPanel.Show();
        SwitchTo(null, new[] { startMenuPanel }, logoVisible: false);
    }

    public void StartGame()
    {
        if (_isTransitioning) return;
        StartCoroutine(FadeAndLoad());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ── Transition core ────────────────────────────────────────────────────────

    // incoming may be null (e.g. when opening Achievement/SkinShop overlays)
    void SwitchTo(GameObject incoming, GameObject[] outgoing, bool logoVisible)
    {
        if (_activeTransition != null)
        {
            StopCoroutine(_activeTransition);
            // Snap everything to a clean state before starting the new transition
            foreach (var p in outgoing)
                SnapHidden(p);
            if (incoming != null)
                SnapVisible(incoming);
        }
        SetLogo(logoVisible);
        _activeTransition = StartCoroutine(TransitionCoroutine(incoming, outgoing));
    }

    IEnumerator TransitionCoroutine(GameObject incoming, GameObject[] outgoing)
    {
        _isTransitioning = true;
        float half = transitionDuration * 0.5f;

        // — Fade OUT outgoing panels —
        // Only fade panels that are currently visible; skip hidden ones entirely.
        // Block raycasts immediately so buttons can't be double-clicked during fade.
        foreach (var p in outgoing)
        {
            if (p == null || !p.activeSelf) continue;
            var cg = GetOrAddCG(p);
            cg.blocksRaycasts = false;
            cg.interactable   = false;
        }

        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / half);
            foreach (var p in outgoing)
            {
                if (p == null || !p.activeSelf) continue;
                GetOrAddCG(p).alpha = alpha;
            }
            yield return null;
        }

        foreach (var p in outgoing)
        {
            if (p == null) continue;
            SnapHidden(p);
        }

        // — Fade IN incoming panel —
        if (incoming != null)
        {
            incoming.SetActive(true);
            var cg = GetOrAddCG(incoming);
            cg.alpha          = 0f;
            cg.blocksRaycasts = false;
            cg.interactable   = false;

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, t / half);
                yield return null;
            }

            SnapVisible(incoming);
        }

        _isTransitioning  = false;
        _activeTransition = null;
    }

    IEnumerator FadeAndLoad()
    {
        _isTransitioning = true;

        // Fade out everything visible
        var allPanels = new[] { startMenuPanel, helpPanel, creditsPanel, logo };
        foreach (var p in allPanels)
        {
            if (p == null) continue;
            var cg = GetOrAddCG(p);
            cg.blocksRaycasts = false;
            cg.interactable   = false;
        }

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / transitionDuration);
            foreach (var p in allPanels)
            {
                if (p != null) GetOrAddCG(p).alpha = alpha;
            }
            yield return null;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    void SetLogo(bool visible)
    {
        if (logo == null) logo = GameObject.Find("Logo");
        if (logo == null) return;
        // Fade the logo in/out via its CanvasGroup alpha so FloatUI keeps working
        var cg = GetOrAddCG(logo);
        cg.alpha          = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
        cg.interactable   = visible;
        logo.SetActive(true); // always keep active so FloatUI coroutine isn't killed
    }

    void SnapVisible(GameObject go)
    {
        if (go == null) return;
        go.SetActive(true);
        var cg = GetOrAddCG(go);
        cg.alpha          = 1f;
        cg.blocksRaycasts = true;
        cg.interactable   = true;
    }

    void SnapHidden(GameObject go)
    {
        if (go == null) return;
        var cg = GetOrAddCG(go);
        cg.alpha          = 0f;
        cg.blocksRaycasts = false;
        cg.interactable   = false;
        go.SetActive(false);
    }

    static CanvasGroup GetOrAddCG(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}
