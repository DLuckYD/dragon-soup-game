using UnityEngine;
using System;

public class DarkEntity : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RecipeProgressManager recipeProgressManager;
    [SerializeField] private DarkEntityUI darkEntityUI;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float itemSpacing = 0.6f;

    [Header("State")]
    [SerializeField] private int darkEntityUsageCount = 0;

    public int DarkEntityUsageCount => darkEntityUsageCount;
    
    public static event Action<string> OnSpawned;

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
        if (recipeProgressManager == null)
        {
            Debug.LogWarning("[DARK ENTITY] Cannot accept deal. RecipeProgressManager is missing.");
            return;
        }

        Recipe currentRecipe = recipeProgressManager.CurrentActiveRecipe;

        if (currentRecipe == null)
        {
            Debug.LogWarning("[DARK ENTITY] Cannot spawn items. Current active recipe is null.");
            return;
        }

        SpawnRecipeIngredients(currentRecipe);

        darkEntityUsageCount++;

        Debug.Log($"[DARK ENTITY] Deal accepted. Usage count: {darkEntityUsageCount}");
    }

    public void DeclineDeal()
    {
        Debug.Log("[DARK ENTITY] Deal declined.");
    }

    private void SpawnRecipeIngredients(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("[DARK ENTITY] Recipe is null.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("[DARK ENTITY] Spawn point is missing.");
            return;
        }

        if (recipe.ingredients == null || recipe.ingredients.Count == 0)
        {
            Debug.LogWarning($"[DARK ENTITY] Recipe {recipe.displayName} has no ingredients.");
            return;
        }

        int spawnedIndex = 0;

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
            }
        }
        OnSpawned?.Invoke("DARK ENTITY Spawned all ingredients for recipe!");
        Debug.Log($"[DARK ENTITY] Spawned all ingredients for recipe: {recipe.displayName}");
    }

    private Vector3 GetSpawnPosition(int index)
    {
        int rowSize = 4;

        int x = index % rowSize;
        int z = index / rowSize;

        Vector3 offset = new Vector3(x * itemSpacing, 0f, z * itemSpacing);

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