using UnityEngine;

public class BloodPuff : MonoBehaviour
{
    public float life = 0.8f;
    public float startScale = 0.3f;
    public float endScale = 1.6f;
    public Vector2 drift = new Vector2(0.1f, 0.2f);

    private SpriteRenderer sr;
    private float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        t = 0f;
        transform.localScale = Vector3.one * startScale;
        drift = new Vector2(0.1f, 0.2f);
        drift *= Random.Range(0.7f, 1.3f);
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) { Color c = sr.color; c.a = 0.8f; sr.color = c; }
    }

    void Update()
    {
        t += Time.deltaTime / life;

        float s = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = Vector3.one * s;

        transform.position += (Vector3)(drift * Time.deltaTime);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0.8f, 0f, t);
            sr.color = c;
        }

        if (t >= 1f) GetComponent<PoolReturn>()?.ReturnToPool();
    }
}
