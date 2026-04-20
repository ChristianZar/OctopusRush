using UnityEngine;

public class SharkSway : MonoBehaviour
{
    public float swayAmount = 0.15f;
    public float swaySpeed = 1.5f;

    float baseY;

    void Start()
    {
        baseY = transform.localPosition.y;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * swaySpeed) * swayAmount;

        Vector3 p = transform.localPosition;
        p.y = baseY + offset;   // only touch Y
        transform.localPosition = p;
    }
}
