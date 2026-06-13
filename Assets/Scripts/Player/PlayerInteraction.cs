using System;
using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private UpgradeStation upgradeStation;
    private InventoryItem heldItem;

    [Header("Keybindings ")]
    public KeyCode pickKey = KeyCode.E;
    public KeyCode upgradeKey = KeyCode.R;
    public KeyCode addToInventoryKey = KeyCode.Q;
    public KeyCode talkKey = KeyCode.T;

    [SerializeField] private float pickUpDistance = 2f;
    public LayerMask pickUpLayerMask;
    public Transform playerCameraTransform;
    public Transform objectGrabPointTransform;
    public HotbarManager hotbarManager;

    public TMP_Text buttonPressedText;

    [Header("Cooking System")]
    public KeyCode activateCookBook = KeyCode.Tab;
    public CookbookUI cookBook;

    private int activeHotbarIndex = -1;              // which hotbar slot is currently active
    private InventoryItem heldStackableVisual = null; // visual representation of stackable item in hands

    private CookingStation cookingStation;
    private InventoryItem nearbyItem; // item near the player for pickup
    private AdventurerNPC nearbyAdventurer;

    // Events for key interactions
    public static event Action<string> OnInteraction;
    public static event Action OnEndedInteraction;

    // Event for in game changes
    public static event Action<string> OnLockedItemInteraction;
    public static event Action<string> OnFullInventory;

    void Update()
    {
        if (Input.GetKeyDown(upgradeKey))
        {
            TryUpgradeItem();
        }
        if (Input.GetKeyDown(pickKey))
        {
            if (cookingStation != null && cookingStation.IsWaitingForIngredient)
            {
                cookingStation.AddCurrentIngredient();
                return;
            }

            PickUpAndDrop();
        }
        if (Input.GetKeyDown(addToInventoryKey))
        {
            AddAndRemoveFromInventory();
        }
        if(Input.GetKeyDown(activateCookBook))
        {
            OpenAndCloseCookBook();
        }

        if (Input.GetKeyDown(talkKey))
        {
            nearbyAdventurer.Interact(this);
        }

        HandleHotbarNumberKeys();
        HighliteHotbarSlot();
    }

    private void OnTriggerEnter(Collider other)
    {
        // NEW:
        // We search for UpgradeStation not only on the collider object,
        // but also on its parent.
        //
        // This is important because many stations have this hierarchy:
        //
        // MagicFridge
        //   ├── UpgradeStation
        //   └── InteractionTrigger
        //        └── BoxCollider
        //
        // In that case "other" is InteractionTrigger, not MagicFridge.
        
        Debug.Log(
            $"[TRIGGER ENTER RAW] other={other.name}, " +
            $"root={other.transform.root.name}, " +
            $"parent={(other.transform.parent != null ? other.transform.parent.name : "NULL")}, " +
            $"layer={LayerMask.LayerToName(other.gameObject.layer)}, " +
            $"isTrigger={other.isTrigger}"
        );
        
        UpgradeStation station = other.GetComponentInParent<UpgradeStation>();

        if (station != null)
        {
            upgradeStation = station;

            if (!upgradeStation.CanInteract)
            {
                OnLockedItemInteraction?.Invoke($"This station is locked.");
                Debug.Log($"[INTERACTION] Entered locked upgrade station: {station.name}");
                return;
            }
            else
            {
                OnInteraction?.Invoke($"Press {upgradeKey} to modify item");
            }

            Debug.Log($"[INTERACTION] Entered upgrade station: {station.name} through collider: {other.name}");
        }

        RoomDoor door = other.GetComponentInParent<RoomDoor>();

        if (door != null)
        {
            if (door.IsLocked)
            {
                OnLockedItemInteraction?.Invoke($"The door is locked!");
            }
        }

        // Better to also use GetComponentInParent here,
        // because AdventurerNPC may also have colliders on child objects.
        AdventurerNPC adventurer = other.GetComponentInParent<AdventurerNPC>();

        if (adventurer != null)
        {
            nearbyAdventurer = adventurer;

            OnInteraction?.Invoke($"Press {talkKey} to talk to adventurer");

            Debug.Log($"[INTERACTION] Entered adventurer: {adventurer.name}");
        }

        // Same idea for CookingStation.
        // The trigger collider may be on a child object.
        CookingStation cauldron = other.GetComponentInParent<CookingStation>();

        if (cauldron != null)
        {
            cookingStation = cauldron;

            OnInteraction?.Invoke($"Press {activateCookBook} to interact with cookbook");
            
            Debug.Log($"[INTERACTION] Entered cooking station: {cauldron.name}");
        }

        // Existing item debug check.
        FoodItem item = other.GetComponentInParent<FoodItem>();

        if (item != null)
        {
            if(!item.isHeld && !item.isInInventory)
            {
                nearbyItem = item;

                OnInteraction?.Invoke($"Press {pickKey} to add to inventory");
                Debug.Log($"[INTERACTION] Entered item pickup area: {item.name}");
            }
            return;
        }

        RewardItem reward = other.GetComponentInParent<RewardItem>();

        if (reward != null)
        {
            if (!reward.isHeld && !reward.isInInventory)
            {
                nearbyItem = reward;

                OnInteraction?.Invoke($"Press {pickKey} to pick up");
                Debug.Log($"[INTERACTION] Entered item pickup area: {reward.name}");
            }
            Debug.LogWarning($"RewardItem '{reward.name}' has NULL itemData (triggered by collider '{other.name}')");
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // NEW:
        // Again, use GetComponentInParent.
        // Otherwise exiting from child trigger will not correctly clear upgradeStation.
        UpgradeStation station = other.GetComponentInParent<UpgradeStation>();

        if (station != null && station == upgradeStation)
        {
            Debug.Log($"[INTERACTION] Exited upgrade station: {station.name}");

            upgradeStation = null;

            OnEndedInteraction?.Invoke();
        }

        AdventurerNPC adventurer = other.GetComponentInParent<AdventurerNPC>();

        if (adventurer != null && adventurer == nearbyAdventurer)
        {
            Debug.Log($"[INTERACTION] Exited adventurer: {adventurer.name}");

            nearbyAdventurer = null;

            OnEndedInteraction?.Invoke();
        }

        CookingStation cauldron = other.GetComponentInParent<CookingStation>();

        if (cauldron != null && cauldron == cookingStation)
        {
            Debug.Log($"[INTERACTION] Exited cooking station: {cauldron.name}");

            cookingStation = null;

            OnEndedInteraction?.Invoke();
        }

        InventoryItem item = other.GetComponentInParent<InventoryItem>();

        if (item != null)
        {
            if(nearbyItem != null && item == nearbyItem)
            {
                Debug.Log($"[INTERACTION] Exited food item: {item.name}");

                nearbyItem = null;

                OnEndedInteraction?.Invoke();
            }
        }
    }

    private void PickUpAndDrop()
    {
        if (heldItem == null)
        {
            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance, pickUpLayerMask))
            {
                if (raycastHit.transform.TryGetComponent<InventoryItem>(out var inventoryItem))
                {
                    if(inventoryItem.itemData.isStackable)
                    {
                        bool isAdded = hotbarManager.TryAddStackableItemToInventory(inventoryItem);
                        if (isAdded)
                        {
                            nearbyItem = null;
                            OnEndedInteraction?.Invoke();

                            Debug.Log("Stackable item added to inventory.");
                            return;
                        }
                        else
                        {
                            Debug.Log("No place in the inventory for stackable item.");
                        }
                    }
                    else
                    {
                        heldItem = inventoryItem;
                        heldItem.Grab(objectGrabPointTransform);
                        heldItem.isHeld = true;
                        
                        nearbyItem = null;
                        OnInteraction?.Invoke($"Press {addToInventoryKey} to add to inventory");
                        

                        EventManager.CallItemPickedUp(heldItem);
                        Debug.Log("Picked up ");
                    }
                }
            }
        }
        else
        {
            if (!heldItem.itemData.isStackable)
            {

                EventManager.CallItemDropped(heldItem);

                heldItem.Drop();
                heldItem = null;
                Debug.Log("Dropped held item");

                OnEndedInteraction?.Invoke();
            }
            else
            {
                OnInteraction?.Invoke($"Press {addToInventoryKey} to drop the item");
            }
        }
    }

    private void AddAndRemoveFromInventory()
    {
        if (heldItem == null)
        {
            Debug.Log("No item in hands.");
            return;
        }

        if (!heldItem.isInInventory)
        {

            bool added = false;

            if (heldItem.itemData.isStackable)
            {
                added = hotbarManager.TryAddStackableItemToInventory(heldItem);
            }
            else
            {
                added = hotbarManager.TryAddNonStackableItemToInventory(heldItem);
            }

            if (added)
            {
                heldItem.isHeld = false;
                heldItem = null;
                OnEndedInteraction?.Invoke();

                Debug.Log("Item stored in inventory from world.");
            }
            else
            {
                OnFullInventory?.Invoke("Hotbar is full");
                Debug.Log("No place in the inventory to store item.");
            }

            return;
        }

        int slotIndex = -1;

        if (heldItem.itemData.isStackable)
        {
            slotIndex = activeHotbarIndex;
        }
        else
        {
            slotIndex = hotbarManager.GetItemPositionInInventory(heldItem);
        }

        if (slotIndex == -1)
        {
            Debug.LogWarning("Cannot resolve slot index for held item.");
            return;
        }

        if (heldItem.itemData.isStackable)
        {
            // кинули 1 зі стека
            hotbarManager.RemoveStackableItemFromInventory(slotIndex);

            // check if there's still some stack left
            HotbarSlot slot = hotbarManager.GetSlotByPosition(slotIndex);

            if (slot != null && slot.itemData != null && slot.amount > 0)
            {
                // check — still have some stack left — update visual
                EquipStackableFromSlot(slot);
                heldItem = heldStackableVisual;
                heldItem.isHeld = true;
                Debug.Log("Dropped 1 stackable, still holding stackable visual.");
            }
            else
            {
                // stack is empty now — clear hands
                ClearHands(dropWorldItem: false);
                activeHotbarIndex = -1;
                Debug.Log("Dropped last stackable, cleared hands.");
            }

            return;
        }
        else
        {
            // non-stackable — повністю викидаємо
            hotbarManager.RemoveNonStackableItemFromInventory(slotIndex);
            ClearHands(dropWorldItem: false);
            activeHotbarIndex = -1;

            Debug.Log("Dropped non-stackable from inventory.");
            OnEndedInteraction?.Invoke();
        }
    }

    public void ResetHighlite()
    {
        for (int i = 0; i < hotbarManager.slots.Length; i++)
        {
            HotbarSlot slot = hotbarManager.slots[i];
            slot.icon.color = slot.originalColor;
        }
    }

    public void HighliteHotbarSlot()
    {
        ResetHighlite();

        if (heldItem == null) return;

        if (heldItem.isInInventory && heldItem.isHeld)
        {
            int slotIndex = -1;

            if (heldItem.itemData != null && heldItem.itemData.isStackable)
                slotIndex = activeHotbarIndex; // for stackable
            else
                slotIndex = hotbarManager.GetItemPositionInInventory(heldItem); // for non-stackable

            if (slotIndex != -1)
            {
                HotbarSlot slot = hotbarManager.slots[slotIndex];
                slot.icon.color = Color.yellow;
            }
        }
    }

    private void HandleHotbarNumberKeys()
    {
        int index = GetHotbarSlotByKeyIndex();
        if (index == -1)
            return;

        EquipHotbarSlot(index);
    }

    public void EquipHotbarSlot(int index)
    {
        HotbarSlot slot = hotbarManager.GetSlotByPosition(index);

        // if the slot is empty - clear hands
        if (slot == null || (slot.uniqueItem == null && (slot.itemData == null || slot.amount <= 0)))
        {
            // if the item is from the inventory, simply hide it; if it is from the world, discard it
            bool dropWorld = (heldItem != null && !heldItem.isInInventory);
            ClearHands(dropWorldItem: dropWorld);

            activeHotbarIndex = -1;
            OnEndedInteraction?.Invoke();

            Debug.Log("Selected empty hotbar slot, cleared hands");
            return;
        }

        // non-stackable slot
        if (slot.uniqueItem != null)
        {
            // remove current held item
            bool dropWorld = (heldItem != null && !heldItem.isInInventory);
            ClearHands(dropWorldItem: dropWorld);

            activeHotbarIndex = index;

            heldItem = slot.uniqueItem;
            heldItem.isInInventory = true;
            heldItem.gameObject.SetActive(true);
            heldItem.Grab(objectGrabPointTransform);
            heldItem.isHeld = true;

            OnInteraction?.Invoke($"Press {addToInventoryKey} to drop from inventory");

            Debug.Log("Equipped NON-stackable from slot " + (index + 1));
            return;
        }

        // Stackable item visual
        if (slot.itemData != null && slot.amount > 0 && slot.itemData.isStackable)
        {
            activeHotbarIndex = index;
            EquipStackableFromSlot(slot);

            OnInteraction?.Invoke($"Press {addToInventoryKey} to drop from inventory");

            Debug.Log("Equipped STACKABLE visual from slot " + (index + 1));
            return;
        }

        // Non-stackable item restored from save by itemData
        if (slot.itemData != null && slot.amount > 0 && !slot.itemData.isStackable)
        {
            activeHotbarIndex = index;
            EquipNonStackableFromSlot(slot);

            OnInteraction?.Invoke($"Press {addToInventoryKey} to drop from inventory");

            Debug.Log("Equipped NON-stackable visual from itemData slot " + (index + 1));
            return;
        }
    }

    private int GetHotbarSlotByKeyIndex()
    {
        int index = -1;

        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) index = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) index = 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) index = 5;
        if (Input.GetKeyDown(KeyCode.Alpha7)) index = 6;
        if (Input.GetKeyDown(KeyCode.Alpha8)) index = 7;
        if (Input.GetKeyDown(KeyCode.Alpha9)) index = 8;
        return index;
    }

    void TryUpgradeItem()
    {
        if (upgradeStation == null)
        {
            Debug.Log("There's no upgrade station");
            return;
        }

        if (heldItem == null)
        {
            Debug.Log("There's nothing in hands, nothing to modify");
            return;
        }

        if (heldItem is not RewardItem reward)
        {
            Debug.Log("Held item is not a reward item");
            return;
        }

        if (!reward.isInInventory)
        {
            Debug.Log("Item is not in inventory");
            return;
        }

        
        Debug.Log($"[UPGRADE] Using station: {upgradeStation.name}");
        Debug.Log($"[UPGRADE] Item before: {reward.name}, Value={reward.Value}, State={reward.CurrentState}");

        RewardItem result = upgradeStation.UpgradeItem(reward);

        if (result == null)
        {
            Debug.Log("Item was destroyed or removed by station");
            heldItem = null;
            return;
        }
        
        
        Debug.Log($"[UPGRADE] Item after: {result.name}, Value={result.Value}, State={result.CurrentState}");

        
        if (!upgradeStation.LastProcessSuccessful)
        {
            Debug.Log("Station did not modify the item.");
            return;
        }

        Debug.Log("Item processed. Current value = " + result.Value);

        EventManager.CallItemModified(result, upgradeStation);
    }
    
    
    
    private void OpenAndCloseCookBook()
    {
        if (cookingStation == null)
        {
            Debug.Log("There's no cooking station");
            return;
        }
        else { 
            if (cookBook == null)
            {
                Debug.LogWarning("No CookBook assigned to playerInteraction script.");
                return;
            }else if (!cookBook.IsOpened()) {
                cookBook.OpenCookBook();
            }
            else
            {
                cookBook.CloseCookBook();
            }
        }
    }

    private void ClearHands(bool dropWorldItem)
    {
        // remove stackable visual if any
        if (heldStackableVisual != null)
        {
            Destroy(heldStackableVisual.gameObject);
            heldStackableVisual = null;
        }

        if (heldItem != null)
        {
            if (heldItem.isInInventory)
            {
                heldItem.isHeld = false;
                heldItem.gameObject.SetActive(false);
            }
            else
            {
                if (dropWorldItem) heldItem.Drop();
            }

            heldItem = null;
        }
    }

    private void EquipStackableFromSlot(HotbarSlot slot)
    {
        if (slot == null || slot.itemData == null || slot.amount <= 0) return;
        if (!slot.itemData.isStackable) return;
        if (slot.itemData.worldPrefab == null)
        {
            Debug.LogWarning("Stackable slot has no worldPrefab in ItemData.");
            return;
        }

        // if we already have the correct visual, just use it
        if (heldStackableVisual != null && heldStackableVisual.itemData == slot.itemData)
        {
            heldItem = heldStackableVisual;
            return;
        }

        // remove current held item
        ClearHands(dropWorldItem: false);

        // create new visual
        GameObject go = Instantiate(slot.itemData.worldPrefab, objectGrabPointTransform.position, objectGrabPointTransform.rotation);
        heldStackableVisual = go.GetComponent<InventoryItem>();
        if (heldStackableVisual == null)
        {
            Debug.LogWarning("worldPrefab for stackable has no InventoryItem component.");
            Destroy(go);
            return;
        }

        heldStackableVisual.isInInventory = true;
        heldStackableVisual.Grab(objectGrabPointTransform);

        heldItem = heldStackableVisual;
    }

    private void EquipNonStackableFromSlot(HotbarSlot slot)
    {
        // if there is an object in hand not from inventory -> drop it
        bool dropWorld = (heldItem != null && !heldItem.isInInventory);
        ClearHands(dropWorldItem: dropWorld);

        if (slot.itemData == null)
        {
            //Debug.LogWarning("[EQUIP] Cannot equip non-stackable item. itemData is NULL.");
            return;
        }

        // check if data has prefab reference
        if (slot.itemData.worldPrefab == null)
        {
            Debug.LogWarning("[EQUIP] Cannot equip non-stackable item. World prefab is NULL for: " + slot.itemData.name);
            return;
        }

        // add object to the hand
        GameObject spawnedObject = Instantiate(
            slot.itemData.worldPrefab,
            objectGrabPointTransform.position,
            objectGrabPointTransform.rotation
        );

        RewardItem itemObject = spawnedObject.GetComponent<RewardItem>();

        if (itemObject == null)
        {
            Debug.LogWarning("[EQUIP] Spawned prefab does not have RewardItem component: " + spawnedObject.name);
            Destroy(spawnedObject);
            return;
        }

        // assign item data to the spawned object
        heldItem = itemObject;
        heldItem.isInInventory = true;
        heldItem.gameObject.SetActive(true);
        heldItem.Grab(objectGrabPointTransform);
        heldItem.isHeld = true;

        slot.uniqueItem = heldItem;

        Debug.Log("[EQUIP] Non-stackable item equipped from itemData: " + slot.itemData.name);
    }

    public InventoryItem getHeldItem()
    {
        return (heldItem != null) ? heldItem : null;
    }

    public int GetActiveHotbarIndex()
    {
        return activeHotbarIndex;
    }

    public void RestoreActiveHotbarSlot(int index)
    {
        activeHotbarIndex = index;

        if (index < 0)
        {
            ClearHands(dropWorldItem: false);
            return;
        }

        HotbarSlot slot = hotbarManager.GetSlotByPosition(index);

        if (slot == null || (slot.uniqueItem == null && (slot.itemData == null || slot.amount <= 0)))
        {
            ClearHands(dropWorldItem: false);
            activeHotbarIndex = -1;
            return;
        }

        EquipHotbarSlot(index);

        Debug.Log("[PLAYER LOAD] Restored active hotbar slot: " + index);
    }
}
