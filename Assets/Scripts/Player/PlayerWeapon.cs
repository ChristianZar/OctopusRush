using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public WeaponType currentWeapon = WeaponType.None;

    [Header("Optional visuals")]
    public GameObject ak47Visual;

    [Header("Powerup Duration")]
public float weaponDuration = 15f; // seconds

    void Start()
    {
        if (ak47Visual != null)
            ak47Visual.SetActive(false);

        currentWeapon = WeaponType.None;
    }

    public void Equip(WeaponType weapon)
{
    currentWeapon = weapon;

    if (ak47Visual != null)
        ak47Visual.SetActive(currentWeapon == WeaponType.AK47);

    // Start timer
    StopAllCoroutines();
    StartCoroutine(WeaponTimer());
}

private System.Collections.IEnumerator WeaponTimer()
{
    yield return new WaitForSeconds(weaponDuration);

    // Remove weapon
    currentWeapon = WeaponType.None;

    if (ak47Visual != null)
        ak47Visual.SetActive(false);
}
}
