using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 12f;
    public float fireCooldown = 0.12f;

       [Header("Audio")]                 
    public AudioSource gunSound;      

    private float nextFireTime;
    private PlayerWeapon weapon;
      private PlayerHealth health;

    void Start()
    {
        weapon = GetComponent<PlayerWeapon>();
        health = GetComponent<PlayerHealth>(); 
        Debug.Log("PlayerShooting START. weapon found = " + (weapon != null));
        Debug.Log("bulletPrefab assigned = " + (bulletPrefab != null) + ", firePoint assigned = " + (firePoint != null));
    }

    void Update()
    {
         // ✅ STOP shooting if dead
        if (health != null && health.IsDead()) return;

        if (weapon == null) return;
        if (weapon.currentWeapon != WeaponType.AK47) return;

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

          // ✅ PLAY SOUND HERE (after spawning is fine)
       // ✅ Sound
if (gunSound != null && gunSound.clip != null)
{
    gunSound.pitch = Random.Range(0.9f, 1.1f);
    gunSound.PlayOneShot(gunSound.clip);
}

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
