using UnityEngine;

public class KeyRotate : MonoBehaviour
{
    public float rotateSpeed = 360f; // degrees per second

    void Update()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
