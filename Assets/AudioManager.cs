using UnityEngine;

/// Singleton audio manager. Place on a persistent GameObject in the game scene.
/// Assign AudioClips in the Inspector, then call AudioManager.Instance.PlayX() from anywhere.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip   bgMusic;

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
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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

    // ── Public helpers ────────────────────────────────────────────────────────

    public void PlayPlayerDamage()  => Play(playerDamageClip);
    public void PlayPlayerDeath()   => Play(playerDeathClip);
    public void PlayInkFire()       => Play(inkFireClip);
    public void PlayFishEat()       => Play(fishEatClip);
    public void PlayHeal()          => Play(healClip);
    public void PlayMineExplode()   => Play(mineExplodeClip);
    public void PlaySharkBite()     => Play(sharkBiteClip);
    public void PlayJellyfishZap()  => Play(jellyfishZapClip);
    public void PlayShieldPickup()  => Play(shieldPickupClip);
    public void PlayChestOpen()     => Play(chestOpenClip);
    public void PlayKeyPickup()     => Play(keyPickupClip);

    void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
