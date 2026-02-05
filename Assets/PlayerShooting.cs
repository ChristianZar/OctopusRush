using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Weapon")]
    public bool hasAK = false;

    [Header("Bullet")]
    public GameObject bulletPrefab;     // drag Bullet prefab here
    public Transform firePoint;         // drag FirePoint here
    public float bulletSpeed = 12f;

    [Header("Fire Rate")]
    public float fireCooldown = 0.12f;  // how fast AK shoots
    private float nextFireTime = 0f;

    void Update()
    {
        if (!hasAK) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    public void GiveAK()
    {
        hasAK = true;
        // Debug.Log("AK unlocked!");
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.right * bulletSpeed; 
            // if your game scrolls right, bullets go right.
            // If you want bullets to go toward mouse, tell me and I'll update it.
        }
    }
}
