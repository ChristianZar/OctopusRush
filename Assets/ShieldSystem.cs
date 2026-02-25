using System.Collections;
using UnityEngine;

public class ShieldSystem : MonoBehaviour
{
    [Header("Shield Settings")]
    public GameObject shieldBubblePrefab;
    public float shieldDuration = 10f;

    private GameObject currentBubble;
    public bool IsShieldActive { get; private set; }

    Coroutine shieldRoutine;

    public void ActivateShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(ShieldRoutine());
    }

    IEnumerator ShieldRoutine()
    {
        IsShieldActive = true;

        if (currentBubble != null)
            Destroy(currentBubble);

        currentBubble = Instantiate(shieldBubblePrefab, transform);
        currentBubble.transform.localPosition = Vector3.zero;

        yield return new WaitForSeconds(shieldDuration);

        if (currentBubble != null)
            Destroy(currentBubble);

        IsShieldActive = false;
        shieldRoutine = null;
    }
}