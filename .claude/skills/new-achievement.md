# new-achievement

Walk me through adding a new achievement to OctopusRush. I will provide the achievement details and you will implement all required changes across the codebase.

## Steps to follow

1. **Ask me for the achievement details** if not already provided:
   - ID (short unique string, e.g. `"kill_10_sharks"`)
   - Title (shown in UI, e.g. `"Shark Slayer"`)
   - Description (one sentence, e.g. `"Kill 10 sharks in a single run"`)
   - Tracking type: **per-run** (resets on death) or **cumulative** (lifetime total)
   - Trigger condition: what event/counter unlocks it
   - Secret: yes/no (hidden until unlocked)
   - Icon: sprite name or `TBD`

2. **Read the relevant files** before making any changes:
   - `Assets/Scripts/AchievementManager.cs`
   - `Assets/Scripts/AchievementData.cs`
   - `Assets/Scripts/AchievementPanel.cs`

3. **Implement the achievement:**
   - Add the `AchievementData` entry to the definitions list in `AchievementManager`
   - Add the counter variable (per-run or cumulative) in the correct group
   - Add the unlock check — call `Unlock(id)` when the condition is met
   - If per-run: make sure the counter is reset in the death/reset method
   - If cumulative: persist the counter via `PlayerPrefs`

4. **Confirm** the icon asset exists in `Assets/Sprite/`; if not, note it as a TODO.

5. **Summarize** what was changed and what still needs to be done manually in Unity (e.g., assigning the icon sprite in the Inspector).

## Rules to follow
- Per-run and cumulative counters must stay in separate groups — do not mix them.
- Use `PREF_UNLOCK + id` as the PlayerPrefs key for the unlock flag.
- Do not hardcode UI strings anywhere other than the `AchievementData` definition.
