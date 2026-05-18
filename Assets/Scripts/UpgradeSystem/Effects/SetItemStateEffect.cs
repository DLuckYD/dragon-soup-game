using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade System/Effects/Set Item State")]
public class SetItemStateEffect : ItemEffect
{
    [Header("State Settings")]
    [SerializeField] private ItemState state = ItemState.None;

    public override void Apply(RewardItem item, UpgradeStation station)
    {
        // Safety check.
        if (item == null)
            return;

        // Устанавливаем одно текущее состояние предмета.
        // Например: Upgraded, Burned, Frozen, Painted.
        item.SetState(state);

        Debug.Log($"[{item.name}] SetItemStateEffect applied. New state = {state}");
    }
}