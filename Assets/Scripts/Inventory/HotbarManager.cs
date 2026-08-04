using System;
using System.Collections.Generic;
using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance { get; private set; }
    public HotbarSlot[] slots;

    [Header("Items Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    public static event Action<int> OnRequestEquipSlot;

    [Header("Drop Point for Items")]
    public Transform dropPoint;

    [SerializeField] private int activeSlotIndex = 0;
    public int ActiveSlotIndex => activeSlotIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

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

        IngredientData data = worldItem.itemData;

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

    public bool HasItemDataAmount(IngredientData data, int requiredAmount)
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

    public void RemoveItemDataAmount(IngredientData data, int amount)
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

    public bool AddCookingDishToInventory(IngredientData result)
    {
        if (result == null)
        {
            Debug.LogWarning("addCookingDishToInventory: result is NULL");
            return false;
        }

        if (result.worldPrefab == null)
        {
            Debug.LogWarning($"addCookingDishToInventory: worldPrefab is NULL for IngredientData {result.name}");
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s.IsEmpty)
            {
                GameObject go = Instantiate(result.worldPrefab);
                go.name = result.displayName + " (Inventory)";

                InventoryItem inventoryItem = go.GetComponent<InventoryItem>();
                if (inventoryItem == null)
                {
                    Debug.LogWarning("addCookingDishToInventory: worldPrefab has no InventoryItem component.");
                    Destroy(go);
                    return false;
                }

                inventoryItem.itemData = result;
                inventoryItem.isInInventory = true;
                inventoryItem.isHeld = false;
                go.SetActive(false);

                s.itemData = result;
                s.uniqueItem = inventoryItem;
                s.amount = 1;

                s.UpdateUI();

                SetActiveSlot(i);
                OnRequestEquipSlot?.Invoke(i);

                return true;
            }
        }

        Debug.Log("addCookingDishToInventory: inventory is full");
        return false;
    }

    public void SpawnDishInWorld(IngredientData result)
    {
        if (result == null || result.worldPrefab == null)
        {
            Debug.LogWarning("SpawnDishInWorld: result or worldPrefab is NULL");
            return;
        }

        Vector3 pos = dropPoint != null ? dropPoint.position : transform.position;
        Quaternion rot = dropPoint != null ? dropPoint.rotation : Quaternion.identity;

        GameObject go = Instantiate(result.worldPrefab, pos, rot);
        go.name = result.displayName + " (World)";

        InventoryItem inventoryItem = go.GetComponent<InventoryItem>();
        if (inventoryItem != null)
        {
            inventoryItem.itemData = result;
            inventoryItem.isInInventory = false;
            inventoryItem.isHeld = false;
        }

        Debug.Log("SpawnDishInWorld: inventory full, dropped dish in world: " + result.displayName);
    }

    public void SetActiveSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        activeSlotIndex = index;

        Debug.Log("Active slot changed to: " + activeSlotIndex);

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetHighlighted(i == activeSlotIndex);
        }
    }
    public InventorySaveData CaptureSaveData()
    {
        InventorySaveData saveData = new InventorySaveData();

        if (slots == null)
        {
            Debug.LogWarning("[HOTBAR SAVE] hotbarSlots is NULL.");
            return saveData;
        }

        Debug.Log("[HOTBAR SAVE] Hotbar slots count: " + slots.Length);

        for (int i = 0; i < slots.Length; i++)
        {
            HotbarSlot slot = slots[i];

            if (slot == null || slot.itemData == null || slot.amount <= 0)
                continue;

            InventoryItemSaveData itemSaveData = new InventoryItemSaveData
            {
                slotIndex = i,
                itemId = slot.itemData.id,
                amount = slot.amount
            };

            saveData.items.Add(itemSaveData);
        }

        Debug.Log("Captured inventory save data with " + saveData.items.Count + " items.");

        return saveData;
    }

    public void RestoreSaveData(InventorySaveData saveData)
    {
        ClearInventory();

        if (saveData == null || saveData.items == null)
            return;

        foreach (InventoryItemSaveData savedItem in saveData.items)
        {
            IngredientData itemData = itemDatabase.GetItemById(savedItem.itemId);

            if (itemData == null)
            {
                Debug.LogWarning("Cannot restore inventory item. Missing item id: " + savedItem.itemId);
                continue;
            }

            AddItemToSlot(savedItem.slotIndex, itemData, savedItem.amount);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].UpdateUI();
        }
    }

    private void ClearInventory()
    {
        foreach (HotbarSlot slot in slots)
        {
            slot.itemData = null;
            slot.amount = 0;
        }
    }

    private void AddItemToSlot(int slotIndex, IngredientData itemData, int amount)
    {
        List<HotbarSlot> inventorySlots = new List<HotbarSlot>(slots);
        while (slots.Length <= slotIndex)
        {
            inventorySlots.Add(new HotbarSlot());
        }

        inventorySlots[slotIndex].itemData = itemData;
        inventorySlots[slotIndex].amount = amount;

        slots = inventorySlots.ToArray();
    }

    public bool HasEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
                return true;
        }

        return false;
    }
    public int GetItemDataAmount(IngredientData itemData)
    {
        int totalAmount = 0;

        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.itemData == itemData)
            {
                totalAmount += slot.amount;
            }
        }

        return totalAmount;
    }

}