using UnityEngine;

public class InkCloud : MonoBehaviour
{
    public float lifeTime = 1.5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
