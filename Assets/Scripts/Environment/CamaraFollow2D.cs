using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(3f, 0f, -10f);
    public float smooth = 5f;

    private Vector3 shakeOffset;

    public void SetShakeOffset(Vector3 off)
    {
        shakeOffset = off;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset + shakeOffset;
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
    }
}
