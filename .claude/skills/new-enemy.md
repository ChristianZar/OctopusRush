# new-enemy

Walk me through adding a new enemy type to OctopusRush. I will provide the enemy details and you will scaffold all required scripts following the project's existing patterns.

## Steps to follow

1. **Ask me for the enemy details** if not already provided:
   - Enemy name (e.g. `Seahorse`)
   - Movement pattern: patrol / wander / ambush / chase / stationary
   - Attack type: contact damage / projectile / none
   - Health / damage values
   - Does it respond to ink slow? yes/no
   - Spawn zone: top / sides / bottom / all

2. **Read relevant existing enemy scripts** for reference before writing any code:
   - Choose the closest existing pattern (e.g. `SharkAi.cs` for patrol+chase, `JellyfishWander2D.cs` for wander)
   - Read the chosen reference script fully

3. **Create the AI script** at `Assets/Scripts/<EnemyName>Behavior.cs` (or `<EnemyName>AI.cs` for chase-based):
   - Use the enum state machine pattern (see `.claude/rules/ai-state-machines.md`)
   - Include `[Header]` groups for all tunable values
   - Add ink slow support if requested (a `slowMultiplier` field and a public `ApplySlow()` method)

4. **Create the spawner script** at `Assets/Scripts/<EnemyName>Spawner.cs`:
   - Follow the pattern of an existing spawner (e.g. `SharkSpawner.cs`)
   - All wave/interval values must be `[SerializeField]` fields — no hardcoded numbers

5. **List the manual Unity steps** the user still needs to do:
   - Create the prefab in `Assets/Prefabs/`
   - Assign sprite, collider, Rigidbody2D
   - Add the spawner to the scene and wire the prefab reference

## Rules to follow
- File name must match the class name exactly (PascalCase).
- All tunable numbers go in `[Header]` serialized fields.
- Never call `FindObjectOfType` inside `Update` — cache references in `Awake`/`Start`.
- If the enemy deals contact damage, implement it in `OnTriggerEnter2D` / `OnCollisionEnter2D`, not in `Update`.
