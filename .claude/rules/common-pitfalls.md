---
description: Common mistakes to avoid in this codebase
globs: ["Assets/Scripts/**/*.cs"]
---

- **`FindObjectOfType<T>()` returns null** if the manager prefab isn't in the active scene. Always guard: use `?.` or an explicit `if (X == null) return;` before accessing the result.

- **`Camara` is intentional** — do not autocorrect `CamaraAutoScroll`, `CamaraFollow2D`, `CamaraShake`, or `FitToCamara` to `Camera*`. Scene serialization references these exact names.

- **`SharkAi.cs` filename** uses lowercase `i`. Match this when adding related enemy files so the naming stays consistent with the existing file.

- **Spawner difficulty lives in the Inspector** — wave counts, spawn intervals, and difficulty ramps are serialized `[Header]` fields on spawner scripts. Do not hardcode these values in code.

- **Mine chain reactions are radius-sensitive** — `MineBehavior` uses `Physics2D.OverlapCircle` to trigger adjacent mines. Changing a mine's collider or explosion radius will affect every clustered mine group in the scene.
