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
    public float         labelHeight   = 70f;
    public float         labelFontSize = 32f;

    static readonly string[] CardLabels =
    {
        "Swim Up — Hold W",
        "Ink Blast — Press SPACE",
        "Collect Keys — Press E at chest",
        "Eat Fish to Heal",
        "Shield Orb",
        "Gun Power-Up — Press F",
        "Watch Out!",
        "Pause — ESC or P",
    };

    // ── Carousel state ────────────────────────────────────────────────────────

    int                _cardIndex;
    Image              _carouselImage;
    TextMeshProUGUI    _carouselLabel;
    TextMeshProUGUI    _pageIndicator;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        RepositionLogo();
        RepositionButtons();
        AddButtonFeedback();
        BuildHelpPanel();
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

        // ── Title ──
        MakeTMP(root.transform, "HelpTitle",
            anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(1f, 1f),
            pivot: new Vector2(0.5f, 1f), pos: Vector2.zero, size: new Vector2(0f, 70f),
            text: "HOW TO PLAY", fontSize: 54f, bold: true, color: Color.white);

        // ── Card image (fills centre, above label + indicator) ──
        float bottomStrip = labelHeight + 44f; // label + page indicator
        var imgGO = new GameObject("CarouselImage", typeof(RectTransform), typeof(Image));
        imgGO.transform.SetParent(root.transform, false);
        var imgRT = imgGO.GetComponent<RectTransform>();
        imgRT.anchorMin = Vector2.zero;
        imgRT.anchorMax = Vector2.one;
        imgRT.offsetMin = new Vector2(100f, bottomStrip);  // 100px gutters for nav buttons
        imgRT.offsetMax = new Vector2(-100f, -78f);         // 78px below title
        _carouselImage = imgGO.GetComponent<Image>();
        _carouselImage.preserveAspect = true;

        // ── Label ──
        _carouselLabel = MakeTMP(root.transform, "CarouselLabel",
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(1f, 0f),
            pivot: new Vector2(0.5f, 0f), pos: new Vector2(0f, 44f), size: new Vector2(0f, labelHeight),
            text: "", fontSize: labelFontSize, bold: true, color: Color.white);

        // ── Page indicator (e.g. "1 / 8") ──
        _pageIndicator = MakeTMP(root.transform, "PageIndicator",
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(1f, 0f),
            pivot: new Vector2(0.5f, 0f), pos: Vector2.zero, size: new Vector2(0f, 40f),
            text: "", fontSize: 26f, bold: false, color: new Color(0.7f, 0.85f, 1f));

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

        if (_carouselLabel != null)
            _carouselLabel.text = _cardIndex < CardLabels.Length ? CardLabels[_cardIndex] : "";

        if (_pageIndicator != null)
            _pageIndicator.text = $"{_cardIndex + 1}  /  {total}";
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
