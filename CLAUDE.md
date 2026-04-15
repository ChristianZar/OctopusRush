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

## Architecture

### Manager Singletons

All core systems are MonoBehaviour singletons accessed via `SystemName.Instance`:

| Manager | Responsibility |
|---|---|
| `GameManager` | Game state, game-over, continue system, respawn |
| `ScoreManager` | Distance tracking (meters), best/last score, PlayerPrefs |
| `AudioManager` | Music + SFX playback |
| `AchievementManager` | 23 achievements tracked against game events; fires `OnUnlocked` C# event |
| `PauseManager` | Pause/resume state |
| `SkinManager` | Player cosmetics loaded from `SkinData/` ScriptableObjects |

### Player

- `PlayerController` — physics movement (rise on hold, fall on release), ink dash ability, animation state machine
- `PlayerHealth` — 5 HP max, `TakeDamage()`, healing, death → calls `GameManager`
- `PlayerShooting` — fires AK47 prefab projectiles
- `ShieldSystem` — timed bubble shield with warning phase before expiry

### Enemy Spawners

All spawners use **distance-based difficulty ramping**: they read `ScoreManager.Instance.distanceTraveled` and interpolate spawn rate/spacing between easy and hard values over a configurable ramp window (typically 60 m → 250 m).

Key spawners: `SharkSpawner`, `MineSpawner`, `JellyfishSpawner`, `AnglerFishSpawner`, `PufferSpawner`, `FloorCrabSpawner`.

Enemies are instantiated from prefabs in `Assets/Prefabs/` and destroyed when off-screen. Screen positions are calculated via `Camera.ViewportToWorldPoint()`.

### Level / Scrolling

- `CameraAutoScroll` — moves the camera (and thus the world) forward continuously
- `FloorLooper` / `EndlessFloor` — infinite scrolling floor tiles
- `BackgroundScroller` — parallax layers

### Persistence

All saved state uses `PlayerPrefs`: achievement flags, best score, equipped skin index.

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

## File Locations

- `Assets/Scripts/` — all 87 C# scripts
- `Assets/Prefabs/` — 44 prefabs (enemies, pickups, UI elements)
- `Assets/SkinData/` — ScriptableObjects for player skins
- `Assets/Audio/` — BGM and SFX clips
- `Assets/Sprite/` — sprite sheets and individual sprites
- `Assets/Scenes/SampleScene.unity` — the single game scene (main menu + gameplay)
