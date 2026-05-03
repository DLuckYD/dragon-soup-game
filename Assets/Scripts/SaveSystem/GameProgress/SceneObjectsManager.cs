using System.Collections.Generic;
using UnityEngine;

public class SceneObjectsManager : MonoBehaviour
{
    public static SceneObjectsManager Instance { get; private set; }

    [Header("Items Database")]
    [SerializeField] private ItemDataBase itemDatabase;

    InventoryItem[] worldItems = FindObjectsByType<InventoryItem>(
        FindObjectsInactive.Include,
        FindObjectsSortMode.None
    );

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

    // GameObject spawnedObject = Instantiate(itemData.worldPrefab, position, rotation);

    // RewardItem rewardItem = spawnedObject.GetComponent<RewardItem>();

// if (rewardItem != null)
//{
   // rewardItem.InitializeAsRuntimeSpawnedItem();
//}

public SceneSaveData CaptureSaveData()
    {
        SceneSaveData saveData = new SceneSaveData();

        if (worldItems == null)
        {
            Debug.LogWarning("[HOTBAR SAVE] worldItems is NULL.");
            return saveData;
        }

        Debug.Log("[HOTBAR SAVE] worldItems count: " + worldItems.Length);

        for (int i = 0; i < worldItems.Length; i++)
        {
            InventoryItem item = worldItems[i];

            if (item == null || item.itemData == null || item.transform == null)
                continue;

            SceneItemSaveData itemSaveData = new SceneItemSaveData
            {
                //saveId = item,
                itemId = item.itemData.id,
                posX = item.transform.position.x,
                posY = item.transform.position.y,
                posZ = item.transform.position.z,
                rotX = item.transform.rotation.eulerAngles.x,
                rotY = item.transform.rotation.eulerAngles.y,
                rotZ = item.transform.rotation.eulerAngles.z
            };

            saveData.items.Add(itemSaveData);
        }

        Debug.Log("Captured scene save data with " + saveData.items.Count + " items.");

        return saveData;
    }

    public void RestoreSaveData(SceneSaveData saveData)
    {
        //ClearScene();

        if (saveData == null || saveData.items == null)
            return;

        foreach (SceneItemSaveData savedItem in saveData.items)
        {
            ItemData itemData = itemDatabase.GetItemById(savedItem.itemId);

            if (itemData == null)
            {
                Debug.LogWarning("Cannot restore scene item. Missing item id: " + savedItem.itemId);
                continue;
            }

            //AddItemToScene(savedItem);
        }
    }
}
