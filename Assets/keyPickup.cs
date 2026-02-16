using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    public int keyValue = 1;

    [Header("Audio")]
    public AudioClip pickupSfx;
    [Range(0f, 1f)] public float volume = 0.8f;

    private KeyBarUI keyBar;

    void Start()
    {
        keyBar = FindFirstObjectByType<KeyBarUI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Add key to UI
        if (keyBar != null)
            keyBar.AddKey(keyValue);

        // 🔊 Play pickup sound with slight random pitch
        if (pickupSfx != null)
        {
            GameObject tempAudio = new GameObject("TempKeyAudio");
            AudioSource audioSource = tempAudio.AddComponent<AudioSource>();

            audioSource.clip = pickupSfx;
            audioSource.volume = volume;
            audioSource.pitch = Random.Range(0.9f, 1.1f); // slight variation
            audioSource.spatialBlend = 0f; // 2D sound (important!)
            audioSource.Play();

            Destroy(tempAudio, pickupSfx.length);
        }

        Destroy(gameObject);
    }
}