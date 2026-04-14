---
description: Overview of core game systems to understand before editing related scripts
globs: ["Assets/Scripts/**/*.cs"]
---

Read this before editing any of the scripts listed below.

### Ink mechanic (`PlayerController`, `InkBarUI`, `CameraAutoScroll`, `InkCloud`)
- Space bar drains ink energy; ink recharges passively over time.
- While ink is active, `CameraAutoScroll` increases scroll speed via a boost multiplier blended with `Mathf.Lerp`.
- Ink use spawns `InkCloud` prefabs that apply a slow effect to enemies inside them.
- Editing ink drain/recharge rates will affect camera speed and enemy slow duration simultaneously.

### Continue system (`GameManager`, `PlayerHealth`)
- Each run grants one free respawn.
- Handled via `GameManager.ContinueRun()` — do not add respawn logic elsewhere.
- After the free continue is used, death goes straight to the game-over screen.

### Skin system (`SkinManager`, `SkinData` ScriptableObject)
- Each skin is a `SkinData` ScriptableObject with: key cost, sprite frames, animation clips.
- Players purchase skins with collected keys; ownership and equipped skin persist via `PlayerPrefs`.
- To add a skin: create a new `SkinData` asset, set its fields, register it in `SkinManager`'s skin list.

### Scoring (`ScoreManager`, `CameraAutoScroll`)
- Score is distance in meters derived from `CameraAutoScroll`'s scroll position.
- `ScoreManager` tracks current, last-run, and best-run distances and persists best/last via `PlayerPrefs`.
