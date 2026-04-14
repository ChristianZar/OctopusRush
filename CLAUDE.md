# OctopusRush — CLAUDE.md

Vertical auto-scrolling arcade survival game built in Unity (C#). The player controls an octopus navigating an endless underwater environment, fighting enemies and collecting keys to unlock cosmetic skins.

## Project Layout

```
Assets/
  ├── Scripts/       — all 86+ C# scripts (flat, no subdirectories)
  ├── Prefabs/       — pre-configured GameObjects (enemies, weapons, environment)
  ├── Sprite/        — PNG art assets
  ├── Audio/         — SFX and music
  ├── Scenes/        — MainMenu.unity, MainScene.unity
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

**Known intentional typo to preserve:** `Camara` (not `Camera`) appears in existing class names (`CamaraAutoScroll`, `CamaraFollow2D`, `CamaraShake`, `FitToCamara`). Do **not** rename these — Unity scene references will break.

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

## Architecture Patterns

### Singleton Managers
Persistent managers use the standard Unity singleton guard. Never skip the duplication check:
```csharp
public static ManagerName Instance { get; private set; }

void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```
Current managers: `GameManager`, `ScoreManager`, `AchievementManager`, `SkinManager`, `AudioManager`, `PauseManager`.

### AI State Machines
Enemy AI uses an `enum` state + `switch` in `Update`/`FixedUpdate`. Example pattern from `SharkAI`:
```csharp
enum State { Patrol, Chase, ReturnHome }
State _state;

void FixedUpdate() {
    switch (_state) {
        case State.Patrol:   Patrol();   break;
        case State.Chase:    Chase();    break;
        case State.ReturnHome: ReturnHome(); break;
    }
}
```

### Event System
UI and cross-system notifications use `System.Action` delegates. Subscribe in `OnEnable`, unsubscribe in `OnDisable`:
```csharp
// Publisher
public event Action<int> OnHealthChanged;

// Subscriber
void OnEnable()  => HealthSystem.Instance.OnHealthChanged += UpdateBar;
void OnDisable() => HealthSystem.Instance.OnHealthChanged -= UpdateBar;
```

### Coroutines for Timing
Use coroutines (not `Invoke`) for delayed sequences, fade-ins, and animation pauses.

## Persistence (PlayerPrefs)

All save data lives in `PlayerPrefs`. Key categories:

| Category | Keys (examples) |
|---|---|
| Scores | `BEST_KEY`, `LAST_KEY` (via `ScoreManager`) |
| Achievements | `PREF_UNLOCK + achievementId` (via `AchievementManager`) |
| Skin shop | skin ownership flags, equipped skin id (via `SkinManager`) |
| Currency | lifetime key count |

`AchievementManager` tracks **per-run** counters (reset on death) separately from **cumulative** lifetime stats. Keep this separation when adding new achievements.

## Key Systems to Understand Before Editing

- **Ink mechanic** (`PlayerController`): drains on Space, recharges over time, boosts camera scroll speed via `CameraAutoScroll`, spawns `InkCloud` prefabs that slow enemies.
- **Continue system** (`GameManager`, `PlayerHealth`): one free respawn per run; handled in `GameManager.ContinueRun()`.
- **Skin system** (`SkinManager`, `SkinData` ScriptableObject): each skin has key cost, sprite frames, and animation clips. Skins are purchased with collected keys and persist across sessions.
- **Scoring** (`ScoreManager`): distance in meters, updated from `CameraAutoScroll` scroll position.

## Common Pitfalls

- `FindObjectOfType<T>()` calls can return `null` if a manager isn't in the scene — use null-conditional `?.` or guard with `if (X == null) return;`.
- The `SharkAI` filename is `SharkAi.cs` (lowercase `i`) — match this when creating related files.
- Spawner scripts set difficulty via serialized `[Header]` fields in the Inspector; don't hardcode wave values in code.
- Chain-reaction mine explosions (`MineBehavior`) use Physics2D overlap checks — changes to mine radius affect all mine clusters.
