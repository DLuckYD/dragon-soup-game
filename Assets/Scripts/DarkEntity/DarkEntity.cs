using UnityEngine;
using System;
using System.Collections;

public class DarkEntity : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RecipeProgressManager recipeProgressManager;
    [SerializeField] private DarkEntityUI darkEntityUI;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float itemSpacing = 0.6f;
    [SerializeField] private int rowSize = 4;

    [Header("Delayed Spawn")]
    [SerializeField] private float spawnDelayBetweenItems = 0.15f;
    [SerializeField] private float spawnHeightOffset = 0.15f;

    [Header("State")]
    [SerializeField] private int darkEntityUsageCount = 0;
    [SerializeField] private bool isSpawning = false;

    public int DarkEntityUsageCount => darkEntityUsageCount;
    public bool IsSpawning => isSpawning;

    public static event Action<string> OnSpawned;

    private Coroutine spawnRoutine;

    private void Awake()
    {
        if (recipeProgressManager == null)
        {
            recipeProgressManager = FindObjectOfType<RecipeProgressManager>();
        }

        if (darkEntityUI == null)
        {
            darkEntityUI = FindObjectOfType<DarkEntityUI>();
        }
    }

    public void Interact()
    {
        if (isSpawning)
        {
            return; 
        }

        AkUnitySoundEngine.SetState("Game_State", "Dark_Entity");
        WwiseAudioManager.Instance.PostEvent("Dark_Entity_Interact", gameObject);

        if (recipeProgressManager == null)
        {
            Debug.LogWarning("[DARK ENTITY] RecipeProgressManager is missing.");
            return;
        }

        Recipe currentRecipe = recipeProgressManager.CurrentActiveRecipe;

        if (darkEntityUI != null)
        {
            darkEntityUI.Open(this, currentRecipe);
        }
        else
        {
            Debug.LogWarning("[DARK ENTITY] DarkEntityUI is missing.");
        }
    }

    public void AcceptDeal()
    {
        AkUnitySoundEngine.SetState("Game_State", "In_Game");
        WwiseAudioManager.Instance.PostEvent("Dark_Entity_Accept", gameObject);

        if (isSpawning)
        {
            Debug.Log("[DARK ENTITY] Cannot accept deal. Already spawning.");
            return;
        }

        if (recipeProgressManager == null)
        {
            Debug.LogWarning("[DARK ENTITY] Cannot accept deal. RecipeProgressManager is missing.");
            return;
        }

        Recipe currentRecipe = recipeProgressManager.CurrentActiveRecipe;

        if (!CanSpawnRecipe(currentRecipe))
        {
            return;
        }

        darkEntityUsageCount++;

        Debug.Log($"[DARK ENTITY] Deal accepted. Usage count: {darkEntityUsageCount}");

        spawnRoutine = StartCoroutine(SpawnRecipeIngredientsWithDelay(currentRecipe));
    }

    public void DeclineDeal()
    {
        AkUnitySoundEngine.SetState("Game_State", "In_Game");
        WwiseAudioManager.Instance.PostEvent("Dark_Entity_Decline", gameObject);

        Debug.Log("[DARK ENTITY] Deal declined.");
    }

    private bool CanSpawnRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("[DARK ENTITY] Cannot spawn items. Current active recipe is null.");
            return false;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("[DARK ENTITY] Spawn point is missing.");
            return false;
        }

        if (recipe.ingredients == null || recipe.ingredients.Count == 0)
        {
            Debug.LogWarning($"[DARK ENTITY] Recipe {recipe.displayName} has no ingredients.");
            return false;
        }

        return true;
    }

    private IEnumerator SpawnRecipeIngredientsWithDelay(Recipe recipe)
    {
        isSpawning = true;

        int spawnedIndex = 0;
        int totalSpawned = 0;

        foreach (IngredientAmount ingredient in recipe.ingredients)
        {
            if (ingredient == null)
            {
                continue;
            }

            if (ingredient.item == null)
            {
                Debug.LogWarning($"[DARK ENTITY] Ingredient in recipe {recipe.displayName} has no item.");
                continue;
            }

            if (ingredient.item.worldPrefab == null)
            {
                Debug.LogWarning($"[DARK ENTITY] Ingredient {ingredient.item.displayName} has no world prefab.");
                continue;
            }

            int amount = Mathf.Max(1, ingredient.amount);

            for (int i = 0; i < amount; i++)
            {
                Vector3 spawnPosition = GetSpawnPosition(spawnedIndex);

                Instantiate(
                    ingredient.item.worldPrefab,
                    spawnPosition,
                    spawnPoint.rotation
                );

                spawnedIndex++;
                totalSpawned++;

                if (spawnDelayBetweenItems > 0f)
                {
                    yield return new WaitForSecondsRealtime(spawnDelayBetweenItems);
                }
                else
                {
                    yield return null;
                }
            }
        }

        isSpawning = false;
        spawnRoutine = null;
        OnSpawned?.Invoke($"Dark Entity spawned ingredients for {recipe.displayName}");
        
    }

    private Vector3 GetSpawnPosition(int index)
    {
        int safeRowSize = Mathf.Max(1, rowSize);

        int x = index % safeRowSize;
        int z = index / safeRowSize;

        Vector3 offset = new Vector3(
            x * itemSpacing,
            spawnHeightOffset,
            z * itemSpacing
        );

        return spawnPoint.position + offset;
    }

    public void LoadUsageCount(int value)
    {
        darkEntityUsageCount = Mathf.Max(0, value);
        Debug.Log($"[DARK ENTITY] Usage count loaded: {darkEntityUsageCount}");
    }

    public int GetUsageCount()
    {
        return darkEntityUsageCount;
    }
}