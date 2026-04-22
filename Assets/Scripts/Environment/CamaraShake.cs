using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine routine;
    private float currentMagnitude = 0f;

    [Header("Clamp Settings")]
    [SerializeField] private bool clampY = true;

    public void Shake(float duration, float magnitude)
    {
        if (routine != null && magnitude < currentMagnitude) return;
        if (routine != null) StopCoroutine(routine);
        currentMagnitude = magnitude;
        routine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        originalPosition = transform.position;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Vector3 offset = new Vector3(x, y, 0f);
            Vector3 targetPos = originalPosition + offset;

            if (clampY && targetPos.y < originalPosition.y)
                targetPos.y = originalPosition.y;

            transform.position = targetPos;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        currentMagnitude = 0f;
        routine = null;
    }
}
