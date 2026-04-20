using System;

[Serializable]
public class AchievementData
{
    public string id;
    public string icon;         // emoji shown in pop-up and panel
    public string title;
    public string description;
    public bool   isSecret;     // shows as "???" in panel while locked
}
