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
    public Cookbook cookBook;

    private int activeHotbarIndex = -1;              // which hotbar slot is currently active
    private InventoryItem heldStackableVisual = null; // visual representation of stackable item in hands

    private CookingStation cookingStation;
    private InventoryItem nearbyItem; // item near the player for pickup
    private AdventurerNPC nearbyAdventurer;

    void Update()
    {
        if (Input.GetKeyDown(upgradeKey))
        {
            TryUpgradeItem();
        }
        if (Input.GetKeyDown(pickKey))
        {
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
        if (other.TryGetComponent(out UpgradeStation workbench))
        {
            upgradeStation = workbench;
            buttonPressedText.text = $"Press {upgradeKey} to upgrade item";
        }

        if (other.TryGetComponent(out AdventurerNPC adventurer))
        {
            nearbyAdventurer = adventurer;
            buttonPressedText.text = $"Press {talkKey} to talk to adventurer";
        }

        var cauldron = other.GetComponent<CookingStation>();
        if(cauldron != null)
        {
            cookingStation = cauldron;
            buttonPressedText.text = $"Press {activateCookBook} to cook a dish";
        }

        var item = other.GetComponentInParent<InventoryItem>();
        if (item != null && item.itemData == null)
        {
            Debug.LogWarning($"InventoryItem '{item.name}' has NULL itemData (triggered by collider '{other.name}')");
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var workbench = other.GetComponent<UpgradeStation>();
        if (workbench == upgradeStation)
        {
            upgradeStation = null;
            buttonPressedText.text = "";
        }

        if (other.TryGetComponent(out AdventurerNPC adventurer) &&
            adventurer == nearbyAdventurer)
        {
            nearbyAdventurer = null;
            buttonPressedText.text = "";
        }

        var cauldron = other.GetComponent<CookingStation>();
        if (cauldron == cookingStation)
        {
            cookingStation = null;
            buttonPressedText.text = "";
        }

        var item = other.GetComponent<InventoryItem>();
        if (item != null && item == nearbyItem)
        {
            nearbyItem = null;
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
            }
            else
            {
                buttonPressedText.text = $"Use hotbar to drop the item";
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

                Debug.Log("Item stored in inventory from world.");
            }
            else
            {
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
        if (index == -1) return;

        HotbarSlot slot = hotbarManager.GetSlotByPosition(index);

        // if the slot is empty - clear hands
        if (slot == null || (slot.uniqueItem == null && (slot.itemData == null || slot.amount <= 0)))
        {
            // if the item is from the inventory, simply hide it; if it is from the world, discard it
            bool dropWorld = (heldItem != null && !heldItem.isInInventory);
            ClearHands(dropWorldItem: dropWorld);

            activeHotbarIndex = -1;
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

            Debug.Log("Equipped NON-stackable from slot " + (index + 1));
            return;
        }

        // 
        if (slot.itemData != null && slot.amount > 0 && slot.itemData.isStackable)
        {
            activeHotbarIndex = index;
            EquipStackableFromSlot(slot);

            Debug.Log("Equipped STACKABLE visual from slot " + (index + 1));
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
        if (upgradeStation != null && heldItem is RewardItem reward && reward.isInInventory && !reward.isUpgraded)
        {
            upgradeStation.UpgradeItem(reward);
            Debug.Log("is upgraded value=" + reward.Value);
            EventManager.CallItemUpgraded(reward, upgradeStation);
        }
        else if (upgradeStation != null && heldItem == null)
        {
            Debug.Log("There's nothing in hands, nothing to upgrade");
        }
        else
        {
            Debug.Log("There's no upgrade station");
        }
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

    public InventoryItem getHeldItem()
    {
        return (heldItem != null) ? heldItem : null;
    }

    public void DeleteHeldItem()
    {
        if (heldItem == null) return;
        Destroy(heldItem.gameObject);
        heldItem = null;
    }
}
