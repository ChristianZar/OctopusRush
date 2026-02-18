using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 shakeOffset;
    private Coroutine routine;

    public void Shake(float duration, float magnitude)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * magnitude,
                Random.Range(-1f, 1f) * magnitude,
                0f
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }

    void LateUpdate()
    {
        transform.position += shakeOffset;
    }
}
