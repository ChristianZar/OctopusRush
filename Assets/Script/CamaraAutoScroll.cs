using UnityEngine;

public class CameraAutoScroll : MonoBehaviour
{
    public float speed = 3f;

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
}
