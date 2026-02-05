using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("Open Settings")]
    public KeyCode openKey = KeyCode.E;

    [Tooltip("How many keys must be in the bar to open. Set this to match KeyBarUI.keysToFull (example: 5).")]
    public int keysRequired = 5;

    [Header("Reward")]
    public Transform spawnPoint;        // optional: child SpawnPoint
    public GameObject rewardPrefab;     // AK / power-up prefab
    public Sprite openedSprite;         // optional opened chest sprite

    private bool playerInRange = false;
    private bool opened = false;

    private KeyBarUI keyBar;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Your keys are managed by a single KeyBarUI (like your KeyPickup does)
        keyBar = FindFirstObjectByType<KeyBarUI>();
    }

    void Update()
    {
        if (opened) return;

        if (playerInRange && Input.GetKeyDown(openKey))
        {
            if (keyBar != null && keyBar.IsFull())
            {
                // Spend keys (consume the full bar)
                bool spent = keyBar.SpendKeys(keysRequired);
                if (spent) OpenChest();
            }
            else
            {
                // Debug.Log("Bar not full yet.");
            }
        }
    }

    private void OpenChest()
    {
        opened = true;

        // Change sprite to opened (optional)
        if (openedSprite != null && sr != null)
            sr.sprite = openedSprite;

        // Spawn reward (optional)
        if (rewardPrefab != null)
        {
            Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;
            Instantiate(rewardPrefab, pos, Quaternion.identity);
        }

        // Disable collider so it can't be opened again
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
