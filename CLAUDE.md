# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**OctopusRush** — a Jetpack Joyride-style endless runner with an ocean theme. The player controls an octopus swimming forward automatically, dodging/fighting enemies (sharks, mines, jellyfish, anglerfish, crabs, pufferfish), collecting pickups, and surviving as long as possible. Written in C# for Unity 2D.

## Building

Standard Unity project — no custom build scripts. Open in the Unity Editor and use `File > Build Settings`. No automated test suites — testing is done in Play Mode.

## Project Layout

```
Assets/
  ├── Scripts/
  │     ├── Managers/    — GameManager, ScoreManager, AudioManager, AchievementManager, PauseManager, SkinManager, SkinData
  │     ├── Player/      — PlayerController, PlayerHealth, PlayerShooting, PlayerWeapon, ShieldSystem, Ink scripts
  │     ├── Enemies/     — SharkAi, CrabPatrol, JellyFishWander2D, AnglerFishAmbush, PufferFishBehavior, ShrimpBehavior, MineBehavior, etc.
  │     ├── Spawners/    — all *Spawner.cs files
  │     ├── Pickups/     — FishPickUp, BubblePowerUp, ShieldOrbPowerUp, TreasureChest, keyPickup, ShrimpPickup
  │     ├── UI/          — MainMenuManager, MainMenuStyler, AchievementPanel, SkinShopPanel, HealthBar, InkBarUI, etc.
  │     ├── Environment/ — CamaraAutoScroll, CamaraFollow2D, CamaraShake, BackgroundScroller, FloorLooper, etc.
  │     ├── Weapons/     — Bullet, WeaponType, WeaponVisualFollow
  │     ├── Data/        — AchievementData
  │     └── Misc/        — BloodPuff, DamageFX
  ├── Animations/    — all .anim and .controller files
  ├── Materials/     — all .mat files
  ├── Prefabs/       — pre-configured GameObjects (enemies, weapons, environment)
  ├── Sprite/        — PNG art assets (includes *_Card.png How-To-Play illustrations)
  ├── Audio/         — SFX and music clips
  ├── MainMenu.unity — main menu scene (root level)
  ├── MainScene.unity — gameplay scene (root level)
  ├── SkinData/      — SkinData ScriptableObject assets (one per skin)
  └── Settings/      — Rendering pipeline settings + InputSystem_Actions
```

## Architecture

### Manager Singletons

| Manager | Responsibility |
|---|---|
| `GameManager` | Game state, game-over, continue system, respawn |
| `ScoreManager` | Distance tracking (meters), best/last score, PlayerPrefs |
| `AudioManager` | Music + SFX via named methods (`PlaySharkBite()` etc.) — no generic clip API |
| `AchievementManager` | 23 achievements tracked against game events; fires `OnUnlocked` C# event |
| `PauseManager` | Pause/resume state, HUD pause button |
| `SkinManager` | Player cosmetics loaded from `SkinData/` ScriptableObjects |

### Player

- `PlayerController` — physics movement (rise on hold, fall on release), ink dash, animation state machine
- `PlayerHealth` — 5 HP max, `TakeDamage()`, healing (`OnFishEaten()` — every 3 fish = 1 HP), death → `GameManager.GameOver()`
- `PlayerShooting` — fires AK47 prefab projectiles on `[F]`
- `ShieldSystem` — timed bubble shield with warning phase before expiry

### Menu System

The main menu (`MainMenu.unity`) builds all its UI procedurally at runtime — no canvas prefabs.

- `MainMenuManager` — panel navigation using `CanvasGroup` alpha fades. Coroutines always run on the manager (never on panels), so `SetActive(false)` can't kill them mid-animation. Panels: `startMenuPanel`, `helpPanel`, `creditsPanel`, plus `AchievementPanel` / `SkinShopPanel` overlays.
- `MainMenuStyler` — builds the How-To-Play carousel and repositions the button grid in `Start()`. Carousel state (`_cardIndex`, image/label refs) lives as private fields. Requires 8 `*_Card.png` sprites and **Showpop SDF** font assigned in the Inspector.
- `MenuButtonFeedback` — attached automatically by `MainMenuStyler`; pointer handlers for 1.08× hover / 0.95× press scale feedback.
- `AchievementPanel` / `SkinShopPanel` — procedurally built scrollable grids; expose `Show()`, `Hide()` and an `onClose` callback wired back to `MainMenuManager`.
- `AchievementPopup` — toast queue for unlock notifications; three-phase slide animation using `Time.unscaledDeltaTime`.

### Enemy Spawners

All spawners use **distance-based difficulty ramping** — read `ScoreManager.Instance.distanceTraveled` and interpolate spawn rate/spacing between easy and hard values. All difficulty values live in Inspector `[Header]` fields — never hardcode them.

Key spawners: `SharkSpawner`, `MineSpawner`, `JellyfishSpawner`, `AnglerFishSpawner`, `PufferSpawner`, `FloorCrabSpawner`.

### Pickup & Powerup Patterns

- **Fish (`FishPickup`)** — `PlayerHealth.OnFishEaten()`; every 3 fish = 1 HP.
- **Shield orb (`ShieldOrbPowerUp`)** — sets `Time.timeScale = 0` briefly for a freeze-frame, then calls `ShieldSystem.ActivateShield()`.
- **Gun (`BubblePowerup`)** — 2-second collider-disabled delay before becoming collectible (entry animation window).
- **Treasure chest (`TreasureChest` ↔ `TreasureChestSpawner`)** — chest calls `TreasureChestSpawner.NotifyChestOpened()` on collect, which resets the key bar and blocks new spawns until keys refill.

### Key Data Flow

```
Player input → PlayerController → CamaraAutoScroll (scrolls world forward)
                                         ↓
                               ScoreManager.distanceTraveled
                                    ↙          ↘
                        Spawners (ramp)    AchievementManager (thresholds)
                              ↓
                    Enemy hits PlayerHealth
                              ↓
                    ShieldSystem absorbs  —or—  GameManager.GameOver()
```

## Persistence (PlayerPrefs)

Do not introduce a separate save system. Actual string keys used at runtime:

| Category | Keys |
|---|---|
| Scores | `"BEST_SCORE"`, `"LAST_SCORE"` |
| Achievements | `"PREF_UNLOCK" + achievementId` |
| Skin ownership / equipped skin / currency | managed by `SkinManager` |

## Key Systems to Understand Before Editing

- **Ink mechanic** (`PlayerController`): Space drains ink, recharges over time, boosts `CamaraAutoScroll` speed, spawns `InkCloud` prefabs that slow enemies.
- **Continue system** (`GameManager`): one free respawn per run via `GameManager.ContinueRun()` — do not add respawn logic elsewhere.
- **Skin system** (`SkinManager`, `SkinData` ScriptableObject): to add a skin, create a new `SkinData` asset and register it in `SkinManager`'s list.
- **Chain-reaction mines** (`MineBehavior`): `Physics2D.OverlapCircleAll()` triggers adjacent mines with staggered delays. Changing a mine's collider or explosion radius affects every cluster in the scene.
- **Health bar** (`HealthBar`): subscribes to `PlayerHealth.OnHealthChanged`; green→yellow→red gradient + white flash coroutine on damage.
