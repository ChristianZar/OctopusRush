using UnityEngine;

/// <summary>
/// Singleton that owns skin unlock/equip state and lifetime key balance.
/// Persists across scenes via DontDestroyOnLoad.
/// </summary>
public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    // PlayerPrefs keys
    private const string LIFETIME_KEYS_KEY = "lifetime_keys";
    private const string EQUIPPED_SKIN_KEY  = "equipped_skin";
    private const string SKIN_OWNED_PREFIX  = "skin_owned_";

    [Header("All Skins (slot 0 = default, always owned)")]
    public SkinData[] skins;           // drag all 9 SkinData assets here in the Inspector

    public int LifetimeKeys { get; private set; }
    public int EquippedIndex { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LifetimeKeys  = PlayerPrefs.GetInt(LIFETIME_KEYS_KEY, 0);
        EquippedIndex = PlayerPrefs.GetInt(EQUIPPED_SKIN_KEY, 0);

        // Skin 0 is always owned
        if (skins != null && skins.Length > 0)
            SetOwned(0, true);
    }

    // ── Key currency ──────────────────────────────────────────────────────────

    public void AddLifetimeKeys(int amount)
    {
        LifetimeKeys += amount;
        PlayerPrefs.SetInt(LIFETIME_KEYS_KEY, LifetimeKeys);
        PlayerPrefs.Save();
    }

    // ── Ownership ─────────────────────────────────────────────────────────────

    public bool IsOwned(int index)
    {
        if (index == 0) return true;
        return PlayerPrefs.GetInt(SKIN_OWNED_PREFIX + index, 0) == 1;
    }

    private void SetOwned(int index, bool owned)
    {
        PlayerPrefs.SetInt(SKIN_OWNED_PREFIX + index, owned ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>Returns true if the purchase succeeded.</summary>
    public bool TryPurchase(int index)
    {
        if (skins == null || index < 0 || index >= skins.Length) return false;
        if (IsOwned(index)) return false;

        int cost = skins[index].keyCost;
        if (LifetimeKeys < cost) return false;

        LifetimeKeys -= cost;
        PlayerPrefs.SetInt(LIFETIME_KEYS_KEY, LifetimeKeys);
        SetOwned(index, true);
        return true;
    }

    // ── Equip ─────────────────────────────────────────────────────────────────

    public void Equip(int index)
    {
        if (!IsOwned(index)) return;
        EquippedIndex = index;
        PlayerPrefs.SetInt(EQUIPPED_SKIN_KEY, index);
        PlayerPrefs.Save();
    }

    /// <summary>Returns the currently equipped SkinData, or null if not set up.</summary>
    public SkinData GetEquippedSkin()
    {
        if (skins == null || skins.Length == 0) return null;
        int idx = Mathf.Clamp(EquippedIndex, 0, skins.Length - 1);
        return skins[idx];
    }
}
