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
        transform.localScale = Vector3.one * startScale;

        // small random drift variation so it feels organic
        drift *= Random.Range(0.7f, 1.3f);
    }

    void Update()
    {
        t += Time.deltaTime / life;

        // expand
        float s = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = Vector3.one * s;

        // drift
        transform.position += (Vector3)(drift * Time.deltaTime);

        // fade
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0.8f, 0f, t);
            sr.color = c;
        }

        if (t >= 1f) Destroy(gameObject);
    }
}
