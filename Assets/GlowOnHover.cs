using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GlowOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Color normalColor = Color.white;
    public Color glowColor = new Color(0.6f, 1f, 1f); // light blue glow

    public float speed = 5f;
    private bool hovering;

    void Update()
    {
        image.color = Color.Lerp(
            image.color,
            hovering ? glowColor : normalColor,
            Time.deltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
