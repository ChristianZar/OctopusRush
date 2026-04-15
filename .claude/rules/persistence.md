---
description: PlayerPrefs save data layout and achievement counter rules
globs: ["Assets/Scripts/**/*.cs"]
---

All save data uses `PlayerPrefs`. Do not introduce a separate save system.

| Category | Owner | Key pattern |
|---|---|---|
| High score / last score | `ScoreManager` | `BEST_KEY`, `LAST_KEY` |
| Achievement unlocks | `AchievementManager` | `PREF_UNLOCK + achievementId` |
| Skin ownership | `SkinManager` | per-skin ownership flags |
| Equipped skin | `SkinManager` | equipped skin id key |
| Currency (keys) | `SkinManager` | lifetime key count |

### Achievement counters

`AchievementManager` maintains two distinct sets of counters:

- **Per-run counters** — reset to 0 on death (e.g., enemies killed this run).
- **Cumulative lifetime stats** — never reset (e.g., total enemies killed ever).

When adding a new achievement, decide which type it tracks and place it in the correct counter group. Do not mix per-run and cumulative values.
