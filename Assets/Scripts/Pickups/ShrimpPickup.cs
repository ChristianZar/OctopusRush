using UnityEngine;

public class ShrimpPickup : MonoBehaviour
{
    [Header("Eat FX")]
    public GameObject bloodPuffPrefab;
    public Vector3 bloodOffset = Vector3.zero;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null) return;
        if (ph.IsDead()) return;

        // FX
        if (bloodPuffPrefab != null)
            Instantiate(bloodPuffPrefab, transform.position + bloodOffset, Quaternion.identity);

        // heal player
        ph.OnFishEaten();
        AchievementManager.Instance?.ReportFishEaten();

        Destroy(gameObject);
    }
}