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

    // ✅ 1 = right, -1 = left (based on flip)
    float dir = Mathf.Sign(transform.localScale.x);

    // ✅ spawn in front based on facing
    Vector3 spawnPos = firePoint.position + Vector3.right * dir * 0.25f;

    GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    Debug.Log("Spawned bullet: " + b.name + " at " + spawnPos);

    // ✅ Sound
    if (gunSound != null && gunSound.clip != null)
    {
        gunSound.pitch = Random.Range(0.9f, 1.1f);
        gunSound.PlayOneShot(gunSound.clip);
    }

    Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = Vector2.right * dir * bulletSpeed;
    }
    else
    {
        Debug.LogWarning("Bullet has NO Rigidbody2D");
    }
}
}
