using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade System/Effects/Add Value")]
public class AddValueEffect : ItemEffect
{
    [Header("Value Settings")]
    [SerializeField] private int amount = 5;

    public override void Apply(RewardItem item, UpgradeStation station)
    {
        // Safety check.
        if (item == null)
            return;

        // Меняем value предмета.
        item.AddValue(amount);

        Debug.Log($"[{item.name}] AddValueEffect applied. Amount = {amount}. New value = {item.Value}");
    }
}