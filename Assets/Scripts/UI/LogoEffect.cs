using UnityEngine;
using UnityEngine.UI;

public class LogoEffect : MonoBehaviour
{
    [Header("Float")]
    public float floatSpeed  = 1.1f;
    public float floatHeight = 10f;     // canvas units

    [Header("Pulse")]
    public float pulseSpeed  = 0.7f;
    public float pulseAmount = 0.04f;   // 4% scale breath

    [Header("Shimmer")]
    public float shimmerSpeed = 1.4f;
    public Color shimmerColor = new Color(0.72f, 0.93f, 1f, 1f);  // ocean-light blue

    private RectTransform rt;
    private Image         img;
    private Vector2       startPos;
    private Vector3       startScale;
    private Color         baseColor;

    void Start()
    {
        rt         = GetComponent<RectTransform>();
        img        = GetComponent<Image>();
        startPos   = rt.anchoredPosition;
        startScale = rt.localScale;
        baseColor  = img != null ? img.color : Color.white;
    }

    void Update()
    {
        // Float up and down
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        rt.anchoredPosition = startPos + new Vector2(0f, offsetY);

        // Breathing pulse
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        rt.localScale = startScale * pulse;

        // Ocean shimmer — cool blue tint that gently washes over the logo
        if (img != null)
        {
            float t = (Mathf.Sin(Time.time * shimmerSpeed) + 1f) * 0.5f;
            img.color = Color.Lerp(baseColor, shimmerColor, t * 0.28f);
        }
    }
}
