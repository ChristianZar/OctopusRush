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

    public void PlayPlayerDamage() => Play(playerDamageClip);
    public void PlayPlayerDeath() => Play(playerDeathClip);
    public void PlayInkFire() => Play(inkFireClip);

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
    public void PlayChestOpen() => Play(chestOpenClip);
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
        sfxSource.PlayOneShot(clip, sfxVolume);

        yield return new WaitForSeconds(duration);

        sfxSource.Stop();
    }
}