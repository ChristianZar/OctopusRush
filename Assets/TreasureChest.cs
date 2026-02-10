using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("Open Settings")]
    public KeyCode openKey = KeyCode.E;

    [Header("Reward")]
    public Transform spawnPoint;
    public GameObject rewardPrefab;
    public Sprite openedSprite;

    [HideInInspector] public TreasureChestSpawner spawner;

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
        keyBar = FindFirstObjectByType<KeyBarUI>();
    }

    void Update()
    {
        if (opened) return;

        if (playerInRange && Input.GetKeyDown(openKey))
        {
            if (keyBar != null && keyBar.IsFull())
            {
                OpenChest();
            }
        }
    }

    private void OpenChest()
    {
        opened = true;

        if (openedSprite != null && sr != null)
            sr.sprite = openedSprite;

        if (rewardPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(rewardPrefab, pos, Quaternion.identity);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 🔥 THIS IS THE IMPORTANT LINE
        if (spawner != null)
            spawner.NotifyChestOpened();
        else if (keyBar != null)
            keyBar.ResetKeys();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
