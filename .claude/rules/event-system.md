---
description: Event and delegate pattern for cross-system communication and UI updates
globs: ["Assets/Scripts/**/*.cs"]
---

Use `System.Action` delegates for UI updates and cross-system notifications. Always pair subscribe/unsubscribe to avoid memory leaks or stale listeners.

```csharp
// Publisher (on the system/manager)
public event Action<int> OnHealthChanged;

// Subscriber (on UI or dependent script)
void OnEnable()  => HealthSystem.Instance.OnHealthChanged += UpdateBar;
void OnDisable() => HealthSystem.Instance.OnHealthChanged -= UpdateBar;
```

Rules:
- Subscribe in `OnEnable`, unsubscribe in `OnDisable` (not `Start`/`OnDestroy`).
- Never subscribe without a corresponding unsubscribe.
- Use `Invoke` only to fire the event: `OnHealthChanged?.Invoke(currentHp);`
