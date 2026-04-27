using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileControls : MonoBehaviour
{
    [SerializeField] private bool forceShow = false;

    [Header("Button Sprites")]
    [SerializeField] private Sprite swimSprite;
    [SerializeField] private Sprite inkSprite;
    [SerializeField] private Sprite fireSprite;
    [SerializeField] private Sprite openSprite;

    void Start()
    {
        if (!Application.isMobilePlatform && !forceShow) return;

        var bridge = MobileInputBridge.Instance;
        if (bridge == null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        Transform root = canvas.transform;

        // ── Left side ─────────────────────────────────────────────────────────
        // SWIM — bottom-left corner, large hold button
        MakeHoldButton(root, "SWIM", swimSprite,
            anchor: Vector2.zero, pivot: Vector2.zero,
            pos: new Vector2(30, 30), size: new Vector2(630, 630),
            onDown: () => bridge.SwimHeld = true,
            onUp:   () => bridge.SwimHeld = false);

        // ── Right side cluster ─────────────────────────────────────────────────
        // Layout (no overlaps, fits 1080px tall landscape screen):
        //              [ FIRE ]   ← upper-right
        //  [ INK ]  [ OPEN ]      ← lower-right

        // INK — bottom-right, lower-left of cluster
        MakeHoldButton(root, "INK", inkSprite,
            anchor: Vector2.right, pivot: Vector2.right,
            pos: new Vector2(-510, 30), size: new Vector2(450, 450),
            onDown: () => bridge.InkHeld = true,
            onUp:   () => bridge.InkHeld = false);

        // OPEN — bottom-right corner (tap)
        MakeTapButton(root, "OPEN", openSprite,
            anchor: Vector2.right, pivot: Vector2.right,
            pos: new Vector2(-30, 30), size: new Vector2(450, 450),
            onDown: () => bridge.TriggerInteract());

        // FIRE — above OPEN, right column
        MakeHoldButton(root, "FIRE", fireSprite,
            anchor: Vector2.right, pivot: Vector2.right,
            pos: new Vector2(-30, 510), size: new Vector2(450, 450),
            onDown: () => bridge.ShootHeld = true,
            onUp:   () => bridge.ShootHeld = false);
    }

    // ── Factories ─────────────────────────────────────────────────────────────

    void MakeHoldButton(Transform parent, string id, Sprite sprite,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
        System.Action onDown, System.Action onUp)
    {
        var go = MakeBase(parent, id, sprite, anchor, pivot, pos, size);
        var et = go.AddComponent<EventTrigger>();
        AddEntry(et, EventTriggerType.PointerDown, onDown);
        AddEntry(et, EventTriggerType.PointerUp,   onUp);
    }

    void MakeTapButton(Transform parent, string id, Sprite sprite,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
        System.Action onDown)
    {
        var go = MakeBase(parent, id, sprite, anchor, pivot, pos, size);
        var et = go.AddComponent<EventTrigger>();
        AddEntry(et, EventTriggerType.PointerDown, onDown);
    }

    GameObject MakeBase(Transform parent, string id, Sprite sprite,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("MobileBtn_" + id, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchor;
        rt.anchorMax        = anchor;
        rt.pivot            = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        var img = go.GetComponent<Image>();
        img.sprite        = sprite;
        img.color         = new Color(1f, 1f, 1f, 0.85f);
        img.raycastTarget = true;

        return go;
    }

    void AddEntry(EventTrigger et, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        et.triggers.Add(entry);
    }
}
