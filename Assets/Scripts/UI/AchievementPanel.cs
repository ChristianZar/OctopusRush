using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// Two-column scrollable achievement grid.
/// Keep the AchievementPanel GameObject INACTIVE in the scene (default).
/// Show/Hide use SetActive, but deactivation is deferred one frame so the
/// EventSystem finishes processing the click before the GO disappears.
[RequireComponent(typeof(RectTransform))]
public class AchievementPanel : MonoBehaviour
{
    [Header("Font (optional — drag Showpop SDF here)")]
    public TMP_FontAsset cardFont;

    [Header("Icons (optional)")]
    [Tooltip("One sprite per achievement, same order as AchievementManager.All (22 total).")]
    public Sprite[] achievementIcons;
    [Tooltip("Shown for locked entries when achievementIcons is assigned.")]
    public Sprite lockedIcon;
    [Tooltip("Star/badge shown on unlocked cards. Leave empty to hide the badge.")]
    public Sprite unlockedBadgeSprite;

    [Header("Layout")]
    public float cardHeight  = 620f;
    public float cardSpacing =   8f;
    public float sidePad     =  24f;
    public float topPad      =  76f;

    [Header("Colours")]
    public Color panelBg        = new Color(0.04f, 0.06f, 0.13f, 0.98f);
    public Color cardLocked     = new Color(0.08f, 0.09f, 0.15f, 1f);
    public Color cardUnlocked   = new Color(0.06f, 0.18f, 0.09f, 1f);
    public Color accentUnlocked = new Color(0.28f, 0.85f, 0.45f, 1f);
    public Color accentLocked   = new Color(0.16f, 0.18f, 0.27f, 1f);
    public Color titleLocked    = new Color(0.42f, 0.44f, 0.56f, 1f);
    public Color descUnlocked   = new Color(0.80f, 0.88f, 1.00f, 1f);
    public Color descLocked     = new Color(0.27f, 0.29f, 0.38f, 1f);

    // ── Icon definitions ──────────────────────────────────────────────────────
    private static readonly Dictionary<string,(string lbl, Color col)> Icons =
        new Dictionary<string,(string,Color)>
    {
        {"first_stroke",    ("100m", new Color(0.15f,0.42f,0.80f))},
        {"deep_diver",      ("500m", new Color(0.11f,0.34f,0.72f))},
        {"ocean_explorer",  ("1 KM", new Color(0.09f,0.26f,0.65f))},
        {"midnight_zone",   ("2.5K", new Color(0.07f,0.18f,0.55f))},
        {"into_the_abyss",  ("5 KM", new Color(0.05f,0.10f,0.42f))},
        {"first_bite",      ("EAT",  new Color(0.18f,0.58f,0.24f))},
        {"well_fed",        ("+15",  new Color(0.15f,0.52f,0.20f))},
        {"puffer_snack",    ("PUF",  new Color(0.22f,0.54f,0.14f))},
        {"comeback_kid",    ("HEAL", new Color(0.14f,0.64f,0.34f))},
        {"shark_slayer",    ("SHK",  new Color(0.68f,0.14f,0.14f))},
        {"veteran",         ("x10",  new Color(0.55f,0.09f,0.09f))},
        {"armed",           ("GUN",  new Color(0.62f,0.32f,0.06f))},
        {"trigger_happy",   ("x30",  new Color(0.55f,0.26f,0.05f))},
        {"force_field",     ("ORB",  new Color(0.10f,0.54f,0.64f))},
        {"damage_blocked",  ("DEF",  new Color(0.08f,0.44f,0.58f))},
        {"untouchable",     ("0DMG", new Color(0.48f,0.14f,0.54f))},
        {"on_the_edge",     ("1 HP", new Color(0.62f,0.10f,0.28f))},
        {"second_chance",   ("2ND",  new Color(0.38f,0.18f,0.58f))},
        {"ink_storm",       ("INK",  new Color(0.22f,0.10f,0.55f))},
        {"key_hunter",      ("KEY",  new Color(0.60f,0.46f,0.04f))},
        {"treasure_seeker", ("BOX",  new Color(0.55f,0.40f,0.04f))},
        {"chain_reaction",  ("BOOM", new Color(0.64f,0.28f,0.05f))},
    };

