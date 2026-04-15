# new-skin

Walk me through adding a new cosmetic skin to OctopusRush. I will provide the skin details and you will implement all required changes.

## Steps to follow

1. **Ask me for the skin details** if not already provided:
   - Skin name (e.g. `"GoldenOctopus"`)
   - Key cost (integer, how many keys to unlock)
   - Sprite frames: are the PNG assets already in `Assets/Sprite/`? yes/no
   - Animation clips: existing or new?
   - Any special visual effect beyond a sprite swap? yes/no

2. **Read the relevant files** before making changes:
   - `Assets/Scripts/SkinManager.cs`
   - `Assets/Scripts/SkinData.cs` (the ScriptableObject definition)
   - `Assets/Scripts/SkinShopPanel.cs`

3. **Implement the skin:**
   - Verify `SkinData.cs` has all the fields needed (name, cost, sprite frames, animation clips). If a field is missing, add it.
   - Register the new skin in `SkinManager`'s skin list
   - Confirm the `PlayerPrefs` ownership key pattern is consistent with existing skins

4. **List the manual Unity steps** the user still needs to do:
   - Create a new `SkinData` ScriptableObject asset in `Assets/SkinData/`
   - Fill in the cost, assign sprite frames and animation clips in the Inspector
   - Add the asset to `SkinManager`'s skin list in the Inspector

5. **Confirm** the sprite assets exist in `Assets/Sprite/`; flag any that are missing as TODOs.

## Rules to follow
- Skin unlock/equip state must be persisted via `PlayerPrefs` through `SkinManager` — do not add separate save logic.
- Key cost and display name live in the `SkinData` ScriptableObject, not hardcoded in `SkinManager`.
- `SkinShopPanel` reads from `SkinManager` — do not add skin-specific UI logic directly to the panel.
