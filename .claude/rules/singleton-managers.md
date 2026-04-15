---
description: Singleton pattern required for all persistent manager scripts
globs: ["Assets/Scripts/**/*.cs"]
---

All persistent manager classes must use this exact singleton guard in `Awake`. Never skip the duplication check — multiple scene loads will create duplicate managers without it.

```csharp
public static ManagerName Instance { get; private set; }

void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

Existing managers: `GameManager`, `ScoreManager`, `AchievementManager`, `SkinManager`, `AudioManager`, `PauseManager`.

When calling manager singletons from other scripts, guard against null (manager may not be in the scene):
```csharp
GameManager.Instance?.DoSomething();
// or
if (GameManager.Instance == null) return;
```
