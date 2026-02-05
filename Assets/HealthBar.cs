using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    public PlayerHealth playerHealth;

    void Update()
    {
        if (playerHealth == null) return;

        fillImage.fillAmount =
            (float)playerHealth.currentHealth / playerHealth.maxHealth;
    }
}
