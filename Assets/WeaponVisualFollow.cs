using UnityEngine;

public class WeaponVisualFollow : MonoBehaviour
{
    [Header("References")]
    public Transform weaponVisual;            // drag AK47Visual here
    public SpriteRenderer playerSprite;       // drag Player's SpriteRenderer here (the pink octopus)

    [Header("Offsets")]
    public Vector3 rightOffset = new Vector3(1f, -0.19f, 0f);
    public Vector3 leftOffset  = new Vector3(-1f, -0.19f, 0f);

    void LateUpdate()
    {
        if (weaponVisual == null || playerSprite == null) return;

        bool facingLeft = playerSprite.flipX;

        // Move weapon to correct side
        weaponVisual.localPosition = facingLeft ? leftOffset : rightOffset;

        // Flip weapon so it faces the same direction
        Vector3 s = weaponVisual.localScale;
        weaponVisual.localScale = facingLeft ? new Vector3(-Mathf.Abs(s.x), s.y, s.z)
                                             : new Vector3(Mathf.Abs(s.x), s.y, s.z);
    }
}
