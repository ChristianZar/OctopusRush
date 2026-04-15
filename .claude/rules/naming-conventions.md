---
description: C# naming conventions for all identifiers in this project
globs: ["Assets/Scripts/**/*.cs"]
---

Use these naming conventions for all C# identifiers:

| Target | Convention | Example |
|---|---|---|
| Classes | PascalCase | `PlayerController`, `SharkAI` |
| Public fields | camelCase | `speed`, `maxHealth` |
| Private fields | camelCase | `drainTimer`, `_rb` |
| Methods | PascalCase | `TakeDamage()`, `SpawnInkCloud()` |
| Constants / PlayerPrefs keys | UPPER_SNAKE_CASE | `BEST_KEY`, `PREF_UNLOCK` |

**Preserve the `Camara` typo** — existing class names (`CamaraAutoScroll`, `CamaraFollow2D`, `CamaraShake`, `FitToCamara`) use `Camara` instead of `Camera`. Do NOT rename them; Unity scene references will break.

The `SharkAI` script filename is `SharkAi.cs` (lowercase `i`). Match this casing when creating related files.
