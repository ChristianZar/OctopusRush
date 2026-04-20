using System.Collections.Generic;
using UnityEngine;

/// Spawns a cluster of overlapping translucent blobs that expand and fade,
/// giving a spreading underwater-ink-cloud appearance.
public class InkCloud : MonoBehaviour
{
    [Header("Color")]
    public Color inkColor = new Color(0.04f, 0.02f, 0.15f, 0.88f); // dark purple-blue ink

    [Header("Animation")]
    public float lifeTime   = 1.8f;
    public float startScale = 0.3f;
    public float endScale   = 3.0f;

    [Header("Blob Cloud Shape")]
    public int   blobCount     = 5;    // extra blobs spawned around the centre
    public float blobMaxOffset = 0.5f; // max distance from centre

    // ---- runtime ----
    private SpriteRenderer[] renderers;
    private float[]          baseAlphas;
    private float            elapsed;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) { Destroy(gameObject, lifeTime); return; }

        // Main renderer: set ink colour
        sr.color = inkColor;
        Sprite spr = sr.sprite;

        var rList = new List<SpriteRenderer> { sr };

        // Spawn blob children to break up the hard circle silhouette
        for (int i = 0; i < blobCount; i++)
        {
            Vector2 offset    = Random.insideUnitCircle * blobMaxOffset;
            float   blobScale = Random.Range(0.45f, 0.85f);
            float   blobAlpha = Random.Range(0.30f, 0.65f);

            var blob = new GameObject("InkBlob");
            blob.transform.SetParent(transform, false);
            blob.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            blob.transform.localScale    = Vector3.one * blobScale;

            var bsr = blob.AddComponent<SpriteRenderer>();
            bsr.sprite           = spr;
            bsr.material         = sr.material;
            bsr.sortingLayerName = sr.sortingLayerName;
            bsr.sortingOrder     = sr.sortingOrder - 1;
            Color c = inkColor;
            c.a = blobAlpha;
            bsr.color = c;
            rList.Add(bsr);
        }

        renderers  = rList.ToArray();
        baseAlphas = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            baseAlphas[i] = renderers[i].color.a;

        transform.localScale = Vector3.one * startScale;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifeTime);

        // Expand from startScale to endScale
        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

        // Hold full opacity for the first 25%, then fade out
        float fadeT = Mathf.Clamp01((t - 0.25f) / 0.75f);
        float alpha = 1f - fadeT * fadeT; // quadratic ease-in fade

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color c = renderers[i].color;
            c.a = baseAlphas[i] * alpha;
            renderers[i].color = c;
        }
    }
}
