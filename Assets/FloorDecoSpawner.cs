using System.Collections.Generic;
using UnityEngine;

/// Handles two visual improvements on a floor tile:
///   2. Edge shadows  — dark fade sprites at each end to sell the "drop-off into a deep hole"
///   4. Decorations   — rocks / coral sprites randomly scattered on the tile surface
///
/// Add this component alongside EndlessFloor on the same GameObject.
/// Assign sprites in the Inspector; leave any slot empty to skip it.
public class FloorDecoSpawner : MonoBehaviour
{
    [Header("Edge Drop-off Shadow")]
    [Tooltip("Assign shadow.png (or any dark gradient sprite).")]
    public Sprite edgeShadowSprite;
    [Tooltip("Width of the shadow strip at each edge.")]
    public float  edgeShadowWidth  = 1.2f;
    public float  edgeShadowHeight = 2.5f;
    public Color  edgeShadowColor  = new Color(0f, 0f, 0f, 0.75f);
    public int    edgeSortOrder    = 10;   // renders in front of the floor tile

    [Header("Surface Decorations (rocks / coral)")]
    public Sprite[] decorSprites;          // assign rocks.png, coral.png, etc.
    public int   minDecos = 1;
    public int   maxDecos = 4;
    [Tooltip("Y offset above the floor surface centre.")]
    public float decorYOffset = 0.5f;
    [Tooltip("Scale range for decoration sprites.")]
    public float decorMinScale = 0.4f;
    public float decorMaxScale = 0.9f;
    public int   decorSortOrder = 5;

    // ── runtime ──────────────────────────────────────────────────────────────
    private readonly List<GameObject> decos = new List<GameObject>();
    private bool edgesCreated = false;

    /// Called once by EndlessFloor after tile width is known.
    public void Init(float tileWidth)
    {
        if (!edgesCreated)
        {
            CreateEdgeShadows(tileWidth);
            edgesCreated = true;
        }
        SpawnDecos(tileWidth);
    }

    /// Called every time the tile recycles — clears old decos and spawns fresh ones.
    public void Refresh(float tileWidth)
    {
        ClearDecos();
        SpawnDecos(tileWidth);
    }

    // ── edge shadows ─────────────────────────────────────────────────────────

    void CreateEdgeShadows(float tileWidth)
    {
        if (edgeShadowSprite == null) return;

        float halfTile = tileWidth * 0.5f;

        // Left edge (sprite faces right into the tile)
        CreateEdge("EdgeShadow_L", -halfTile, false);

        // Right edge (flipped — faces left into the tile)
        CreateEdge("EdgeShadow_R",  halfTile, true);
    }

    void CreateEdge(string goName, float localX, bool flipX)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(localX, decorYOffset * 0.5f, 0f);
        go.transform.localScale    = new Vector3(edgeShadowWidth, edgeShadowHeight, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite           = edgeShadowSprite;
        sr.color            = edgeShadowColor;
        sr.flipX            = flipX;
        sr.sortingLayerName = GetComponent<SpriteRenderer>()?.sortingLayerName ?? "Default";
        sr.sortingOrder     = edgeSortOrder;
    }

    // ── surface decorations ───────────────────────────────────────────────────

    void SpawnDecos(float tileWidth)
    {
        if (decorSprites == null || decorSprites.Length == 0) return;

        int count   = Random.Range(minDecos, maxDecos + 1);
        float halfW = tileWidth * 0.5f;
        float padding = tileWidth * 0.08f; // keep away from edges

        // Track used X positions to avoid obvious stacking
        var usedX = new List<float>();

        for (int i = 0; i < count; i++)
        {
            Sprite spr = decorSprites[Random.Range(0, decorSprites.Length)];
            if (spr == null) continue;

            float x = RandomX(-halfW + padding, halfW - padding, usedX, 1.0f);
            usedX.Add(x);

            var go = new GameObject("FloorDeco");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(x, decorYOffset, 0f);

            float s = Random.Range(decorMinScale, decorMaxScale);
            go.transform.localScale = new Vector3(
                Random.value > 0.5f ? s : -s,  // random flip for variety
                s, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite           = spr;
            sr.sortingLayerName = GetComponent<SpriteRenderer>()?.sortingLayerName ?? "Default";
            sr.sortingOrder     = decorSortOrder;

            decos.Add(go);
        }
    }

    void ClearDecos()
    {
        foreach (var d in decos)
            if (d != null) Destroy(d);
        decos.Clear();
    }

    // Returns an X that isn't too close to any already-used X position
    float RandomX(float min, float max, List<float> used, float minDist)
    {
        for (int attempt = 0; attempt < 15; attempt++)
        {
            float x = Random.Range(min, max);
            bool ok = true;
            foreach (float u in used)
                if (Mathf.Abs(x - u) < minDist) { ok = false; break; }
            if (ok) return x;
        }
        return Random.Range(min, max); // fallback
    }
}
