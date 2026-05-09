using System;
using System.Collections.Generic;

[Serializable]
public class AchievementDefinition
{
    public string id;
    public int number;
    public string iconId;
    public string title;
    public string description;

    // Temporary for UI test.
    // Later this should come from player progress save file.
    public bool unlocked;
}

[Serializable]
public class AchievementDefinitionList
{
    public List<AchievementDefinition> achievements;
}