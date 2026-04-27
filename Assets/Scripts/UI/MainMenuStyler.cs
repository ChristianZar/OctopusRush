using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Organizes main menu layout at runtime:
///   - Repositions logo and button grid
///   - Builds How To Play panel as a carousel (one card at a time, Prev/Next)
public class MainMenuStyler : MonoBehaviour
{
    [Header("How To Play Cards")]
    [Tooltip("Drag the 8 card sprites here in order:\n" +
             "Swimup, Ink, CollectKeys, EatFish,\n" +
             "ShieldOrb, Gun, Watchout, Stop")]
    public Sprite[]      helpCardSprites;
    public TMP_FontAsset helpFont;

    // ── Carousel state ────────────────────────────────────────────────────────

    int   _cardIndex;
    Image _carouselImage;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        RepositionLogo();
        RepositionButtons();
        AddButtonFeedback();
        BuildHelpPanel();
        BuildCreditsPanel();
    }

    // ── Logo ──────────────────────────────────────────────────────────────────

    void RepositionLogo()
    {
        var go = GameObject.Find("Logo");
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;

        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, 555f);
        rt.sizeDelta        = new Vector2(900f, 400f);
    }

    // ── Button grid ───────────────────────────────────────────────────────────

    void RepositionButtons()
    {
        float colGap = 24f;
        float rowGap = 20f;
        float rowH   = 130f;

        float wStart = 460f; float wHelp  = 450f;
        float wAch   = 400f; float wSkins = 400f;
        float wCred  = 440f; float wQuit  = 350f;

        float colOffset = wStart / 2f + colGap / 2f;
        float totalH    = rowH * 3 + rowGap * 2;
        float topY      = totalH / 2f - rowH / 2f;

        float[] rowY =
        {
            topY,
            topY - (rowH + rowGap),
            topY - (rowH + rowGap) * 2f,
        };

        Apply("StartButton",       new Vector2(-colOffset, rowY[0]), new Vector2(wStart, rowH));
        Apply("HowToPlayButton",   new Vector2( colOffset, rowY[0]), new Vector2(wHelp,  rowH));
        Apply("AchievementButton", new Vector2(-colOffset, rowY[1]), new Vector2(wAch,   rowH));
        Apply("SkinsButton",       new Vector2( colOffset, rowY[1]), new Vector2(wSkins, rowH));
        Apply("CreditsButton",     new Vector2(-colOffset, rowY[2]), new Vector2(wCred,  rowH));
        Apply("QuitButton",        new Vector2( colOffset, rowY[2]), new Vector2(wQuit,  rowH));
    }

    void Apply(string goName, Vector2 pos, Vector2 size)
    {
        var go = GameObject.Find(goName);
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }

    // ── Button hover feedback ─────────────────────────────────────────────────

    static readonly string[] ButtonNames =
    {
        "StartButton", "HowToPlayButton", "AchievementButton",
        "SkinsButton", "CreditsButton", "QuitButton"
    };

    void AddButtonFeedback()
    {
        foreach (var name in ButtonNames)
        {
            var go = GameObject.Find(name);
            if (go == null) continue;
            if (go.GetComponent<MenuButtonFeedback>() == null)
                go.AddComponent<MenuButtonFeedback>();
        }
    }

    // ── Help panel — carousel ────────────────────────────────────────────────

    void BuildHelpPanel()
    {
        var mgr = FindObjectOfType<MainMenuManager>();
        if (mgr == null || mgr.helpPanel == null) return;
        var panel = mgr.helpPanel;

        if (panel.transform.Find("HelpContent") != null) return;

        // Remove old non-button children
        var toDelete = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in panel.transform)
        {
            if (child.GetComponent<Button>() == null)
                toDelete.Add(child.gameObject);
        }
        foreach (var go in toDelete) Destroy(go);

        // ── Root ──
        var root = new GameObject("HelpContent", typeof(RectTransform));
        root.transform.SetParent(panel.transform, false);
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = new Vector2(16f, 130f);  // extra bottom room for back button
        rootRT.offsetMax = new Vector2(-16f, -16f);

        // ── Card image (fills the full panel, nav buttons overlay on sides) ──
        var imgGO = new GameObject("CarouselImage", typeof(RectTransform), typeof(Image));
        imgGO.transform.SetParent(root.transform, false);
        var imgRT = imgGO.GetComponent<RectTransform>();
        imgRT.anchorMin = Vector2.zero;
        imgRT.anchorMax = Vector2.one;
        imgRT.offsetMin = new Vector2(100f, 0f);
        imgRT.offsetMax = new Vector2(-100f, 0f);
        _carouselImage = imgGO.GetComponent<Image>();
        _carouselImage.preserveAspect = true;

        // ── Prev button (left) ──
        var prevBtn = MakeNavButton(root.transform, "PrevButton", left: true);
        prevBtn.onClick.AddListener(PrevCard);

        // ── Next button (right) ──
        var nextBtn = MakeNavButton(root.transform, "NextButton", left: false);
        nextBtn.onClick.AddListener(NextCard);

        // Show first card
        ShowCard(0);

        if (panel.GetComponent<Image>() is Image bg) bg.raycastTarget = true;
    }

    // ── Carousel logic ────────────────────────────────────────────────────────

    void ShowCard(int index)
    {
        int total = helpCardSprites != null ? helpCardSprites.Length : 0;
        if (total == 0) return;

        _cardIndex = (index + total) % total;

        if (_carouselImage != null)
            _carouselImage.sprite = helpCardSprites[_cardIndex];
    }

    void NextCard() => ShowCard(_cardIndex + 1);
    void PrevCard() => ShowCard(_cardIndex - 1);

    // ── Helpers ───────────────────────────────────────────────────────────────

    TextMeshProUGUI MakeTMP(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size,
        string text, float fontSize, bool bold, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot     = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text           = text;
        tmp.fontSize       = fontSize;
        tmp.fontStyle      = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.color          = color;
        tmp.alignment      = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget  = false;
        if (helpFont != null) tmp.font = helpFont;
        return tmp;
    }

    // ── Credits panel ─────────────────────────────────────────────────────────

    void BuildCreditsPanel()
    {
        var mgr = FindObjectOfType<MainMenuManager>();
        if (mgr == null || mgr.creditsPanel == null) return;

        var tmps = mgr.creditsPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmps)
        {
            if (tmp.gameObject.name == "Text (TMP)" || tmp.text.Contains("[YOUR NAME]") || tmp.text.Contains("Octopus Prime") || tmp.text.Contains("OCTOPUS RUSH"))
            {
                tmp.text =
                    "<size=42><b>OCTOPUS RUSH</b></size>\n\n" +
                    "A game about survival,\ntreasure, and tentacles!\n\n" +
                    "<color=#FFB347>\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501</color>\n\n" +
                    "<b>Developed by</b>\n" +
                    "<color=#FFB347><size=34>Octopus Prime</size></color>\n\n" +
                    "Eyob Kabeto\n" +
                    "Christian Zarate\n" +
                    "Duc Le\n\n" +
                    "<color=#FFB347>\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501</color>\n\n" +
                    "Made with Unity\n\u00A9 2025\n\n" +
                    "<i>Thank you for playing!</i>";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = true;
                break;
            }
        }
    }

    Button MakeNavButton(Transform parent, string name, bool left)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();

        // Pin to left or right edge, vertically centred in the card area
        rt.anchorMin = left ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        rt.anchorMax = left ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        rt.pivot     = left ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(90f, 160f);

        go.GetComponent<Image>().color = new Color(0.1f, 0.2f, 0.4f, 0.75f);

        // Arrow label
        var lblGO = new GameObject("Arrow", typeof(RectTransform), typeof(CanvasRenderer));
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.GetComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text      = left ? "<" : ">";
        lbl.fontSize  = 52f;
        lbl.color     = Color.white;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.raycastTarget = false;
        if (helpFont != null) lbl.font = helpFont;

        // Hover feedback
        if (go.GetComponent<MenuButtonFeedback>() == null)
            go.AddComponent<MenuButtonFeedback>();

        return go.GetComponent<Button>();
    }
}
