using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{
    public abstract void Apply(RewardItem item, UpgradeStation station);
}
