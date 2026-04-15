---
description: Use coroutines for all timed sequences and delays
globs: ["Assets/Scripts/**/*.cs"]
---

Use coroutines (`IEnumerator` + `StartCoroutine`) for delayed sequences, fade-ins, animation pauses, and any multi-step timed logic.

Do **not** use `Invoke` or `InvokeRepeating` — coroutines are preferred for readability and cancellability.

```csharp
// Preferred
StartCoroutine(FadeOut(0.5f));

IEnumerator FadeOut(float duration) {
    float t = 0f;
    while (t < duration) {
        t += Time.deltaTime;
        canvasGroup.alpha = 1f - (t / duration);
        yield return null;
    }
    gameObject.SetActive(false);
}
```

Stop coroutines explicitly when the object deactivates or the sequence is interrupted:
```csharp
StopCoroutine(myCoroutine);
// or
StopAllCoroutines();
```
