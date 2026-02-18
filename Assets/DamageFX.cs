using UnityEngine;

public class DamageFX : MonoBehaviour
{
    [SerializeField] private GameObject bloodPrefab;

    public void SpawnBlood()
    {
        if (bloodPrefab == null) return;

        Instantiate(bloodPrefab, transform.position, Quaternion.identity);
    }
}
