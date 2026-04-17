using NUnit.Framework.Internal.Execution;
using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    public HotbarSlot[] slots;

    [Header("Drop Point for Items")]
    public Transform dropPoint;

    public bool TryAddNonStackableItemToInventory(InventoryItem worldItem)
    {
        if (worldItem == null || worldItem.itemData == null) return false;
        if (worldItem.itemData.isStackable) return false;

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s.IsEmpty)
            {
                s.uniqueItem = worldItem;
                s.itemData = worldItem.itemData;
                s.amount = 1;

                worldItem.isInInventory = true;
                worldItem.gameObject.SetActive(false);

                s.UpdateUI();
                return true;
            }
        }
        return false;
    }

    public bool TryAddStackableItemToInventory(InventoryItem worldItem)
    {
        if (worldItem == null || worldItem.itemData == null) return false;
        if (!worldItem.itemData.isStackable) return false;

        ItemData data = worldItem.itemData;

        // 1) stack into existing
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s.itemData == data && s.amount < data.maxStackSize)
            {
                s.amount++;
                s.UpdateUI();
                Destroy(worldItem.gameObject);
                return true;
            }
        }

        // 2) create new stack in empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s.IsEmpty)
            {
                s.itemData = data;
                s.amount = 1;
                s.uniqueItem = null;
                s.UpdateUI();

                Destroy(worldItem.gameObject);
                return true;
            }
        }

        return false;
    }

    public void RemoveNonStackableItemFromInventory(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        var s = slots[index];
        if (s.uniqueItem == null) return;

        var item = s.uniqueItem;
        s.Clear();

        item.gameObject.SetActive(true);
        item.isInInventory = false;
        item.isHeld = false;
        item.Drop();
    }

    public void RemoveStackableItemFromInventory(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        var s = slots[index];
        if (s.itemData == null || s.amount <= 0) return;

        var data = s.itemData;
        if (!data.isStackable) return;

        if (data.worldPrefab == null)
        {
            Debug.LogWarning($"Hotbar: worldPrefab is null for {data.name}");
            return;
        }

        Vector3 pos = dropPoint != null ? dropPoint.position : Vector3.zero;
        Quaternion rot = dropPoint != null ? dropPoint.rotation : Quaternion.identity;

        var go = Instantiate(data.worldPrefab, pos, rot);

        var inv = go.GetComponent<InventoryItem>();
        if (inv != null)
        {
            inv.isInInventory = false;
            inv.isHeld = false;
            inv.Drop();
        }

        s.amount--;
        if (s.amount <= 0) s.Clear();
        else s.UpdateUI();
    }

    public int GetItemPositionInInventory(InventoryItem item)
    {
        if (item == null) return -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].uniqueItem == item)
                return i;
        }
        return -1;
    }

    public InventoryItem GetItemInInventoryByPosition(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index].uniqueItem;
    }

    public HotbarSlot GetSlotByPosition(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }

    public bool HasItemInInventory(InventoryItem item)
    {
        if (item == null) return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].uniqueItem == item)
                return true;
        }
        return false;
    }

    public bool HasItemDataAmount(ItemData data, int requiredAmount)
    {
        int total = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s.itemData == data)
            {
                total += s.amount;
                if (total >= requiredAmount)
                    return true;
            }
        }

        return false;
    }

    public void RemoveItemDataAmount(ItemData data, int amount)
    {
        int remaining = amount;
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s.itemData == data)
            {
                if (s.amount >= remaining)
                {
                    s.amount -= remaining;
                    if (s.amount <= 0) s.Clear();
                    else s.UpdateUI();
                    return;
                }
                else
                {
                    remaining -= s.amount;
                    s.Clear();
                }
            }
        }
    }

    public void addCookingDishToInventory(ItemData result)
    {
        if (result == null)
        {
            Debug.LogWarning("addCookingDishToInventory: result is NULL");
            return;
        }

        if (result.worldPrefab == null)
        {
            Debug.LogWarning($"addCookingDishToInventory: worldPrefab is NULL for ItemData {result.name}");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s.IsEmpty)
            {
                // create prefab instance
                GameObject go = Instantiate(result.worldPrefab);
                go.name = result.displayName + " (Inventory)";

                // get any InventoryItem component
                InventoryItem inventoryItem = go.GetComponent<InventoryItem>();
                if (inventoryItem == null)
                {
                    Debug.LogWarning("addCookingDishToInventory: worldPrefab has no InventoryItem component.");
                    Destroy(go);
                    return;
                }

                // add itemData and hide
                inventoryItem.itemData = result;
                inventoryItem.isInInventory = true;
                inventoryItem.isHeld = false;
                go.SetActive(false);

                // add to the slot
                s.itemData = result;
                s.uniqueItem = inventoryItem;
                s.amount = 1;

                s.UpdateUI();
                return;
            }
        }

        Debug.Log("addCookingDishToInventory: inventory is full");
    }
}