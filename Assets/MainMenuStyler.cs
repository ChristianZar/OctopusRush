using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Organizes main menu layout at runtime:
///   - Moves ButtonContainer to the RIGHT half of the screen
///   - Stacks buttons with original widths, consistent gaps, FloatUI preserved
///   - Fixes HelpPanel text and makes it scrollable
/// Drag onto any GameObject in the Menu scene.
/// </summary>
public class MainMenuStyler : MonoBehaviour
{
    // ── Controls / How To Play text ───────────────────────────────────────────

    const string HowToPlayText =
        "HOW TO PLAY\n\n" +
        "Hold [W] to swim upward. Release to drift down with the current.\n\n" +
        "Tap [SPACE] to release an ink cloud — it slows nearby enemies " +
        "and gives you a quick speed boost.\n\n" +
        "Collect keys scattered through the ocean. " +
        "When your key bar is full, swim up to a treasure chest and press [E] to open it.\n\n" +
        "Pick up Shield Orbs to block one hit. " +
        "Grab the AK-47 powerup and press [F] to shoot enemies for 15 seconds.\n\n" +
        "Eat fish to heal — every 3 fish restores 1 HP. " +
        "Eat a deflated pufferfish for a free heal too!\n\n" +
        "Avoid sharks, mines, jellyfish, crabs, and anglerfish. " +
        "Your health drains slowly over time, so keep moving and keep eating.\n\n" +
        "Survive as long as possible, travel as far as you can, " +
        "and beat your best distance score!\n\n" +
        "Press [ESC] or [P] to pause at any time.";

    // ── Layout ────────────────────────────────────────────────────────────────
    //
    //   Screen split: Logo fills LEFT half, buttons stack on RIGHT half
    //
    //   Buttons use their original image widths (unchanged).
    //   Consistent 20px gap between each button.
    //   Stack is vertically centred on the right side.
    //
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        RepositionLogo();
        RepositionButtons();
        FixHelpText();
        MakeHelpScrollable();
    }

    void RepositionLogo()
    {
        var go = GameObject.Find("Logo");
        if (go == null) return;

        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;

        // Restore original position/size/anchor
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, 555f);
        rt.sizeDelta        = new Vector2(900f, 400f);
    }

    void RepositionButtons()
    {
        // 3 rows × 2 columns grid
        // Row 0: Start      | How To Play
        // Row 1: Achievement | Skins
        // Row 2: Credits     | Quit
        //
        // Each button keeps its original image width.
        // Heights unified per row for clean alignment.

        float colGap  = 24f;   // horizontal gap between the two columns
        float rowGap  = 20f;   // vertical gap between rows
        float rowH    = 130f;  // uniform row height

        // Original widths
        float wStart  = 460f; float wHelp  = 450f;
        float wAch    = 400f; float wSkins = 400f;
        float wCred   = 440f; float wQuit  = 350f;

        // Column centres: left column at -x, right column at +x
        // Use the wider pair (Start/Help row) to set the column offset
        float colOffset = (wStart / 2f + colGap / 2f);   // ~254

        // Total grid height
        float totalH = rowH * 3 + rowGap * 2;
        float topY   = totalH / 2f - rowH / 2f;  // centre Y of first row

        float[] rowY = new float[]
        {
             topY,
             topY - (rowH + rowGap),
             topY - (rowH + rowGap) * 2f,
        };

        Apply("StartButton",       new Vector2(-colOffset, rowY[0]), new Vector2(wStart,  rowH));
        Apply("HowToPlayButton",   new Vector2( colOffset, rowY[0]), new Vector2(wHelp,   rowH));
        Apply("AchievementButton", new Vector2(-colOffset, rowY[1]), new Vector2(wAch,    rowH));
        Apply("SkinsButton",       new Vector2( colOffset, rowY[1]), new Vector2(wSkins,  rowH));
        Apply("CreditsButton",     new Vector2(-colOffset, rowY[2]), new Vector2(wCred,   rowH));
        Apply("QuitButton",        new Vector2( colOffset, rowY[2]), new Vector2(wQuit,   rowH));
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

    // ── Help panel text ───────────────────────────────────────────────────────

    void FixHelpText()
    {
        foreach (var tmp in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (tmp.text.Contains("CONTROLS") || tmp.text.Contains("WASD") ||
                tmp.text.Contains("ARROW")    || tmp.text.Contains("HOW TO PLAY"))
            {
                tmp.text               = HowToPlayText;
                tmp.enableWordWrapping = true;
                tmp.alignment          = TextAlignmentOptions.Left;
                break;
            }
        }
    }

    // ── Scrollable help panel ─────────────────────────────────────────────────

    void MakeHelpScrollable()
    {
        TextMeshProUGUI contentTMP = null;
        foreach (var tmp in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (tmp.text.Contains("HOW TO PLAY") || tmp.text.Contains("Hold [W]"))
            {
                contentTMP = tmp;
                break;
            }
        }
        if (contentTMP == null) return;

        if (contentTMP.transform.parent != null &&
            contentTMP.transform.parent.name == "HelpScrollContent") return;

        var panelGO = contentTMP.transform.parent?.gameObject;
        if (panelGO == null) return;

        var viewGO = new GameObject("HelpViewport", typeof(RectTransform), typeof(RectMask2D));
        viewGO.transform.SetParent(panelGO.transform, false);
        var viewRT = viewGO.GetComponent<RectTransform>();
        viewRT.anchorMin = Vector2.zero;
        viewRT.anchorMax = Vector2.one;
        viewRT.offsetMin = new Vector2(40f,  80f);
        viewRT.offsetMax = new Vector2(-40f, -80f);

        var contentGO = new GameObject("HelpScrollContent", typeof(RectTransform));
        contentGO.transform.SetParent(viewGO.transform, false);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin        = new Vector2(0f, 1f);
        contentRT.anchorMax        = new Vector2(1f, 1f);
        contentRT.pivot            = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta        = Vector2.zero;
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        contentTMP.transform.SetParent(contentGO.transform, false);
        var tmpRT = contentTMP.GetComponent<RectTransform>();
        tmpRT.anchorMin        = new Vector2(0f, 1f);
        tmpRT.anchorMax        = new Vector2(1f, 1f);
        tmpRT.pivot            = new Vector2(0.5f, 1f);
        tmpRT.anchoredPosition = Vector2.zero;
        tmpRT.sizeDelta        = Vector2.zero;
        contentTMP.enableWordWrapping = true;

        var sr = panelGO.GetComponent<ScrollRect>() ?? panelGO.AddComponent<ScrollRect>();
        sr.viewport          = viewRT;
        sr.content           = contentRT;
        sr.horizontal        = false;
        sr.vertical          = true;
        sr.scrollSensitivity = 40f;
        sr.movementType      = ScrollRect.MovementType.Clamped;
        sr.inertia           = true;
        sr.decelerationRate  = 0.135f;
        sr.verticalNormalizedPosition = 1f;

        var img = panelGO.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }
}
