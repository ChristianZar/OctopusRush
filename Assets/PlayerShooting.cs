using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 12f;
    public float fireCooldown = 0.12f;

    private float nextFireTime;
    private PlayerWeapon weapon;

    void Start()
    {
        weapon = GetComponent<PlayerWeapon>();
        Debug.Log("PlayerShooting START. weapon found = " + (weapon != null));
        Debug.Log("bulletPrefab assigned = " + (bulletPrefab != null) + ", firePoint assigned = " + (firePoint != null));
    }

    void Update()
    {
        // Prove input is being detected
        if (Input.GetKeyDown(KeyCode.F))
            Debug.Log("F pressed (Update running)");

        // Require AK equipped
        if (weapon == null) return;
        if (weapon.currentWeapon != WeaponType.AK47) return;

        if (Input.GetKey(KeyCode.F) && Time.time >= nextFireTime)
        {
            Debug.Log("Calling Shoot()");
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Shoot blocked: bulletPrefab or firePoint is NULL");
            return;
        }

        // Spawn a tiny bit in front so it doesn't start inside the player collider
        Vector3 spawnPos = firePoint.position + firePoint.right * 0.25f;

        GameObject b = Instantiate(bulletPrefab, spawnPos, firePoint.rotation);
        Debug.Log("Spawned bullet: " + b.name + " at " + spawnPos);

        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = (Vector2)firePoint.right * bulletSpeed; // use velocity (reliable)
        }
        else
        {
            Debug.LogWarning("Bullet has NO Rigidbody2D");
        }
    }
}
