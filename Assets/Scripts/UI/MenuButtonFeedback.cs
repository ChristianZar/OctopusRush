using UnityEngine;
using UnityEngine.EventSystems;

/// Attach to any main-menu button to get a smooth scale-up on hover and scale-down on exit.
/// MainMenuStyler adds this automatically to all menu buttons.
[RequireComponent(typeof(RectTransform))]
public class MenuButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover")]
    public float hoverScale  = 1.08f;
    public float pressScale  = 0.95f;
    public float lerpSpeed   = 12f;

    private Vector3 _targetScale = Vector3.one;
    private RectTransform _rt;

    void Awake() => _rt = GetComponent<RectTransform>();

    void Update()
    {
        _rt.localScale = Vector3.Lerp(_rt.localScale, _targetScale, Time.unscaledDeltaTime * lerpSpeed);
    }

    public void OnPointerEnter(PointerEventData _) => _targetScale = Vector3.one * hoverScale;
    public void OnPointerExit(PointerEventData _)  => _targetScale = Vector3.one;
    public void OnPointerDown(PointerEventData _)  => _targetScale = Vector3.one * pressScale;
    public void OnPointerUp(PointerEventData _)    => _targetScale = Vector3.one * hoverScale;
}
