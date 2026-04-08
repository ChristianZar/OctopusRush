using UnityEngine;

[CreateAssetMenu(fileName = "SkinData", menuName = "Octopush/Skin Data")]
public class SkinData : ScriptableObject
{
    [Header("Info")]
    public string skinName = "Default";
    public Sprite thumbnail;           // shown in shop UI
    public int keyCost = 0;            // 0 = free/default

    [Header("Animation Frames")]
    public Sprite[] swimmingFrames;    // drop your sprite sheet slices here
    public Sprite[] idleFrames;        // optional; falls back to swimmingFrames if empty
}
