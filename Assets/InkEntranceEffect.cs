using UnityEngine;

public class InkEntranceEffect : MonoBehaviour
{
    public float expandSpeed = 1.5f;
    public float fadeSpeed = 1f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        Color c = sr.color;
        c.a -= fadeSpeed * Time.deltaTime;
        sr.color = c;

        if (c.a <= 0)
            Destroy(gameObject);
    }
}
