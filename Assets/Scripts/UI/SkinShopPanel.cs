using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Procedurally-built skin shop panel.
/// Add this component to an inactive GameObject in your Menu canvas.
/// Assign SkinManager (drag the prefab/GO) and optionally a cardFont.
/// Call Show() / Hide() from MainMenuManager buttons.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SkinShopPanel : MonoBehaviour
{
    [Header("Font (optional)")]
    public TMP_FontAsset cardFont;

    [Header("Layout")]
    public float cardHeight  = 200f;
    public float cardSpacing =   8f;
    public float sidePad     =  24f;

    [Header("Colours")]
    public Color panelBg      = new Color(0.04f, 0.06f, 0.13f, 0.98f);
    public Color cardOwned    = new Color(0.06f, 0.18f, 0.09f, 1f);
    public Color cardLocked   = new Color(0.08f, 0.09f, 0.15f, 1f);
    public Color cardEquipped = new Color(0.06f, 0.25f, 0.35f, 1f);
    public Color accentOwned    = new Color(0.28f, 0.85f, 0.45f, 1f);
    public Color accentLocked   = new Color(0.16f, 0.18f, 0.27f, 1f);
    public Color accentEquipped = new Color(0.20f, 0.72f, 0.90f, 1f);

    // ── runtime refs ──────────────────────────────────────────────────────────
    private TextMeshProUGUI keyBalanceText;
    private ScrollRect scrollRect;
    private readonly List<GameObject> cards = new List<GameObject>();
    private bool built;

    // ── Public API ────────────────────────────────────────────────────────────
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

    // ── Build (runs once) ─────────────────────────────────────────────────────
    void Build()
    {
        built = true;

        var selfRT = GetComponent<RectTransform>();
        selfRT.anchorMin = Vector2.zero; selfRT.anchorMax = Vector2.one;
        selfRT.offsetMin = selfRT.offsetMax = Vector2.zero;

        var bg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        bg.color = panelBg; bg.raycastTarget = true;

        // ── Header ────────────────────────────────────────────────────────────
        float headerH = Screen.height / 4f;

        var titleGO = new GameObject("PanelTitle", typeof(RectTransform), typeof(CanvasRenderer));
        titleGO.transform.SetParent(transform, false);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1); titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot     = new Vector2(.5f, 1);
        titleRT.anchoredPosition = Vector2.zero;
        titleRT.sizeDelta        = new Vector2(0, headerH);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "Skin Shop";
        titleTMP.color     = new Color(1f, .88f, .3f);
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.enableAutoSizing   = true;
        titleTMP.fontSizeMin        = 20f;
        titleTMP.fontSizeMax        = 300f;
        titleTMP.raycastTarget      = false;
        titleTMP.enableWordWrapping = false;
        if (cardFont != null) titleTMP.font = cardFont;

        // ── Key balance ───────────────────────────────────────────────────────
        float balanceY = -(headerH + 4f);
        keyBalanceText = MakeTMP(gameObject, "KeyBalance",
            aMin: new Vector2(0,1), aMax: new Vector2(1,1), piv: new Vector2(.5f,1),
            pos: new Vector2(0, balanceY), sz: new Vector2(0, 48),
            fs: 30, col: new Color(1f, .90f, .35f),
            align: TextAlignmentOptions.Center, bold: false, txt: "");

        // ── Scroll view ───────────────────────────────────────────────────────
        float scrollTopPad = headerH + 64f;
        var viewGO = new GameObject("View",
            typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
        viewGO.transform.SetParent(transform, false);
        var viewRT = viewGO.GetComponent<RectTransform>();
        viewRT.anchorMin = Vector2.zero; viewRT.anchorMax = Vector2.one;
        viewRT.offsetMin = new Vector2(sidePad, 10f);
        viewRT.offsetMax = new Vector2(-sidePad, -scrollTopPad);

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewGO.transform, false);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0,1); contentRT.anchorMax = new Vector2(1,1);
        contentRT.pivot     = new Vector2(.5f,1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = Vector2.zero;

        var glg = contentGO.AddComponent<GridLayoutGroup>();
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2;
        glg.spacing         = new Vector2(cardSpacing, cardSpacing);
        glg.padding         = new RectOffset(4, 4, 4, 4);
        glg.childAlignment  = TextAnchor.UpperLeft;

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

        // ── Cards ─────────────────────────────────────────────────────────────
        var mgr = SkinManager.Instance;
        if (mgr != null && mgr.skins != null)
        {
            for (int i = 0; i < mgr.skins.Length; i++)
                cards.Add(BuildCard(contentGO, mgr.skins[i], i));
        }

        BuildCloseButton();
        Refresh();
    }

    // ── Single card ───────────────────────────────────────────────────────────
    GameObject BuildCard(GameObject parent, SkinData skin, int index)
    {
        var mgr      = SkinManager.Instance;
        bool owned    = mgr != null && mgr.IsOwned(index);
        bool equipped = mgr != null && mgr.EquippedIndex == index;

        var card = new GameObject("SkinCard_" + index,
            typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        card.transform.SetParent(parent.transform, false);
        card.GetComponent<Image>().color = equipped ? cardEquipped : owned ? cardOwned : cardLocked;

        // Left accent bar
        MakeSolidRect(card, "Accent",
            aMin: new Vector2(0,0), aMax: new Vector2(0,1),
            piv: new Vector2(0,.5f), offMin: Vector2.zero, offMax: Vector2.zero,
            w: 8, col: equipped ? accentEquipped : owned ? accentOwned : accentLocked);

        // Thumbnail
        float thumbSize = cardHeight - 20f;
        var thumbGO = new GameObject("Thumb",
            typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        thumbGO.transform.SetParent(card.transform, false);
        var thumbRT = thumbGO.GetComponent<RectTransform>();
        thumbRT.anchorMin = new Vector2(0,.5f); thumbRT.anchorMax = new Vector2(0,.5f);
        thumbRT.pivot     = new Vector2(0,.5f);
        thumbRT.anchoredPosition = new Vector2(18f, 0);
        thumbRT.sizeDelta        = new Vector2(thumbSize, thumbSize);
        var thumbImg = thumbGO.GetComponent<Image>();
        if (skin != null && skin.thumbnail != null)
        {
            thumbImg.sprite = skin.thumbnail;
            thumbImg.preserveAspect = true;
            thumbImg.color = owned ? Color.white : new Color(.45f,.45f,.45f,1f);
        }
        else
        {
            thumbImg.color = owned ? new Color(.3f,.6f,.9f,.6f) : new Color(.2f,.2f,.3f,.6f);
        }

        // Lock overlay on thumbnail
        if (!owned)
        {
            var lockGO = new GameObject("Lock",
                typeof(RectTransform), typeof(CanvasRenderer));
            lockGO.transform.SetParent(thumbGO.transform, false);
            var lRT = lockGO.GetComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = lRT.offsetMax = Vector2.zero;
            MakeTMP(lockGO, "LockIcon",
                aMin: Vector2.zero, aMax: Vector2.one, piv: new Vector2(.5f,.5f),
                pos: Vector2.zero, sz: Vector2.zero,
                fs: 40, col: new Color(1,1,1,.7f),
                align: TextAlignmentOptions.Center, bold: true, txt: "🔒");
        }

        // Text area
        float textLeft  = 18f + thumbSize + 12f;
        float textRight = 12f;

        // Skin name
        string displayName = (skin != null && !string.IsNullOrEmpty(skin.skinName)) ? skin.skinName : "Skin " + index;
        MakeTMP(card, "SkinName",
            aMin: new Vector2(0, .55f), aMax: new Vector2(1,1),
            piv: new Vector2(0,.5f),
            pos: Vector2.zero, sz: Vector2.zero,
            fs: 28, col: owned ? Color.white : new Color(.42f,.44f,.56f,1f),
            align: TextAlignmentOptions.Left, bold: true, txt: displayName,
            offsetMin: new Vector2(textLeft, 4), offsetMax: new Vector2(-textRight, -4));

        // Cost or "Owned" label
        string costLabel = index == 0 ? "Free" : owned ? "Owned" : $"{skin?.keyCost ?? 0} Keys";
        Color costColor  = index == 0 ? new Color(.4f,.9f,.4f) :
                           owned     ? new Color(.4f,.8f,.4f) :
                                       new Color(1f,.85f,.2f);
        MakeTMP(card, "CostLabel",
            aMin: new Vector2(0, 0), aMax: new Vector2(1, .55f),
            piv: new Vector2(0,.5f),
            pos: Vector2.zero, sz: Vector2.zero,
            fs: 22, col: costColor,
            align: TextAlignmentOptions.Left, bold: false, txt: costLabel,
            offsetMin: new Vector2(textLeft, 4), offsetMax: new Vector2(-textRight, -4));

        // Action button (Buy / Equip / Equipped)
        BuildActionButton(card, index, owned, equipped);

        return card;
    }

    void BuildActionButton(GameObject card, int index, bool owned, bool equipped)
    {
        var mgr = SkinManager.Instance;

        string label;
        Color  btnColor;
        bool   interactable;

        if (equipped)
        {
            label = "Equipped"; btnColor = new Color(.10f,.55f,.72f,1f); interactable = false;
        }
        else if (owned)
        {
            label = "Equip"; btnColor = new Color(.14f,.52f,.22f,1f); interactable = true;
        }
        else
        {
            int cost = (mgr != null && mgr.skins != null && index < mgr.skins.Length && mgr.skins[index] != null)
                ? mgr.skins[index].keyCost : 0;
            bool canAfford = mgr != null && mgr.LifetimeKeys >= cost;
            label = "Buy"; btnColor = canAfford ? new Color(.55f,.40f,.04f,1f) : new Color(.22f,.22f,.28f,1f);
            interactable = canAfford;
        }

        var btnGO = new GameObject("ActionBtn",
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasRenderer));
        btnGO.transform.SetParent(card.transform, false);
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1,.5f); btnRT.anchorMax = new Vector2(1,.5f);
        btnRT.pivot     = new Vector2(1,.5f);
        btnRT.sizeDelta = new Vector2(120, 48);
        btnRT.anchoredPosition = new Vector2(-10, 0);
        btnGO.GetComponent<Image>().color = btnColor;

        MakeTMP(btnGO, "BtnLabel",
            aMin: Vector2.zero, aMax: Vector2.one, piv: new Vector2(.5f,.5f),
            pos: Vector2.zero, sz: Vector2.zero,
            fs: 22, col: Color.white,
            align: TextAlignmentOptions.Center, bold: true, txt: label);

        var btn = btnGO.GetComponent<Button>();
        btn.interactable = interactable;
        int captured = index;
        btn.onClick.AddListener(() => OnActionClicked(captured));
    }

    void OnActionClicked(int index)
    {
        var mgr = SkinManager.Instance;
        if (mgr == null) return;

        if (mgr.IsOwned(index))
            mgr.Equip(index);
        else
            mgr.TryPurchase(index);

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────────────────────
    void Refresh()
    {
        var mgr = SkinManager.Instance;

        if (keyBalanceText != null)
            keyBalanceText.text = mgr != null ? $"Keys: {mgr.LifetimeKeys}" : "";

        if (mgr == null || mgr.skins == null) return;

        for (int i = 0; i < cards.Count && i < mgr.skins.Length; i++)
        {
            var card  = cards[i];
            var skin  = mgr.skins[i];
            bool owned    = mgr.IsOwned(i);
            bool equipped = mgr.EquippedIndex == i;

            card.GetComponent<Image>().color = equipped ? cardEquipped : owned ? cardOwned : cardLocked;

            // Collect the old ActionBtn to destroy AFTER the loop (never modify children during foreach)
            GameObject oldActionBtn = null;

            foreach (Transform child in card.transform)
            {
                switch (child.name)
                {
                    case "Accent":
                        child.GetComponent<Image>().color =
                            equipped ? accentEquipped : owned ? accentOwned : accentLocked;
                        break;

                    case "Thumb":
                    {
                        var img = child.GetComponent<Image>();
                        if (img.sprite != null)
                            img.color = owned ? Color.white : new Color(.45f,.45f,.45f,1f);
                        else
                            img.color = owned ? new Color(.3f,.6f,.9f,.6f) : new Color(.2f,.2f,.3f,.6f);
                        var lockT = child.Find("Lock");
                        if (lockT != null) lockT.gameObject.SetActive(!owned);
                        break;
                    }

                    case "SkinName":
                    {
                        var t = child.GetComponent<TextMeshProUGUI>();
                        if (t != null) t.color = owned ? Color.white : new Color(.42f,.44f,.56f,1f);
                        break;
                    }

                    case "CostLabel":
                    {
                        var t = child.GetComponent<TextMeshProUGUI>();
                        if (t != null)
                        {
                            t.text  = i == 0 ? "Free" : owned ? "Owned" : $"{skin?.keyCost ?? 0} Keys";
                            t.color = i == 0 ? new Color(.4f,.9f,.4f) :
                                      owned  ? new Color(.4f,.8f,.4f) : new Color(1f,.85f,.2f);
                        }
                        break;
                    }

                    case "ActionBtn":
                        oldActionBtn = child.gameObject;
                        break;
                }
            }

            // Destroy old button and rebuild outside the foreach
            if (oldActionBtn != null) Destroy(oldActionBtn);
            BuildActionButton(card, i, owned, equipped);
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
        go.GetComponent<Image>().color = new Color(.70f,.10f,.10f,1f);
        MakeTMP(go, "X",
            aMin: Vector2.zero, aMax: Vector2.one, piv: new Vector2(.5f,.5f),
            pos: Vector2.zero, sz: Vector2.zero,
            fs: 32, col: Color.white,
            align: TextAlignmentOptions.Center, bold: true, txt: "X");
        go.GetComponent<Button>().onClick.AddListener(Hide);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void MakeSolidRect(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 piv, Vector2 offMin, Vector2 offMax,
        float w, Color col)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = piv;
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        if (w > 0) rt.sizeDelta = new Vector2(w, rt.sizeDelta.y);
        go.GetComponent<Image>().color = col;
    }

    TextMeshProUGUI MakeTMP(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 piv, Vector2 pos, Vector2 sz,
        float fs, Color col, TextAlignmentOptions align, bool bold, string txt,
        Vector2? offsetMin = null, Vector2? offsetMax = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.pivot = piv; rt.anchoredPosition = pos; rt.sizeDelta = sz;
        if (offsetMin.HasValue) rt.offsetMin = offsetMin.Value;
        if (offsetMax.HasValue) rt.offsetMax = offsetMax.Value;
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
