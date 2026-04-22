using UnityEngine;
using TMPro;
using System.Collections;

public class TreasureChest : MonoBehaviour
{
    [Header("Open Settings")]
    public KeyCode openKey = KeyCode.E;

    [Header("Reward")]
    public Transform spawnPoint;
    public GameObject rewardPrefab;
    public Sprite openedSprite;

    [Header("Press E Prompt")]
    public GameObject pressEPrompt;

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
        ShowPrompt(false);
    }

    void Update()
    {
        if (opened) return;

        if (playerInRange && (Input.GetKeyDown(openKey) || (MobileInputBridge.Instance?.InteractDown ?? false)))
        {
            if (keyBar != null && keyBar.IsFull())
            {
                OpenChest();
            }
        }
    }

    void ShowPrompt(bool show)
    {
        if (pressEPrompt != null)
            pressEPrompt.SetActive(show);
    }

    private void OpenChest()
    {
        opened = true;
        ShowPrompt(false);

        AudioManager.Instance?.PlayChestOpen();
        StartCoroutine(StopAudioAfterDelay(3f));

        AchievementManager.Instance?.ReportChestOpened();

        if (openedSprite != null && sr != null)
            sr.sprite = openedSprite;

        if (rewardPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(rewardPrefab, pos, Quaternion.identity);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (spawner != null)
            spawner.NotifyChestOpened();
        else if (keyBar != null)
            keyBar.ResetKeys();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!opened) ShowPrompt(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowPrompt(false);
        }
    }

    private void OnDestroy()
    {
        if (!opened && spawner != null)
        {
            spawner.NotifyChestDespawned(this);
        }
    }

    private void OnDisable()
    {
        if (!opened && spawner != null)
        {
            spawner.NotifyChestDespawned(this);
        }
    }

    IEnumerator StopAudioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance?.StopAllAudio();
    }
}