using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Place on any child of the Canvas (e.g. a new empty GO called "AchievementPopup").
/// Builds its own UI, subscribes to AchievementManager.OnUnlocked automatically.
/// Works while Time.timeScale = 0 (game-over screen, pause menu).
[RequireComponent(typeof(RectTransform))]
public class AchievementPopup : MonoBehaviour
{
    [Header("Font (optional — drag Showpop SDF here)")]
    public TMP_FontAsset popupFont;

    [Header("Icons (same order as AchievementManager.All)")]
    public Sprite[] achievementIcons;

    [Header("Appearance")]
    public float  panelWidth   = 500f;
    public float  panelHeight  = 180f;
    public Color  bgColor      = new Color(0.06f, 0.10f, 0.20f, 0.96f);
    public Color  accentColor  = new Color(1f, 0.82f, 0.18f);   // gold
    public Color  titleColor   = new Color(1f, 0.88f, 0.3f);
    public Color  descColor    = new Color(0.82f, 0.90f, 1f);

    [Header("Timing")]
    public float holdDuration  = 3.0f;
    public float slideDuration = 0.30f;

    [Header("Position (top-right corner)")]
    public float marginRight = 16f;
    public float marginTop   = 16f;

    // ── UI refs ──────────────────────────────────────────────────────────────
    private RectTransform panel;
    private TextMeshProUGUI headerText;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descText;
    private Image iconImage;

    // ── State machine ─────────────────────────────────────────────────────────
    private readonly Queue<AchievementData> queue = new Queue<AchievementData>();
    private bool  busy;
    private float timer;
    private enum Phase { SlideIn, Hold, SlideOut }
    private Phase phase;

    private float OnScreenX  => -marginRight;
    private float OffScreenX =>   panelWidth + 50f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()  => BuildPanel();
    void OnEnable()  => AchievementManager.OnUnlocked += Enqueue;
    void OnDisable() => AchievementManager.OnUnlocked -= Enqueue;

    // ── Queue / show ──────────────────────────────────────────────────────────
    void Enqueue(AchievementData data)
    {
        queue.Enqueue(data);
        if (!busy) ShowNext();
    }

    void ShowNext()
    {
        if (queue.Count == 0) { busy = false; return; }
        busy = true;

        var d = queue.Dequeue();
        headerText.text = "Achievement Unlocked!";
        titleText.text  = d.title;
        descText.text   = d.description;

        int idx = AchievementManager.All.FindIndex(a => a.id == d.id);
        if (iconImage != null)
        {
            iconImage.sprite = (idx >= 0 && achievementIcons != null && idx < achievementIcons.Length)
                ? achievementIcons[idx] : null;
            iconImage.enabled = iconImage.sprite != null;
            iconImage.preserveAspect = true;
        }

        SetX(OffScreenX);
        phase = Phase.SlideIn;
        timer = 0f;
    }

    // ── Animation ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (!busy || panel == null) return;
        timer += Time.unscaledDeltaTime;

