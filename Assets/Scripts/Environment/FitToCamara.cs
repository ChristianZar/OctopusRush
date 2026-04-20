using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitToCamera : MonoBehaviour
{
    public bool coverScreen = true; // true = no gaps (may crop). false = no crop (may show gaps)

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) return;

        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleX = screenWidth / spriteSize.x;
        float scaleY = screenHeight / spriteSize.y;

        float scale = coverScreen ? Mathf.Max(scaleX, scaleY) : Mathf.Min(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
