using UnityEngine;

public class KillFarBelowCamera : MonoBehaviour
{
    [SerializeField] private float killDistanceBelow = 30f;
    private Transform camT;

    void Start()
    {
        if (Camera.main != null)
            camT = Camera.main.transform;
    }

    void Update()
    {
        if (camT == null) return;

        if (transform.position.y < camT.position.y - killDistanceBelow)
        {
            Destroy(gameObject);
        }
    }
}
