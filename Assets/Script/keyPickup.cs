using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public int keyValue = 1;
    private KeyBarUI keyBar;

    void Start()
    {
        keyBar = FindFirstObjectByType<KeyBarUI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (keyBar != null)
            keyBar.AddKey(keyValue);

        Destroy(gameObject);
    }
}
