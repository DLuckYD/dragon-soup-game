using System;

[Serializable]
public class ProgressionEffect
{
    public EffectType effectType;
    public string targetId;
}

[Serializable]
public enum EffectType
{
    UnlockRecipe,
    UnlockUpdateStation,
    UnlockRoom,
    EndGame
}
