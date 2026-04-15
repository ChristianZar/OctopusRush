---
description: State machine pattern for all enemy AI scripts
globs: ["Assets/Scripts/**/*.cs"]
---

Enemy AI uses an `enum` state + `switch` dispatched in `Update` or `FixedUpdate`. Follow this pattern when adding or editing AI scripts:

```csharp
enum State { Patrol, Chase, ReturnHome }
State _state;

void FixedUpdate() {
    switch (_state) {
        case State.Patrol:      Patrol();      break;
        case State.Chase:       Chase();       break;
        case State.ReturnHome:  ReturnHome();  break;
    }
}
```

Each state should be a self-contained private method. State transitions happen inside those methods by assigning `_state`.

Existing AI scripts: `SharkAI` (Patrol/Chase/ReturnHome), `CrabPatrol`, `JellyfishWander2D`, `PufferFishBehavior`, `AnglerFishAmbush`, `ShrimpBehavior`.