        switch (phase)
        {
            case Phase.SlideIn:
                SetX(Mathf.Lerp(OffScreenX, OnScreenX, EaseOut(timer / slideDuration)));
                if (timer >= slideDuration) { SetX(OnScreenX); phase = Phase.Hold; timer = 0f; }
                break;

            case Phase.Hold:
                if (timer >= holdDuration) { phase = Phase.SlideOut; timer = 0f; }
                break;

            case Phase.SlideOut:
                SetX(Mathf.Lerp(OnScreenX, OffScreenX, EaseIn(timer / slideDuration)));
                if (timer >= slideDuration) { SetX(OffScreenX); ShowNext(); }
                break;
        }
    }

    void SetX(float x)
    {
        if (panel == null) return;
        var p = panel.anchoredPosition;
        p.x = x;
        panel.anchoredPosition = p;
    }

    static float EaseOut(float t) => 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
    static float EaseIn (float t) => Mathf.Clamp01(t) * Mathf.Clamp01(t);

    // ── UI builder ────────────────────────────────────────────────────────────
    void BuildPanel()
    {
        // Stretch this GO to fill the entire canvas so child anchors work correctly
        var selfRT = GetComponent<RectTransform>();
        selfRT.anchorMin = Vector2.zero;
        selfRT.anchorMax = Vector2.one;
        selfRT.offsetMin = Vector2.zero;
        selfRT.offsetMax = Vector2.zero;

        // Root panel anchored to top-right corner
        var go = new GameObject("PopupPanel", typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        go.transform.SetParent(transform, false);
        panel = go.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(1f, 1f);
        panel.anchorMax = new Vector2(1f, 1f);
        panel.pivot     = new Vector2(1f, 1f);
        panel.sizeDelta = new Vector2(panelWidth, panelHeight);
        panel.anchoredPosition = new Vector2(OffScreenX, -marginTop);
        go.GetComponent<Image>().color = bgColor;

        // Gold left accent bar
        AddRect(go, "Accent", new Color(1f, 0.82f, 0.18f),
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(0f, 1f),
            pivot: new Vector2(0f, 0.5f), sizeDelta: new Vector2(6f, 0f), pos: Vector2.zero);

        // Icon box — left side, square, vertically centred
        float iconSize = panelHeight - 24f;
        float iconLeft = 14f;
        float textLeft = iconLeft + iconSize + 12f;

        var ibGO = new GameObject("IconBg", typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        ibGO.transform.SetParent(go.transform, false);
        var ibRT = ibGO.GetComponent<RectTransform>();
        ibRT.anchorMin = new Vector2(0f, 0.5f); ibRT.anchorMax = new Vector2(0f, 0.5f);
        ibRT.pivot     = new Vector2(0f, 0.5f);
        ibRT.anchoredPosition = new Vector2(iconLeft, 0f);
        ibRT.sizeDelta        = new Vector2(iconSize, iconSize);
        ibGO.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.24f, 1f);

        // Sprite icon inside icon box
        var iImgGO = new GameObject("IconImg", typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        iImgGO.transform.SetParent(ibGO.transform, false);
        var iImgRT = iImgGO.GetComponent<RectTransform>();
        iImgRT.anchorMin = new Vector2(0.05f, 0.05f); iImgRT.anchorMax = new Vector2(0.95f, 0.95f);
        iImgRT.offsetMin = Vector2.zero; iImgRT.offsetMax = Vector2.zero;
        iconImage = iImgGO.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.enabled = false;

        // "Achievement Unlocked!" header — top 38%, right-aligned
        headerText = AddTMP(go, "Header",
            anchorMin: new Vector2(0f, 0.62f), anchorMax: new Vector2(1f, 1f),
            pos: Vector2.zero, sizeDelta: Vector2.zero,
            fontSize: 20f, color: new Color(0.75f, 0.80f, 0.95f),
            alignment: TextAlignmentOptions.Right);
        { var r = headerText.GetComponent<RectTransform>(); r.offsetMin = new Vector2(textLeft, 0f); r.offsetMax = new Vector2(-14f, 0f); }

        // Achievement title — middle, bold gold
        titleText = AddTMP(go, "Title",
            anchorMin: new Vector2(0f, 0.28f), anchorMax: new Vector2(1f, 0.68f),
            pos: Vector2.zero, sizeDelta: Vector2.zero,
            fontSize: 26f, color: titleColor,
            alignment: TextAlignmentOptions.Left, bold: true);
        { var r = titleText.GetComponent<RectTransform>(); r.offsetMin = new Vector2(textLeft, 0f); r.offsetMax = new Vector2(-14f, 0f); }

        // Description — bottom 36%
        descText = AddTMP(go, "Desc",
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(1f, 0.36f),
            pos: Vector2.zero, sizeDelta: Vector2.zero,
            fontSize: 20f, color: descColor,
            alignment: TextAlignmentOptions.Left);
        { var r = descText.GetComponent<RectTransform>(); r.offsetMin = new Vector2(textLeft, 0f); r.offsetMax = new Vector2(-14f, 0f); }

        SetX(OffScreenX);
    }

    void AddRect(GameObject parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot = pivot; rt.sizeDelta = sizeDelta; rt.anchoredPosition = pos;
        go.GetComponent<Image>().color = color;
    }

    TextMeshProUGUI AddTMP(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 sizeDelta,
        float fontSize, Color color, TextAlignmentOptions alignment, bool bold = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = pos;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = alignment;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.raycastTarget = false;
        if (popupFont != null) tmp.font = popupFont;
        return tmp;
    }
}
