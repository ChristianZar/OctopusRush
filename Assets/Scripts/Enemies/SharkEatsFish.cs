using UnityEngine;

public class SharkEatFish : MonoBehaviour
{
    [Header("Eat Settings")]
    public string fishTag = "Fish";
    public float eatCooldown = 0.15f;

    [Header("Blood FX")]
    public GameObject bloodPuffPrefab;
    public Vector3 bloodOffset = Vector3.zero;
    public float bloodLifeTime = 1.5f;

    private float nextEatTime = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < nextEatTime) return;

        if (other.CompareTag(fishTag))
        {
            nextEatTime = Time.time + eatCooldown;

            // 🩸 Spawn blood at fish position
            SpawnBlood(other.transform.position);

            // 🦈 Eat fish
            Destroy(other.gameObject);
        }
    }

    void SpawnBlood(Vector3 position)
    {
        if (bloodPuffPrefab == null) return;

        GameObject blood = Instantiate(
            bloodPuffPrefab,
            position + bloodOffset,
            Quaternion.identity
        );

        Destroy(blood, bloodLifeTime);
    }
}