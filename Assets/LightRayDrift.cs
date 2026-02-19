using UnityEngine;

public class LightRayDrift : MonoBehaviour
{
    public float driftSpeed = 0.02f;
    public float driftAmount = 0.2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * driftSpeed) * driftAmount;
        transform.position = new Vector3(startPos.x + offset, startPos.y, startPos.z);
    }
}
