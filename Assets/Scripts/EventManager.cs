using System;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static event Action<InventoryItem> OnItemPickedUp;
    public static event Action<InventoryItem> OnItemDropped;
    public static event Action<RewardItem, UpgradeStation> OnItemUpgraded;
    public static event Action<InventoryItem> OnItemStored;
    public static event Action<InventoryItem> OnItemRemoved;

    public static void CallItemPickedUp(InventoryItem item)
    {
        OnItemPickedUp?.Invoke(item);
    }

    public static void CallItemDropped(InventoryItem item)
    {
        OnItemDropped?.Invoke(item);
    }

    public static void CallItemModified(RewardItem item, UpgradeStation station)
    {
        OnItemUpgraded?.Invoke(item, station);
    }

    public static void CallItemStored(InventoryItem item)
    {
        OnItemStored?.Invoke(item);
    }

    public static void CallItemRemoved(InventoryItem item)
    {
        OnItemRemoved?.Invoke(item);
    }
}

