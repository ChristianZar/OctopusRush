using UnityEngine;

public class CameraAutoScroll : MonoBehaviour
{
    [Header("Normal Scroll")]
    public float speed = 3f;

    [Header("Boost")]
    public float boostMultiplier = 1.8f;
    public float boostLerpSpeed = 6f;

    private float currentMultiplier = 1f;
    private float targetMultiplier = 1f;

    void Update()
    {
        currentMultiplier = Mathf.Lerp(currentMultiplier, targetMultiplier, boostLerpSpeed * Time.deltaTime);
        transform.position += Vector3.right * (speed * currentMultiplier) * Time.deltaTime;
    }

    public void SetBoosting(bool boosting)
    {
        targetMultiplier = boosting ? boostMultiplier : 1f;
    }
}