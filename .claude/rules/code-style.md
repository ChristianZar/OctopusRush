---
description: C# code style rules for Unity MonoBehaviour scripts
globs: ["Assets/Scripts/**/*.cs"]
---

- **Inspector-first design:** expose tunable fields as `public` or `[SerializeField] private`. Group related fields with `[Header("Section Name")]`.
- **Arrow getters** for trivial read-only accessors: `public float GetInk() => currentInk;`
- **Null-conditional operators** (`?.`) for optional component lookups instead of explicit null guards.
- **No XML doc comments.** Rely on clear method and variable names. Add inline comments only for non-obvious logic.
- **Section dividers** in longer files: `// ── Section Name ──`
- Always use `Time.deltaTime` for frame-rate-dependent movement and timers.
- Use `Time.unscaledDeltaTime` for UI animations and anything that must run while `Time.timeScale = 0` (pause screen, game-over, pickup freeze-frames).
- Use `Mathf.Lerp()` for smooth transitions (speed blending, UI animations).

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
