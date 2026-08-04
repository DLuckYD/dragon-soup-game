using UnityEngine;

public class DismantleTarget : MonoBehaviour
{
    [Header("Dismantle Settings")]
    [SerializeField] private DismantleRecipe recipe;
    [SerializeField] private float dismantleTime = 3f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 0.6f;
    [SerializeField] private float spawnUpOffset = 0.2f;

    public DismantleRecipe Recipe => recipe;
    public float DismantleTime => Mathf.Max(0.1f, dismantleTime);
    
    private Transform spawnCenter;

    public void Dismantle()
    {
        if (recipe == null)
        {
            Debug.LogWarning($"[DISMANTLE] {name} has no dismantle recipe.");
            return;
        }

        Vector3 centerPosition = spawnCenter != null ? spawnCenter.position : transform.position;

        foreach (DismantleOutput output in recipe.outputs)
        {
            if (output == null || output.itemData == null)
            {
                Debug.LogWarning($"[DISMANTLE] {name} has empty output in recipe.");
                continue;
            }

            if (output.itemData.worldPrefab == null)
            {
                Debug.LogWarning($"[DISMANTLE] Output item '{output.itemData.displayName}' has no world prefab.");
                continue;
            }

            int amount = Mathf.Max(1, output.amount);

            for (int i = 0; i < amount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

                Vector3 spawnPosition = centerPosition + new Vector3(
                    randomCircle.x,
                    spawnUpOffset,
                    randomCircle.y
                );

                Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                GameObject spawnedObject = Instantiate(
                    output.itemData.worldPrefab,
                    spawnPosition,
                    spawnRotation
                );

                InventoryItem inventoryItem = spawnedObject.GetComponent<InventoryItem>();

                if (inventoryItem != null)
                {
                    inventoryItem.itemData = output.itemData;
                    inventoryItem.isHeld = false;
                    inventoryItem.isInInventory = false;
                }

                Debug.Log($"[DISMANTLE] Spawned '{output.itemData.displayName}' from '{name}'.");
            }
        }

        Debug.Log($"[DISMANTLE] Destroyed original object: {name}");

        Destroy(gameObject);
    }
}