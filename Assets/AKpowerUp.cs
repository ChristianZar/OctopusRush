using UnityEngine;

public class AKPowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerShooting shoot = other.GetComponent<PlayerShooting>();
        if (shoot != null)
        {
            shoot.GiveAK();
        }

        Destroy(gameObject); // remove pickup
    }
}
