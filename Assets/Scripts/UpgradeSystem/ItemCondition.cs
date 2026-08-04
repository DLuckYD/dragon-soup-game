using UnityEngine;

public abstract class ItemCondition : ScriptableObject
{
    [SerializeField] private string failMassage = "Condition Failed";
    [SerializeField] private string successMassage = "Condition Succeeded";

    public string FailMassage => failMassage;

    public abstract bool isValid(RewardItem item, UpgradeStation station);
    
}