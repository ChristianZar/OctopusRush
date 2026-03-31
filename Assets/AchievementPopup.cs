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

    [Header("Appearance")]
    public float  panelWidth   = 420f;
    public float  panelHeight  = 100f;
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
        titleText.text  = $"{d.icon}  {d.title}";
        descText.text   = d.description;

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

        var bg = go.GetComponent<Image>();
        bg.color = bgColor;

        // Gold left accent bar
        AddRect(go, "Accent", new Color(1f, 0.82f, 0.18f),
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(0f, 1f),
            pivot: new Vector2(0f, 0.5f), sizeDelta: new Vector2(5f, 0f), pos: Vector2.zero);

        // "Achievement Unlocked!" header (top-right, small)
        headerText = AddTMP(go, "Header",
            anchorMin: new Vector2(0f, 0.58f), anchorMax: new Vector2(1f, 1f),
            pos: new Vector2(-12f, 0f), sizeDelta: new Vector2(-24f, 0f),
            fontSize: 14f, color: new Color(0.75f, 0.80f, 0.95f),
            alignment: TextAlignmentOptions.Right);

        // Achievement title (icon + name)
        titleText = AddTMP(go, "Title",
            anchorMin: new Vector2(0f, 0.3f), anchorMax: new Vector2(1f, 0.72f),
            pos: new Vector2(18f, 0f), sizeDelta: new Vector2(-36f, 0f),
            fontSize: 19f, color: titleColor,
            alignment: TextAlignmentOptions.Left, bold: true);

        // Description
        descText = AddTMP(go, "Desc",
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(1f, 0.36f),
            pos: new Vector2(18f, 0f), sizeDelta: new Vector2(-36f, 0f),
            fontSize: 15f, color: descColor,
            alignment: TextAlignmentOptions.Left);

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
