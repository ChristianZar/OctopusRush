# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Octopush Rush** — a Jetpack Joyride-style endless runner with an ocean theme. The player controls an octopus swimming forward automatically, dodging/fighting enemies (sharks, mines, jellyfish, anglerfish, crabs, pufferfish), collecting pickups, and surviving as long as possible. Written in C# for Unity 2D.

## Building

This is a standard Unity project (no custom build scripts). Open in the Unity Editor and use `File > Build Settings` to build, or use Unity's command-line build:

```
/path/to/Unity -batchmode -quit -projectPath . -buildTarget StandaloneOSX -executeMethod BuildScript.Build
```

There are no automated test suites — testing is done by running the game in Play Mode in the Unity Editor.

## Project Layout

```
Assets/
  ├── (root)         — all 87 C# scripts live directly here (flat, no subdirectory)
  ├── Prefabs/       — pre-configured GameObjects (enemies, weapons, environment)
  ├── Sprite/        — PNG art assets
  ├── Audio/         — SFX and music
  ├── Scenes/        — SampleScene.unity (main menu + gameplay in one scene)
  ├── SkinData/      — ScriptableObject skin definitions
  └── Settings/      — Rendering pipeline settings
```

## Naming Conventions

| Target | Convention | Example |
|---|---|---|
| Classes | PascalCase | `PlayerController`, `SharkAI` |
| Public fields | camelCase | `speed`, `maxHealth` |
| Private fields | camelCase | `drainTimer`, `_rb` |
| Methods | PascalCase | `TakeDamage()`, `SpawnInkCloud()` |
| Constants / PlayerPrefs keys | UPPER_SNAKE_CASE | `BEST_KEY`, `PREF_UNLOCK` |

**Known intentional typo to preserve:** `Camara` (not `Camera`) appears in existing class names (`CamaraAutoScroll`, `CamaraShake`, `FitToCamara`). Do **not** rename these — Unity scene references will break.

## Code Style

- **Inspector-first design:** expose fields as `public` or `[SerializeField] private`. Group them with `[Header("Section Name")]`.
- **Arrow getters** for trivial accessors: `public float GetInk() => currentInk;`
- **Null-conditional operators** (`?.`) for optional component lookups.
- **No XML doc comments.** Use clear method/variable names; add inline comments only for non-obvious logic.
- **Section dividers** in longer files: `// ── Section Name ──`
- `Time.deltaTime` for all frame-rate-dependent values.
- `Mathf.Lerp()` for smooth transitions (speed blending, UI animations).

### File structure order
```
using statements
class declaration
  [Header] groups of serialized fields
  Awake / OnEnable
  Start
  Update / FixedUpdate
  Core logic methods
  Public getters / helpers
```

## Architecture

### Manager Singletons

All core systems are MonoBehaviour singletons accessed via `SystemName.Instance`. Never skip the duplication check:

```csharp
public static ManagerName Instance { get; private set; }

void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

| Manager | Responsibility |
|---|---|
| `GameManager` | Game state, game-over, continue system, respawn |
| `ScoreManager` | Distance tracking (meters), best/last score, PlayerPrefs |
| `AudioManager` | Music + SFX playback |
| `AchievementManager` | 23 achievements tracked against game events; fires `OnUnlocked` C# event |
| `PauseManager` | Pause/resume state, HUD pause button |
| `SkinManager` | Player cosmetics loaded from `SkinData/` ScriptableObjects |

### Player

- `PlayerController` — physics movement (rise on hold, fall on release), ink dash ability, animation state machine
- `PlayerHealth` — 5 HP max, `TakeDamage()`, healing, death → calls `GameManager`
- `PlayerShooting` — fires AK47 prefab projectiles
- `ShieldSystem` — timed bubble shield with warning phase before expiry

### Enemy Spawners

All spawners use **distance-based difficulty ramping**: they read `ScoreManager.Instance.distanceTraveled` and interpolate spawn rate/spacing between easy and hard values over a configurable ramp window (typically 60 m → 250 m). Difficulty values are set via Inspector `[Header]` fields — don't hardcode them in code.

Key spawners: `SharkSpawner`, `MineSpawner`, `JellyfishSpawner`, `AnglerFishSpawner`, `PufferSpawner`, `FloorCrabSpawner`.

Enemy AI uses an `enum` state + `switch` in `Update`/`FixedUpdate`:
```csharp
enum State { Patrol, Chase, ReturnHome }
void FixedUpdate() {
    switch (_state) {
        case State.Patrol:      Patrol();      break;
        case State.Chase:       Chase();       break;
        case State.ReturnHome:  ReturnHome();  break;
    }
}
```

### Event System

UI and cross-system notifications use `System.Action` delegates. Subscribe in `OnEnable`, unsubscribe in `OnDisable`:
```csharp
void OnEnable()  => HealthSystem.Instance.OnHealthChanged += UpdateBar;
void OnDisable() => HealthSystem.Instance.OnHealthChanged -= UpdateBar;
```

Use coroutines (not `Invoke`) for delayed sequences, fade-ins, and animation pauses.

### Key Data Flow

```
Player input → PlayerController → CameraAutoScroll (distance++)
                                         ↓
                               ScoreManager.distanceTraveled
                                    ↙          ↘
                        Spawners (ramp)    AchievementManager (thresholds)
                              ↓
                    Enemy hits PlayerHealth
                              ↓
                    ShieldSystem or GameManager.GameOver()
```

## Persistence (PlayerPrefs)

All save data lives in `PlayerPrefs`:

| Category | Keys |
|---|---|
| Scores | `BEST_KEY`, `LAST_KEY` (via `ScoreManager`) |
| Achievements | `PREF_UNLOCK + achievementId` (via `AchievementManager`) |
| Skin shop | skin ownership flags, equipped skin id (via `SkinManager`) |
| Currency | lifetime key count |

`AchievementManager` tracks **per-run** counters (reset on death) separately from **cumulative** lifetime stats. Keep this separation when adding new achievements.

## Key Systems to Understand Before Editing

- **Ink mechanic** (`PlayerController`): drains on Space, recharges over time, boosts camera scroll speed via `CamaraAutoScroll`, spawns `InkCloud` prefabs that slow enemies.
- **Continue system** (`GameManager`, `PlayerHealth`): one free respawn per run; handled in `GameManager.ContinueRun()`.
- **Skin system** (`SkinManager`, `SkinData` ScriptableObject): each skin has key cost, sprite frames, and animation clips. Skins are purchased with collected keys and persist across sessions.
- **Scoring** (`ScoreManager`): distance in meters, updated from `CamaraAutoScroll` scroll position.
- **Chain-reaction mines** (`MineBehavior`): use Physics2D overlap checks — changes to mine radius affect all mine clusters.
