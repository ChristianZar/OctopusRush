using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public WeaponType currentWeapon = WeaponType.None;

    [Header("Optional visuals")]
    public GameObject ak47Visual;

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

    }
}
