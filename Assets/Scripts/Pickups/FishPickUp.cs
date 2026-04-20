using UnityEngine;

public class FishPickup : MonoBehaviour
{
    [Header("Eat FX")]
    public GameObject bloodPuffPrefab;   // drag your blood prefab here
    public Vector3 bloodOffset = Vector3.zero;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null) return;
        if (ph.IsDead()) return;

        // spawn blood puff
        if (bloodPuffPrefab != null)
            Instantiate(bloodPuffPrefab, transform.position + bloodOffset, Quaternion.identity);

        // heal system: +1 HP every fishPerHeal fish
        ph.OnFishEaten();
        AchievementManager.Instance?.ReportFishEaten();

        Destroy(gameObject);
    }
}