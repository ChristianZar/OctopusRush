using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip bgMusic;

    [Header("Player SFX")]
    public AudioClip playerDamageClip;
    public AudioClip playerDeathClip;
    public AudioClip inkFireClip;

    [Header("Eat / Heal SFX")]
    public AudioClip fishEatClip;
    public AudioClip healClip;

    [Header("Enemy SFX")]
    public AudioClip mineExplodeClip;
    public AudioClip sharkBiteClip;
    public AudioClip jellyfishZapClip;

    [Header("Item / UI SFX")]
    public AudioClip shieldPickupClip;
    public AudioClip chestOpenClip;
    public AudioClip keyPickupClip;

    [Header("SFX Volume")]
    [Range(0f, 2f)] public float sfxVolume = 1f;

    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 1f;
    }

    void Start()
    {
        if (musicSource != null && bgMusic != null)
        {
            musicSource.clip = bgMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayPlayerDamage()
    {
        if (playerDamageClip == null || sfxSource == null) return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(playerDamageClip, 1f);
    }


    public void PlayPlayerDeath() => Play(playerDeathClip);

    public void PlayInkFire()
    {
        if (inkFireClip == null || sfxSource == null) return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(inkFireClip, 1.5f);
    }

    public void PlayFishEat()
    {
        if (fishEatClip == null || sfxSource == null) return;
        StartCoroutine(PlayShortClip(fishEatClip, 0.2f));
    }

    public void PlayHeal() => Play(healClip);
    public void PlayMineExplode() => Play(mineExplodeClip);
    public void PlaySharkBite() => Play(sharkBiteClip);
    public void PlayJellyfishZap() => Play(jellyfishZapClip);
    public void PlayShieldPickup() => Play(shieldPickupClip);
    public void PlayChestOpen()
    {
        if (chestOpenClip == null || sfxSource == null) return;

        sfxSource.Stop();

        sfxSource.clip = chestOpenClip;
        sfxSource.time = 0.5f; // ⏩ skip first 0.5 seconds (change if needed)

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.volume = 1f;

        sfxSource.PlayOneShot(chestOpenClip, 3f); // 🔊 louder
    }
    public void PlayKeyPickup() => Play(keyPickupClip);

    void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    IEnumerator PlayShortClip(AudioClip clip, float duration)
    {
        if (clip == null || sfxSource == null) yield break;

        sfxSource.pitch = Random.Range(0.95f, 1.1f);
        sfxSource.PlayOneShot(clip, 4.5f);

        yield return new WaitForSeconds(duration);

        sfxSource.Stop();
    }

    IEnumerator StopAfter(float time)
    {
        yield return new WaitForSeconds(time);

        if (sfxSource != null)
            sfxSource.Stop();
    }
    public void StopAllAudio()
    {
        if (musicSource != null)
            musicSource.Stop();

        if (sfxSource != null)
            sfxSource.Stop();
    }
}