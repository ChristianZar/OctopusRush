using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Attach to the GameOverPanel.
/// Assign orb.png (or any soft-circle sprite) to dropSprite for best results.
/// Works over Screen Space Overlay canvas and while Time.timeScale = 0.
public class GameOverRain : MonoBehaviour
{
    [Header("Sprite (assign orb.png for soft-edged drops)")]
    public Sprite dropSprite;       // leave empty for plain rectangle fallback

    [Header("Drop Colour")]
    public Color heavyColor = new Color(0.5f, 0.72f, 1f, 0.55f);  // large drops
    public Color lightColor = new Color(0.65f, 0.82f, 1f, 0.28f); // fine mist drops

    [Header("Drop Size")]
    public float minWidth  =  3f;   // canvas px — heavy drop
    public float maxWidth  =  7f;
    public float minHeight = 22f;   // canvas px tall
    public float maxHeight = 55f;
    public float angleDeg  = 12f;   // diagonal slant

    [Header("Density & Speed")]
    public int   maxDrops   = 55;
    public float spawnRate  = 18f;   // drops/sec
    public float minSpeed   = 180f;  // canvas units/sec
    public float maxSpeed   = 320f;

    [Header("Splash (uses same sprite, optional)")]
    public bool  showSplash      = true;
    public float splashDuration  = 0.35f;  // seconds (unscaled)
    public float splashMaxWidth  = 28f;    // canvas px at peak
    public float splashMaxHeight =  8f;

    // ── internals ────────────────────────────────────────────────────────────
    private struct Drop
    {
        public RectTransform rt;
        public Image         img;
        public float         speed;
        public bool          isHeavy;
    }

    private struct Splash
    {
        public RectTransform rt;
        public Image         img;
        public float         elapsed;
        public Color         baseColor;
    }

    private readonly List<Drop>           active  = new List<Drop>();
    private readonly List<Splash>         splashes = new List<Splash>();
    private readonly Queue<RectTransform> dropPool   = new Queue<RectTransform>();
    private readonly Queue<RectTransform> splashPool = new Queue<RectTransform>();

    private RectTransform canvasRect;
    private float         spawnTimer;
    private float         bottomY;   // y where drops are considered "at floor"

    // ── lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        Canvas c = GetComponentInParent<Canvas>();
        canvasRect = c != null ? c.GetComponent<RectTransform>() : null;
    }

    void OnEnable()
    {
        spawnTimer = 0f;
        bottomY = canvasRect != null ? -canvasRect.rect.height * 0.5f : -540f;
    }

    void OnDisable()
    {
        foreach (var d in active)   ReturnDrop(d.rt);
        foreach (var s in splashes) ReturnSplash(s.rt);
        active.Clear();
        splashes.Clear();
    }

    // ── update ────────────────────────────────────────────────────────────────
    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        SpawnDrops(dt);
        MoveDrops(dt);
        AnimateSplashes(dt);
    }

    // ── spawning ──────────────────────────────────────────────────────────────
    void SpawnDrops(float dt)
    {
        spawnTimer += dt;
        float interval = 1f / Mathf.Max(spawnRate, 1f);

        while (spawnTimer >= interval && active.Count < maxDrops)
        {
            CreateDrop();
            spawnTimer -= interval;
        }
    }

    void CreateDrop()
    {
        bool isHeavy = Random.value > 0.35f;  // 65% heavy, 35% light mist

        RectTransform rt;
        Image img;

        if (dropPool.Count > 0)
        {
            rt = dropPool.Dequeue();
            rt.gameObject.SetActive(true);
            img = rt.GetComponent<Image>();
        }
        else
        {
            var go = new GameObject("Drop", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            rt  = go.GetComponent<RectTransform>();
            img = go.GetComponent<Image>();
            img.raycastTarget = false;
            if (dropSprite != null) img.sprite = dropSprite;
        }

        float cw = canvasRect != null ? canvasRect.rect.width  : 1920f;
        float ch = canvasRect != null ? canvasRect.rect.height : 1080f;

        float w = isHeavy
            ? Random.Range(minWidth, maxWidth)
            : Random.Range(minWidth * 0.5f, minWidth);
        float h = isHeavy
            ? Random.Range(minHeight * 0.7f, maxHeight)
            : Random.Range(minHeight * 0.3f, minHeight * 0.6f);

        img.color = Color.Lerp(lightColor, heavyColor, isHeavy ? Random.Range(0.5f, 1f) : Random.Range(0f, 0.4f));

        rt.sizeDelta        = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(
            Random.Range(-cw * 0.55f, cw * 0.55f),
            ch * 0.5f + h + Random.Range(0f, ch * 0.35f));
        rt.localRotation = Quaternion.Euler(0f, 0f, -angleDeg);

        float speed = isHeavy
            ? Random.Range(minSpeed * 0.8f, maxSpeed)
            : Random.Range(minSpeed * 0.4f, minSpeed * 0.7f);

        active.Add(new Drop { rt = rt, img = img, speed = speed, isHeavy = isHeavy });
    }

    // ── movement ──────────────────────────────────────────────────────────────
    void MoveDrops(float dt)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        var   dir = new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad));

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var d = active[i];
            if (d.rt == null) { active.RemoveAt(i); continue; }

            d.rt.anchoredPosition += dir * d.speed * dt;

            if (d.rt.anchoredPosition.y < bottomY)
            {
                if (showSplash && d.isHeavy)
                    CreateSplash(d.rt.anchoredPosition, d.img.color);

                ReturnDrop(d.rt);
                active.RemoveAt(i);
            }
        }
    }

    // ── splashes ──────────────────────────────────────────────────────────────
    void CreateSplash(Vector2 pos, Color baseColor)
    {
        RectTransform rt;
        Image img;

        if (splashPool.Count > 0)
        {
            rt = splashPool.Dequeue();
            rt.gameObject.SetActive(true);
            img = rt.GetComponent<Image>();
        }
        else
        {
            var go = new GameObject("Splash", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            rt  = go.GetComponent<RectTransform>();
            img = go.GetComponent<Image>();
            img.raycastTarget = false;
            if (dropSprite != null) img.sprite = dropSprite;
        }

        rt.sizeDelta        = new Vector2(4f, 2f);
        rt.anchoredPosition = new Vector2(pos.x, bottomY);
        rt.localRotation    = Quaternion.identity;

        Color c = baseColor;
        c.a = baseColor.a * 0.9f;
        img.color = c;

        splashes.Add(new Splash { rt = rt, img = img, elapsed = 0f, baseColor = c });
    }

    void AnimateSplashes(float dt)
    {
        for (int i = splashes.Count - 1; i >= 0; i--)
        {
            var s = splashes[i];
            if (s.rt == null) { splashes.RemoveAt(i); continue; }

            s.elapsed += dt;
            float t = Mathf.Clamp01(s.elapsed / splashDuration);

            // Expand outward and fade
            s.rt.sizeDelta = new Vector2(
                Mathf.Lerp(4f, splashMaxWidth,  t),
                Mathf.Lerp(2f, splashMaxHeight, t));

            Color c = s.baseColor;
            c.a = s.baseColor.a * (1f - t * t);
            s.img.color = c;

            splashes[i] = s;

            if (t >= 1f)
            {
                ReturnSplash(s.rt);
                splashes.RemoveAt(i);
            }
        }
    }

    // ── pooling ───────────────────────────────────────────────────────────────
    void ReturnDrop(RectTransform rt)
    {
        if (rt == null) return;
        rt.gameObject.SetActive(false);
        dropPool.Enqueue(rt);
    }

    void ReturnSplash(RectTransform rt)
    {
        if (rt == null) return;
        rt.gameObject.SetActive(false);
        splashPool.Enqueue(rt);
    }
}