    // ── refs ──────────────────────────────────────────────────────────────────
    private readonly List<GameObject> cards = new List<GameObject>();
    private readonly Dictionary<string, GameObject> cardByID = new Dictionary<string, GameObject>();
    private TextMeshProUGUI progressText;
    private ScrollRect scrollRect;
    private bool built;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void OnEnable()  => AchievementManager.OnUnlocked += OnUnlock;
    void OnDisable() => AchievementManager.OnUnlocked -= OnUnlock;

    void OnUnlock(AchievementData data)
    {
        Refresh();
        if (built && gameObject.activeSelf && cardByID.TryGetValue(data.id, out var card))
            StartCoroutine(RevealPulse(card));
    }

    IEnumerator RevealPulse(GameObject card)
    {
        const float duration = 0.40f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float s = 1f + 0.12f * Mathf.Sin(Mathf.PI * t / duration);
            card.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        card.transform.localScale = Vector3.one;
    }

    // ── Public ────────────────────────────────────────────────────────────────
    public System.Action onClose;

    public void Show()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        if (!built) Build();
        else        Refresh();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
    }

    public void Hide()
    {
        EventSystem.current?.SetSelectedGameObject(null);
        gameObject.SetActive(false);
        onClose?.Invoke();
    }

    public void Toggle() { if (gameObject.activeSelf) Hide(); else Show(); }

    // ── Build ─────────────────────────────────────────────────────────────────
    void Build()
    {
        built = true;

        var selfRT = GetComponent<RectTransform>();
        selfRT.anchorMin = Vector2.zero; selfRT.anchorMax = Vector2.one;
        selfRT.offsetMin = selfRT.offsetMax = Vector2.zero;

        var bg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        bg.color = panelBg; bg.raycastTarget = true;

        // Header — 1/4 of screen height
        float headerH = Screen.height / 4f;

        var titleGO = new GameObject("PanelTitle", typeof(RectTransform), typeof(CanvasRenderer));
        titleGO.transform.SetParent(transform, false);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1); titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot     = new Vector2(.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, 0);
        titleRT.sizeDelta        = new Vector2(0, headerH);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "Achievements";
        titleTMP.color     = new Color(1f, .88f, .3f);
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.enableAutoSizing   = true;
        titleTMP.fontSizeMin        = 20f;
        titleTMP.fontSizeMax        = 300f;
        titleTMP.raycastTarget      = false;
        titleTMP.enableWordWrapping = false;
        if (cardFont != null) titleTMP.font = cardFont;
        ApplyTitleStyle(titleTMP, true);

        // Progress counter — sits just below the header
        float progressY = -(headerH + 4f);
        progressText = MakeTMP(gameObject, "Progress",
            aMin: new Vector2(0,1), aMax: new Vector2(1,1), piv: new Vector2(.5f,1),
            pos: new Vector2(0, progressY), sz: new Vector2(0, 48),
            fs: 30, col: new Color(.60f,.65f,.80f),
            align: TextAlignmentOptions.Center, bold: false, txt: "");

        // Scroll view — starts below header + progress (progress text is 48px tall + 4px gap = 52, add 12px margin)
        float scrollTopPad = headerH + 64f;
        var viewGO = new GameObject("View",
            typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
        viewGO.transform.SetParent(transform, false);
        var viewRT = viewGO.GetComponent<RectTransform>();
        viewRT.anchorMin = Vector2.zero; viewRT.anchorMax = Vector2.one;
        viewRT.offsetMin = new Vector2(sidePad, 10f);
        viewRT.offsetMax = new Vector2(-sidePad, -scrollTopPad);

        // Content — stretched width, auto height
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewGO.transform, false);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0,1); contentRT.anchorMax = new Vector2(1,1);
        contentRT.pivot     = new Vector2(.5f,1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = Vector2.zero;

        // Two-column grid
        var glg = contentGO.AddComponent<GridLayoutGroup>();
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2;
        glg.spacing         = new Vector2(cardSpacing, cardSpacing);
        glg.padding         = new RectOffset(4, 4, 4, 4);
        glg.childAlignment  = TextAnchor.UpperLeft;

        // Use root canvas width — always valid, no ForceUpdateCanvases needed
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        while (rootCanvas != null && !rootCanvas.isRootCanvas)
            rootCanvas = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();
        float canvasW  = rootCanvas != null ? rootCanvas.GetComponent<RectTransform>().rect.width : Screen.width;
        float contentW = Mathf.Max(200f, canvasW - 2f * sidePad) - glg.padding.left - glg.padding.right;
        float cellW    = Mathf.Max(100f, (contentW - cardSpacing) / 2f);
        glg.cellSize   = new Vector2(cellW, cardHeight);

        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = viewGO.GetComponent<ScrollRect>();
        sr.viewport = viewRT; sr.content = contentRT;
        sr.horizontal = false; sr.vertical = true;
        sr.scrollSensitivity = 40; sr.movementType = ScrollRect.MovementType.Clamped;
        sr.inertia = true; sr.decelerationRate = 0.135f;
        scrollRect = sr;

        for (int i = 0; i < AchievementManager.All.Count; i++)
            cards.Add(BuildCard(contentGO, AchievementManager.All[i], i));

        BuildCloseButton();
        Refresh();
    }

    // ── Single card ───────────────────────────────────────────────────────────
    GameObject BuildCard(GameObject parent, AchievementData data, int index)
    {
        bool u      = IsUnlocked(data.id);
        bool secret = data.isSecret && !u;

        Icons.TryGetValue(data.id, out var icon);
        Color iconCol  = icon.col == default ? new Color(.3f,.3f,.5f) : icon.col;
        string iconLbl = string.IsNullOrEmpty(icon.lbl) ? "?" : icon.lbl;
        if (!u) iconCol = Color.Lerp(iconCol, new Color(.1f,.1f,.15f), 0.55f);

        Sprite iconSprite = SpriteFor(index, u);

        // Card root
        var card = new GameObject("Card_" + data.id,
            typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        card.transform.SetParent(parent.transform, false);
        card.GetComponent<Image>().color = u ? cardUnlocked : cardLocked;
        cardByID[data.id] = card;

        // Left accent bar
        MakeSolidRect(card, "Accent",
            aMin: new Vector2(0,0), aMax: new Vector2(0,1),
            piv: new Vector2(0,.5f), offMin: Vector2.zero, offMax: Vector2.zero,
            w: 8, h: 0, col: u ? accentUnlocked : accentLocked);

        // Icon
        float iconSize = Mathf.Min(cardHeight - 40f, 130f);
        float iconLeft = 18f;

        var iconBg = new GameObject("IconBg",
            typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        iconBg.transform.SetParent(card.transform, false);
        var ibRT = iconBg.GetComponent<RectTransform>();
        ibRT.anchorMin = new Vector2(0,.5f); ibRT.anchorMax = new Vector2(0,.5f);
        ibRT.pivot     = new Vector2(0,.5f);
        ibRT.anchoredPosition = new Vector2(iconLeft, 0);
        ibRT.sizeDelta        = new Vector2(iconSize, iconSize);

        var ibImg = iconBg.GetComponent<Image>();
        if (iconSprite != null)
        {
            ibImg.sprite = iconSprite; ibImg.color = u ? Color.white : new Color(.55f,.55f,.55f,1f);
            ibImg.preserveAspect = true;
        }
        else ibImg.color = iconCol;

        var iconTxt = MakeTMP(iconBg, "IconTxt",
            aMin: Vector2.zero, aMax: Vector2.one, piv: new Vector2(.5f,.5f),
            pos: Vector2.zero, sz: new Vector2(-4,0),
            fs: iconLbl.Length <= 3 ? 36f : 26f,
            col: new Color(1,1,1, u ? 0.95f : 0.50f),
            align: TextAlignmentOptions.Center, bold: true,
            txt: secret ? "?" : iconLbl);
        iconTxt.gameObject.SetActive(secret || iconSprite == null);

        // Text area
        float textLeft  = iconLeft + iconSize + 14f;
        float textRight = 50f;

        // Title — top half of card, explicit offsets so it always shows
        var titleTMP = MakeTMP(card, "CardTitle",
            aMin: new Vector2(0, 0.5f), aMax: new Vector2(1, 1f),
            piv: new Vector2(0.5f, 0.5f),
            pos: Vector2.zero, sz: Vector2.zero,
            fs: 52, col: u ? Color.white : titleLocked,
            align: TextAlignmentOptions.Left, bold: true,
            txt: secret ? "???" : data.title);
        { var r = titleTMP.GetComponent<RectTransform>(); r.offsetMin = new Vector2(textLeft, 4f); r.offsetMax = new Vector2(-textRight, -4f); }
        ApplyTitleStyle(titleTMP, u);

        // Separator at 50%
        var sep = new GameObject("TitleSep",
            typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        sep.transform.SetParent(card.transform, false);
        var sepRT = sep.GetComponent<RectTransform>();
        sepRT.anchorMin = new Vector2(0, 0.5f); sepRT.anchorMax = new Vector2(1, 0.5f);
        sepRT.pivot     = new Vector2(0.5f, 0.5f);
        sepRT.offsetMin = new Vector2(textLeft, -1f);
        sepRT.offsetMax = new Vector2(-10f, 1f);
        sep.GetComponent<Image>().color = u
            ? new Color(1f, 0.85f, 0.2f, 0.25f)
            : new Color(1f, 1f, 1f, 0.05f);

        // Description — bottom half, explicit offsets
        var descTMP = MakeTMP(card, "CardDesc",
            aMin: new Vector2(0, 0f), aMax: new Vector2(1, 0.5f),
            piv: new Vector2(0.5f, 0.5f),
            pos: Vector2.zero, sz: Vector2.zero,
            fs: 34, col: u ? descUnlocked : descLocked,
            align: TextAlignmentOptions.Left, bold: false,
            txt: secret ? "Complete more of the game to reveal this secret." : data.description);
        { var r = descTMP.GetComponent<RectTransform>(); r.offsetMin = new Vector2(textLeft, 4f); r.offsetMax = new Vector2(-textRight, -4f); }

        // Unlocked badge — sprite if assigned, otherwise hidden
        var chk = new GameObject("ChkBadge",
            typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        chk.transform.SetParent(card.transform, false);
        var chkRT = chk.GetComponent<RectTransform>();
        chkRT.anchorMin = new Vector2(1,.5f); chkRT.anchorMax = new Vector2(1,.5f);
        chkRT.pivot     = new Vector2(1,.5f);
        chkRT.sizeDelta = new Vector2(80, 80);
        chkRT.anchoredPosition = new Vector2(-10, 0);
        var chkImg = chk.GetComponent<Image>();
        if (unlockedBadgeSprite != null)
        {
            chkImg.sprite = unlockedBadgeSprite;
            chkImg.color  = new Color(1f, 0.88f, 0.2f, 1f);
            chkImg.preserveAspect = true;
        }
        else
        {
            chkImg.color = Color.clear;  // invisible until user assigns sprite
        }
        chk.SetActive(u);

        return card;
    }

    // ── Refresh ───────────────────────────────────────────────────────────────
    void Refresh()
    {
        if (progressText != null)
        {
            int total = AchievementManager.All.Count, got = 0;
            foreach (var a in AchievementManager.All) if (IsUnlocked(a.id)) got++;
            progressText.text = $"{got} / {total} Unlocked";
        }

        for (int i = 0; i < cards.Count && i < AchievementManager.All.Count; i++)
        {
            var data = AchievementManager.All[i];
            var card = cards[i];
            if (card == null) continue;

            bool u      = IsUnlocked(data.id);
            bool secret = data.isSecret && !u;

            Icons.TryGetValue(data.id, out var icon);
            Color iconCol = icon.col == default ? new Color(.3f,.3f,.5f) : icon.col;
            if (!u) iconCol = Color.Lerp(iconCol, new Color(.1f,.1f,.15f), 0.55f);

            Sprite sp = SpriteFor(i, u);
            card.GetComponent<Image>().color = u ? cardUnlocked : cardLocked;

            foreach (Transform child in card.transform)
            {
                switch (child.name)
                {
                    case "Accent":
                        child.GetComponent<Image>().color = u ? accentUnlocked : accentLocked;
                        break;

                    case "IconBg":
                    {
                        var img = child.GetComponent<Image>();
                        if (sp != null)
                        {
                            img.sprite = sp; img.color = u ? Color.white : new Color(.55f,.55f,.55f,1f);
                            img.preserveAspect = true;
                        }
                        else { img.sprite = null; img.color = iconCol; }

                        var itxt = child.Find("IconTxt")?.GetComponent<TextMeshProUGUI>();
                        if (itxt != null)
                        {
                            itxt.gameObject.SetActive(secret || sp == null);
                            itxt.text  = secret ? "?" : (icon.lbl ?? "?");
                            itxt.color = new Color(1,1,1, u ? 0.95f : 0.50f);
                        }
                        break;
                    }

                    case "CardTitle":
                    {
                        var tt = child.GetComponent<TextMeshProUGUI>();
                        if (tt != null)
                        {
                            tt.text  = secret ? "???" : data.title;
                            tt.color = u ? Color.white : titleLocked;
                            ApplyTitleStyle(tt, u);
                        }
                        break;
                    }

                    case "TitleSep":
                        child.GetComponent<Image>().color = u
                            ? new Color(1f, 0.85f, 0.2f, 0.25f)
                            : new Color(1f, 1f, 1f, 0.05f);
                        break;

                    case "CardDesc":
                    {
                        var dt = child.GetComponent<TextMeshProUGUI>();
                        if (dt != null)
                        {
                            dt.text  = secret ? "Complete more of the game to reveal this secret." : data.description;
                            dt.color = u ? descUnlocked : descLocked;
                        }
                        break;
                    }

                    case "ChkBadge":
                        child.gameObject.SetActive(u);
                        break;
                }
            }
        }
    }

    // ── Close button ──────────────────────────────────────────────────────────
    void BuildCloseButton()
    {
        var go = new GameObject("CloseBtn",
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasRenderer));
        go.transform.SetParent(transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1,1); rt.anchorMax = new Vector2(1,1);
        rt.pivot     = new Vector2(1,1);
        rt.sizeDelta = new Vector2(80, 80);
        rt.anchoredPosition = new Vector2(-10, -10);
        var img = go.GetComponent<Image>();
        img.color         = new Color(.70f,.10f,.10f,1f);
        img.raycastTarget = true;
        MakeTMP(go, "X",
            aMin: Vector2.zero, aMax: Vector2.one, piv: new Vector2(.5f,.5f),
            pos: Vector2.zero, sz: Vector2.zero,
            fs: 32, col: Color.white,
            align: TextAlignmentOptions.Center, bold: true, txt: "X");
        go.GetComponent<Button>().onClick.AddListener(Hide);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static void ApplyTitleStyle(TextMeshProUGUI tmp, bool unlocked)
    {
        if (unlocked)
        {
            tmp.enableVertexGradient = true;
            tmp.colorGradient = new VertexGradient(
                new Color(1.00f, 1.00f, 0.70f),
                new Color(1.00f, 1.00f, 0.70f),
                new Color(0.95f, 0.60f, 0.05f),
                new Color(0.95f, 0.60f, 0.05f)
            );
            tmp.outlineWidth = 0.12f;
            tmp.outlineColor = new Color32(100, 50, 0, 200);
        }
        else
        {
            tmp.enableVertexGradient = false;
            tmp.outlineWidth = 0f;
        }
    }

    Sprite SpriteFor(int index, bool unlocked)
    {
        if (achievementIcons != null && index < achievementIcons.Length && achievementIcons[index] != null)
            return achievementIcons[index];
        if (!unlocked && lockedIcon != null) return lockedIcon;
        return null;
    }

    bool IsUnlocked(string id)
    {
        if (AchievementManager.Instance != null)
            return AchievementManager.Instance.IsUnlocked(id);
        return PlayerPrefs.GetInt("ach_unlock_" + id, 0) == 1;
    }

    void MakeSolidRect(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 piv, Vector2 offMin, Vector2 offMax,
        float w, float h, Color col)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = piv;
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        if (w > 0) rt.sizeDelta = new Vector2(w, rt.sizeDelta.y);
        if (h > 0) rt.sizeDelta = new Vector2(rt.sizeDelta.x, h);
        go.GetComponent<Image>().color = col;
    }

    TextMeshProUGUI MakeTMP(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 piv, Vector2 pos, Vector2 sz,
        float fs, Color col, TextAlignmentOptions align, bool bold, string txt)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.pivot = piv; rt.anchoredPosition = pos; rt.sizeDelta = sz;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = txt; tmp.fontSize = fs; tmp.color = col;
        tmp.alignment = align;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.raycastTarget = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.enableWordWrapping = true;
        if (cardFont != null) tmp.font = cardFont;
        return tmp;
    }
}
