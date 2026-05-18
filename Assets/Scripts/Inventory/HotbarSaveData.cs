using System.Collections.Generic;

[System.Serializable]
public class InventorySaveData
{
    public List<InventoryItemSaveData> items = new List<InventoryItemSaveData>();
}

[System.Serializable]
public class InventoryItemSaveData
{
    public int slotIndex;
    public string itemId;
    public int amount;
}
