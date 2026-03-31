using System;
using System.Collections.Generic;
using UnityEngine;

/// Singleton. Place on an empty GameObject in the scene.
/// All other scripts call AchievementManager.Instance?.ReportXxx().
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    /// Fired whenever a new achievement is unlocked.
    public static event Action<AchievementData> OnUnlocked;

    // ── Achievement Definitions ───────────────────────────────────────────────
    public static readonly List<AchievementData> All = new List<AchievementData>
    {
        // ── Distance ──────────────────────────────────────────────────────────
        new AchievementData { id="first_stroke",    icon="🏊", title="First Stroke",       description="Travel 100 m" },
        new AchievementData { id="deep_diver",      icon="🤿", title="Deep Diver",          description="Travel 500 m" },
        new AchievementData { id="ocean_explorer",  icon="🌊", title="Ocean Explorer",      description="Travel 1,000 m" },
        new AchievementData { id="midnight_zone",   icon="🌑", title="Midnight Zone",       description="Travel 2,500 m" },
        new AchievementData { id="into_the_abyss",  icon="🕳", title="Into the Abyss",      description="Travel 5,000 m",                        isSecret=true },
        // ── Food & Healing ────────────────────────────────────────────────────
        new AchievementData { id="first_bite",      icon="🐟", title="First Bite",          description="Eat your first fish" },
        new AchievementData { id="well_fed",        icon="🍽", title="Well Fed",             description="Eat 15 fish in one run" },
        new AchievementData { id="puffer_snack",    icon="🐡", title="Puffer Snack",        description="Eat a deflated pufferfish" },
        new AchievementData { id="comeback_kid",    icon="💚", title="Comeback Kid",        description="Heal back to full HP in one run" },
        // ── Combat ────────────────────────────────────────────────────────────
        new AchievementData { id="shark_slayer",    icon="🦈", title="Shark Slayer",        description="Kill your first shark" },
        new AchievementData { id="veteran",         icon="⚔",  title="Veteran",              description="Kill 10 sharks in total",               isSecret=true },
        new AchievementData { id="armed",           icon="🔫", title="Armed!",               description="Pick up the AK47" },
        new AchievementData { id="trigger_happy",   icon="🎯", title="Trigger Happy",       description="Fire 30 shots in one run",              isSecret=true },
        // ── Shield ────────────────────────────────────────────────────────────
        new AchievementData { id="force_field",     icon="🫧", title="Force Field",         description="Collect a shield orb" },
        new AchievementData { id="damage_blocked",  icon="⚡", title="Blocked!",             description="Block an attack with your shield",      isSecret=true },
        // ── Survival ──────────────────────────────────────────────────────────
        new AchievementData { id="untouchable",     icon="🛡", title="Untouchable",          description="Finish a run without taking damage" },
        new AchievementData { id="on_the_edge",     icon="❤",  title="On the Edge",          description="Die with exactly 1 HP remaining" },
        new AchievementData { id="second_chance",   icon="🔄", title="Second Chance",        description="Use a continue to keep going" },
        // ── Ink ───────────────────────────────────────────────────────────────
        new AchievementData { id="ink_storm",       icon="🦑", title="Ink Storm",            description="Release ink 10 times in one run" },
        // ── Keys & Chests ─────────────────────────────────────────────────────
        new AchievementData { id="key_hunter",      icon="🔑", title="Key Hunter",           description="Collect 20 keys in one run" },
        new AchievementData { id="treasure_seeker", icon="📦", title="Treasure Seeker",      description="Open 3 chests in one run" },
        // ── Mines ─────────────────────────────────────────────────────────────
        new AchievementData { id="chain_reaction",  icon="💥", title="Chain Reaction",       description="Trigger a mine chain explosion",        isSecret=true },
    };

    // ── PlayerPrefs keys ──────────────────────────────────────────────────────
    private const string PREF_UNLOCK = "ach_unlock_";
    private const string PREF_SHARKS = "ach_prog_sharks";

    // ── Per-run counters (reset each run) ─────────────────────────────────────
    private int  keysThisRun;
    private int  chestsThisRun;
    private int  inkUsesThisRun;
    private int  fishEatenThisRun;
    private int  shotsThisRun;
    private bool tookEnemyDamage;   // only counts real enemy hits, not drain
    private int  hpBeforeDeathBlow; // HP the player had before the final hit

    // ── Cumulative counters (saved to PlayerPrefs) ────────────────────────────
    private int totalSharksKilled;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        totalSharksKilled = PlayerPrefs.GetInt(PREF_SHARKS, 0);
        ResetRunCounters();
    }

    void ResetRunCounters()
    {
        keysThisRun       = 0;
        chestsThisRun     = 0;
        inkUsesThisRun    = 0;
        fishEatenThisRun  = 0;
        shotsThisRun      = 0;
        tookEnemyDamage   = false;
        hpBeforeDeathBlow = 0;
    }

    // ── Unlock ────────────────────────────────────────────────────────────────
    public void Unlock(string id)
    {
        if (IsUnlocked(id)) return;
        PlayerPrefs.SetInt(PREF_UNLOCK + id, 1);
        PlayerPrefs.Save();
        AchievementData data = All.Find(a => a.id == id);
        if (data != null) OnUnlocked?.Invoke(data);
    }

    public bool IsUnlocked(string id) =>
        PlayerPrefs.GetInt(PREF_UNLOCK + id, 0) == 1;

    // ── Report Methods (called from game scripts) ─────────────────────────────

    public void ReportDistance(float meters)
    {
        if (meters >= 100f)   Unlock("first_stroke");
        if (meters >= 500f)   Unlock("deep_diver");
        if (meters >= 1000f)  Unlock("ocean_explorer");
        if (meters >= 2500f)  Unlock("midnight_zone");
        if (meters >= 5000f)  Unlock("into_the_abyss");
    }

    public void ReportFishEaten()
    {
        fishEatenThisRun++;
        Unlock("first_bite");
        if (fishEatenThisRun >= 15) Unlock("well_fed");
    }

    public void ReportPufferfishEaten() => Unlock("puffer_snack");

    public void ReportShieldCollected() => Unlock("force_field");

    public void ReportDamageBlocked() => Unlock("damage_blocked");

    public void ReportWeaponPickedUp() => Unlock("armed");

    public void ReportShotFired()
    {
        shotsThisRun++;
        if (shotsThisRun >= 30) Unlock("trigger_happy");
    }

    public void ReportHealedToFull() => Unlock("comeback_kid");

    public void ReportKeyCollected()
    {
        keysThisRun++;
        if (keysThisRun >= 20) Unlock("key_hunter");
    }

    public void ReportChestOpened()
    {
        chestsThisRun++;
        if (chestsThisRun >= 3) Unlock("treasure_seeker");
    }

    /// Called each time an ink cloud is spawned (Space held).
    public void ReportInkUsed()
    {
        inkUsesThisRun++;
        if (inkUsesThisRun >= 10) Unlock("ink_storm");
    }

    /// Called when a mine triggers at least one linked mine.
    public void ReportChainExplosion() => Unlock("chain_reaction");

    /// Called from PlayerHealth.TakeDamage (NOT drain damage).
    public void ReportEnemyDamageTaken() => tookEnemyDamage = true;

    /// Called just before the player dies — pass the HP they had before the fatal hit.
    public void ReportDyingWithHP(int hpBeforeHit) => hpBeforeDeathBlow = hpBeforeHit;

    /// Called by GameManager when the run ends (PlayerDied).
    public void ReportRunEnded()
    {
        if (!tookEnemyDamage)   Unlock("untouchable");
        if (hpBeforeDeathBlow == 1) Unlock("on_the_edge");
        ResetRunCounters();
    }

    /// Called when the player uses a Continue.
    public void ReportContinueUsed()
    {
        Unlock("second_chance");
        tookEnemyDamage = true; // can't get Untouchable after continuing
    }

    /// Called from SharkHealth.Die().
    public void ReportSharkKilled()
    {
        Unlock("shark_slayer");
        totalSharksKilled++;
        PlayerPrefs.SetInt(PREF_SHARKS, totalSharksKilled);
        PlayerPrefs.Save();
        if (totalSharksKilled >= 10) Unlock("veteran");
    }

    // ── Debug helpers ─────────────────────────────────────────────────────────
    [ContextMenu("Test Popup (First Stroke)")]
    public void DebugTestPopup()
    {
        AchievementData data = All.Find(a => a.id == "first_stroke");
        if (data != null) OnUnlocked?.Invoke(data);
    }

    [ContextMenu("Reset All Achievements")]
    public void DebugResetAll()
    {
        foreach (var a in All) PlayerPrefs.DeleteKey(PREF_UNLOCK + a.id);
        PlayerPrefs.DeleteKey(PREF_SHARKS);
        PlayerPrefs.Save();
        Debug.Log("All achievements reset.");
    }
}
