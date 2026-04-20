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
    }

    void Update()
    {
        if (health != null && health.IsDead()) return;

        if (weapon == null) return;
        if (weapon.currentWeapon != WeaponType.AK47) return;

        if (Input.GetKey(KeyCode.F) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

void Shoot()
{
    if (bulletPrefab == null || firePoint == null) return;

    var controller = GetComponent<PlayerController>();
    float dir = (controller != null && !controller.IsFacingRight()) ? -1f : 1f;
    Vector3 spawnPos = firePoint.position + Vector3.right * dir * 0.25f;

    AchievementManager.Instance?.ReportShotFired();
    GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

    if (gunSound != null && gunSound.clip != null)
    {
        gunSound.pitch = Random.Range(0.9f, 1.1f);
        gunSound.PlayOneShot(gunSound.clip);
    }

    Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
    if (rb != null)
        rb.linearVelocity = Vector2.right * dir * bulletSpeed;
}
}