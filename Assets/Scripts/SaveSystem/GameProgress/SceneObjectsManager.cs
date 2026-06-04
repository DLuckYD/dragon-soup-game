using UnityEngine;
public class SceneObjectsManager : MonoBehaviour
{
    public static SceneObjectsManager Instance { get; private set; }

    [Header("Items Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    [SerializeField] private Transform worldItemsParent;

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

    public SceneSaveData CaptureSaveData()
    {
        SceneSaveData saveData = new SceneSaveData();

        InventoryItem[] worldItems = FindObjectsByType<InventoryItem>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        Debug.Log("[SCENE SAVE] Found InventoryItem objects: " + worldItems.Length);

        for (int i = 0; i < worldItems.Length; i++)
        {
            InventoryItem item = worldItems[i];

            if (item == null)
            {
                Debug.LogWarning("[SCENE SAVE] Item " + i + " is NULL. Skipping.");
                continue;
            }

            if (item.itemData == null)
            {
                Debug.LogWarning("[SCENE SAVE] Item has NULL itemData: " + item.name);
                continue;
            }

            if (item.isInInventory)
            {
                Debug.Log("[SCENE SAVE] Skipping item in inventory or held by player: " + item.name);
                continue;
            }

            Vector3 position = item.transform.position;
            Quaternion rotation = item.transform.rotation;

            SceneItemSaveData itemSaveData = new SceneItemSaveData
            {
                itemId = item.itemData.id,

                posX = position.x,
                posY = position.y,
                posZ = position.z,

                rotX = rotation.x,
                rotY = rotation.y,
                rotZ = rotation.z,
                rotW = rotation.w,

                isActive = item.gameObject.activeSelf
            };

            RewardItem rewardItem = item as RewardItem;

            if (rewardItem != null)
            {
                itemSaveData.isUpgraded = rewardItem.isUpgraded;
                itemSaveData.value = rewardItem.Value;
                itemSaveData.canBeUpgraded = rewardItem.canBeModified;
                itemSaveData.itemType = rewardItem.type;
            }
            saveData.items.Add(itemSaveData);
        }

        return saveData;
    }

    public void RestoreSaveData(SceneSaveData saveData)
    {

        if (saveData == null || saveData.items == null)
        {
            Debug.LogWarning("[SCENE LOAD] Save data is NULL.");
            return;
        }

        // delete all the items in the scene
        ClearCurrentSceneItems();

        // spawn items from save data
        foreach (SceneItemSaveData savedItem in saveData.items)
        {
            SpawnItemFromSave(savedItem);
        }
    }

    private void ClearCurrentSceneItems()
    {
        // get all InventoryItem objects in the scene
        InventoryItem[] currentItems = FindObjectsByType<InventoryItem>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        Debug.Log("[SCENE LOAD] Clearing current scene items: " + currentItems.Length);

        // destroy all of them except those that are in inventory or held by the player
        foreach (InventoryItem item in currentItems)
        {
            if (item == null)
                continue;

            if (item.isInInventory || item.isHeld)
            {
                Debug.Log("[SCENE LOAD] Skipping inventory/held item while clearing: " + item.name);
                continue;
            }

            Destroy(item.gameObject);
        }
    }

    private void SpawnItemFromSave(SceneItemSaveData savedItem)
    {
        IngredientData itemData = itemDatabase.GetItemById(savedItem.itemId);

        if (itemData == null)
        {
            Debug.LogWarning("[SCENE LOAD] Cannot spawn item. Missing IngredientData with id: " + savedItem.itemId);
            return;
        }

        if (itemData.worldPrefab == null)
        {
            Debug.LogWarning("[SCENE LOAD] Cannot spawn item. worldPrefab is NULL for item: " + itemData.name);
            return;
        }

        Vector3 position = new Vector3(savedItem.posX, savedItem.posY, savedItem.posZ);
        Quaternion rotation = new Quaternion(savedItem.rotX, savedItem.rotY, savedItem.rotZ, savedItem.rotW);

        GameObject spawnedObject = Instantiate(
            itemData.worldPrefab,
            position,
            rotation,
            worldItemsParent
        );

        // in order to add InventoryItem attributes
        InventoryItem inventoryItem = spawnedObject.GetComponent<InventoryItem>();

        if (inventoryItem == null)
        {
            Debug.LogWarning("[SCENE LOAD] Spawned prefab does not have InventoryItem component: " + spawnedObject.name);
            Destroy(spawnedObject);
            return;
        }

        inventoryItem.itemData = itemData;
        spawnedObject.SetActive(savedItem.isActive);

        RewardItem rewardItem = inventoryItem as RewardItem;

        if (rewardItem != null)
        {
            rewardItem.isUpgraded = savedItem.isUpgraded;
            rewardItem.Value = savedItem.value;
            rewardItem.canBeModified = savedItem.canBeUpgraded;
            rewardItem.type = savedItem.itemType;
            rewardItem.UpdateVisual();
        }
    }
}
